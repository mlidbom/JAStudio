using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>What <see cref="MemoryTools.DescribeWhatTheHeapHolds"/> reports: the managed heap's objects, by type, and its strings.</summary>
/// <param name="Megabytes">The megabytes every object on the heap takes.</param>
/// <param name="Objects">How many objects the heap holds, those that could be read.</param>
/// <param name="ObjectsThatCouldNotBeRead">
/// How many objects the snapshot held that couldn't be read, left out of the rest. With thousands, the counts may be off by up to 1%.
/// </param>
/// <param name="TypesTakingTheMost">The types whose objects take the most, the one that takes the most first.</param>
/// <param name="Strings">The strings, and the values whose copies take the most memory.</param>
public record WhatTheHeapHolds(double Megabytes, long Objects, long ObjectsThatCouldNotBeRead, IReadOnlyList<HeapType> TypesTakingTheMost, StringsOnTheHeap Strings);
