using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>
/// What <see cref="MemoryTools.DescribeWhatHoldsTheObjectsOfAType"/> reports: the objects of one type on the managed heap, and the types of<br/>
/// the objects that reference them.
/// </summary>
/// <param name="Type">The type whose objects are held, as named in the call.</param>
/// <param name="Objects">How many objects of the type the heap holds.</param>
/// <param name="Megabytes">The megabytes they take.</param>
/// <param name="HolderTypesHoldingTheMost">
/// The types of the objects that reference them, the one whose objects reference the most megabytes of them first. An object referenced by<br/>
/// objects of two types is counted with each, so the megabytes can add up to more than <paramref name="Megabytes"/>.
/// </param>
/// <param name="ObjectsHeldByARootAlone">
/// How many of the objects no other object references: they're held by a static field, a thread's stack or a handle of the runtime's.
/// </param>
/// <param name="MegabytesHeldByARootAlone">The megabytes those take.</param>
/// <param name="ObjectsThatCouldNotBeRead">
/// How many objects the snapshot held that couldn't be read, left out of the rest. With thousands, the counts may be off by up to 1%.
/// </param>
public record WhatHoldsTheObjectsOfAType(
   string Type,
   long Objects,
   double Megabytes,
   IReadOnlyList<HolderType> HolderTypesHoldingTheMost,
   long ObjectsHeldByARootAlone,
   double MegabytesHeldByARootAlone,
   long ObjectsThatCouldNotBeRead);
