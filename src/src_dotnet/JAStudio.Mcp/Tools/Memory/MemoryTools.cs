using System;
using System.ComponentModel;
using JAStudio.Mcp.Tools.Memory.Heap;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>The tools about the memory of the JAStudio process, for telling what a change, such as to the garbage collector's mode, costs in memory.</summary>
[McpServerToolType]
public class MemoryTools
{
   [McpServerTool(Name = "describe_memory", Idempotent = false)]
   [Description("""
                Describes the memory of the JAStudio process: the garbage collector's mode, workstation or server, and every setting of it; how
                much the managed heap holds and how much the garbage collector has committed; and the process's working set and private memory.
                With alsoAfterCollectingGarbage, it then runs the most thorough collection there is, a blocking, compacting, aggressive one of
                every generation that gives the operating system back all the memory it can, and describes the memory again: what the process
                keeps when it must give back all it can, as against what it held only because nothing made it collect.
                """)]
   public static MemoryDescription DescribeMemory(
      [Description("True to also collect all garbage and describe the memory after it.")] bool alsoAfterCollectingGarbage = false)
   {
      var memoryUse = MemoryUse.Now();
      return new MemoryDescription(TheGarbageCollector.Mode,
                                   GC.GetConfigurationVariables(),
                                   memoryUse,
                                   alsoAfterCollectingGarbage ? MemoryUseAfterCollectingAllGarbage() : null);
   }

   [McpServerTool(Name = "describe_what_the_heap_holds", ReadOnly = true, Idempotent = false)]
   [Description("""
                Describes what the JAStudio process's managed heap holds, after collecting all garbage, as describe_memory does: every object
                counted from a snapshot of the process, as dotnet-dump's dumpheap -stat and -strings count a dump's. Reports the heap's
                megabytes and objects; the types whose objects take the most, each with its objects, megabytes and share of the heap; and the
                strings, with how many distinct values they have, the megabytes their copies take, strings of the same value that could be
                one string, and the values whose copies take the most. An object's megabytes are its own, not what it references. For telling
                what the notes take in memory, and what a change to them saves: compare a call before and after. Takes about half a minute
                on the whole corpus, as the process goes on. A snapshot can catch parts of the heap that can't be read, as
                objectsThatCouldNotBeRead tells: with thousands, the counts may be off by up to 1%. On the headless JAStudio, it read every
                object.
                """)]
   public static WhatTheHeapHolds DescribeWhatTheHeapHolds(
      [Description("The most types to list, the one whose objects take the most first.")] int maxTypesListed = 40,
      [Description("""
                   Lists only the types whose name contains this, ignoring case, such as JAStudio or Sentence. The heap's totals stay the
                   whole heap's. All types if none.
                   """)] string? typesNamed = null,
      [Description("The most repeated string values to list, the one whose copies take the most first.")] int maxRepeatedStringsListed = 20) =>
      HeapCensus.DescribedForThisProcessAfterCollectingAllGarbage(maxTypesListed, typesNamed, maxRepeatedStringsListed);

   [McpServerTool(Name = "describe_what_holds_the_objects_of_a_type", ReadOnly = true, Idempotent = false)]
   [Description("""
                Describes what holds the objects of one type on the JAStudio process's managed heap, after collecting all garbage, from a
                snapshot as describe_what_the_heap_holds takes: how many objects of the type the heap holds and their megabytes, and, for each
                type of object that references them, how many of its objects do, how many of the type's objects they reference, and those
                objects' megabytes. Also how many no other object references, held by a static field, a thread's stack or a handle of the
                runtime's alone. Only the objects that reference one directly count: a list's strings are held by its array, System.String[];
                ask again about the array's type to learn what holds the arrays, though not only those arrays. An object referenced by objects
                of two types counts with each. For telling what keeps the objects of a type that takes much memory alive, such as the strings.
                Takes about a minute on the whole corpus.
                """)]
   public static WhatHoldsTheObjectsOfAType DescribeWhatHoldsTheObjectsOfAType(
      [Description("""
                   The type's full name, exactly as describe_what_the_heap_holds lists it, such as System.String, System.Byte[] or
                   System.Collections.Generic.HashSet<System.String>+Entry[].
                   """)] string type,
      [Description("The most holder types to list, the one whose objects reference the most megabytes of the type's first.")] int maxHolderTypesListed = 30) =>
      HolderCensus.DescribedForThisProcessAfterCollectingAllGarbage(type, maxHolderTypesListed);

   static MemoryUse MemoryUseAfterCollectingAllGarbage()
   {
      TheGarbageCollector.CollectAllGarbage();
      return MemoryUse.Now();
   }
}
