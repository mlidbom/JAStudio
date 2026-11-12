using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>
/// A count of what holds the objects of one type on this process's managed heap: for each type of object that references them, how many<br/>
/// of its objects do, how many of the type's objects they reference, and what those take. For telling what keeps the objects of a type,<br/>
/// such as the strings, in memory. Made from a <see cref="HeapSnapshot"/>.
/// </summary>
/// <remarks>
/// Only the objects that reference one directly count, not what references those: a list's strings are held by the list's array, so the<br/>
/// array's type is the holder counted. A census of what holds the array's type then tells what holds the arrays, though not only the arrays<br/>
/// that hold those strings. An object referenced by objects of two types is counted with each. An object no other object references is<br/>
/// held by a root alone: a static field, a thread's stack, or a handle of the runtime's.
/// </remarks>
sealed class HolderCensus
{
   readonly string _typeHeld;
   readonly Dictionary<ulong, long> _bytesOfEachObjectOfTheType = [];
   readonly HashSet<ulong> _objectsOfTheTypeReferencedByAnObject = [];
   readonly Dictionary<string, HolderTally> _holderTypes = new(StringComparer.Ordinal);
   long _objectsThatCouldNotBeRead;

   HolderCensus(string typeHeld) => _typeHeld = typeHeld;

   /// <summary>
   /// What holds the objects of the type named <paramref name="typeHeld"/> on this process's managed heap, after collecting all garbage, as<br/>
   /// <see cref="MemoryTools.DescribeWhatHoldsTheObjectsOfAType"/> reports it.
   /// </summary>
   public static WhatHoldsTheObjectsOfAType DescribedForThisProcessAfterCollectingAllGarbage(string typeHeld, int maxHolderTypesListed) =>
      HeapSnapshot.ReadAfterCollectingAllGarbage(snapshot => Of(snapshot, typeHeld).Described(maxHolderTypesListed));

   static HolderCensus Of(HeapSnapshot snapshot, string typeHeld)
   {
      var census = new HolderCensus(typeHeld);
      census.FindTheObjectsOfTheType(snapshot);
      census.CountTheObjectsThatReferenceThem(snapshot);
      return census;
   }

   void FindTheObjectsOfTheType(HeapSnapshot snapshot)
   {
      foreach(var found in snapshot.Objects())
      {
         if(!found.CanBeRead) _objectsThatCouldNotBeRead++;
         else if(found.TypeName == _typeHeld) _bytesOfEachObjectOfTheType.Add(found.Object.Address, (long)found.Object.Size);
      }
   }

   void CountTheObjectsThatReferenceThem(HeapSnapshot snapshot)
   {
      foreach(var holder in snapshot.Objects().Where(it => it is { CanBeRead: true, Object.ContainsPointers: true }))
      foreach(var referencedAddress in holder.Object.EnumerateReferenceAddresses(carefully: true, considerDependantHandles: false))
      {
         if(!_bytesOfEachObjectOfTheType.TryGetValue(referencedAddress, out var bytesOfTheObjectHeld)) continue;

         TallyOf(holder.TypeName).Count(holder.Object.Address, referencedAddress, bytesOfTheObjectHeld);
         _objectsOfTheTypeReferencedByAnObject.Add(referencedAddress);
      }
   }

   WhatHoldsTheObjectsOfAType Described(int maxHolderTypesListed)
   {
      var bytesOfTheType = _bytesOfEachObjectOfTheType.Values.Sum();
      var heldByARootAlone = _bytesOfEachObjectOfTheType.Where(it => !_objectsOfTheTypeReferencedByAnObject.Contains(it.Key)).ToList();
      return new WhatHoldsTheObjectsOfAType(_typeHeld,
                                            _bytesOfEachObjectOfTheType.Count,
                                            Megabytes.Of(bytesOfTheType),
                                            [.. _holderTypes.OrderByDescending(it => it.Value.BytesHeld)
                                                            .Take(maxHolderTypesListed)
                                                            .Select(it => new HolderType(it.Key,
                                                                                         it.Value.Holders,
                                                                                         it.Value.ObjectsHeld,
                                                                                         Megabytes.Of(it.Value.BytesHeld),
                                                                                         PercentOf(it.Value.BytesHeld)))],
                                            heldByARootAlone.Count,
                                            Megabytes.Of(heldByARootAlone.Sum(it => it.Value)),
                                            _objectsThatCouldNotBeRead);

      double PercentOf(long bytes) => bytesOfTheType == 0 ? 0 : Math.Round(100.0 * bytes / bytesOfTheType, 2);
   }

   HolderTally TallyOf(string holderType)
   {
      ref var tally = ref CollectionsMarshal.GetValueRefOrAddDefault(_holderTypes, holderType, out _);
      return tally ??= new HolderTally();
   }

   /// <summary>The objects of one type that reference objects of the type held, and the objects of it they reference.</summary>
   sealed class HolderTally
   {
      readonly HashSet<ulong> _objectsHeld = [];
      ulong _lastHolderCounted;

      /// <summary>How many objects of the holder type reference at least one object of the type held.</summary>
      public long Holders { get; private set; }

      /// <summary>How many objects of the type held the holders reference, each counted once however many of them reference it.</summary>
      public long ObjectsHeld => _objectsHeld.Count;

      /// <summary>The bytes the <see cref="ObjectsHeld"/> take.</summary>
      public long BytesHeld { get; private set; }

      /// <summary>
      /// Counts that the holder at <paramref name="holderAddress"/> references the object at <paramref name="heldAddress"/>. A holder's<br/>
      /// references are counted one after the other, so a holder is new when it isn't the last one counted.
      /// </summary>
      public void Count(ulong holderAddress, ulong heldAddress, long bytesOfTheObjectHeld)
      {
         if(holderAddress != _lastHolderCounted) Holders++;
         _lastHolderCounted = holderAddress;
         if(_objectsHeld.Add(heldAddress)) BytesHeld += bytesOfTheObjectHeld;
      }
   }
}
