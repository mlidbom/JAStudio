using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>The strings on the managed heap, and the memory their copies take: strings of the same value, which could be one string.</summary>
/// <param name="Strings">How many strings the heap holds.</param>
/// <param name="Megabytes">The megabytes they take.</param>
/// <param name="DistinctValues">How many different values they have, among those compared: strings up to 4096 characters long.</param>
/// <param name="MegabytesInCopies">The megabytes the copies take: for each value, all of its strings but one.</param>
/// <param name="RepeatedStringsTakingTheMost">The values whose copies take the most, the one whose copies take the most first.</param>
public record StringsOnTheHeap(long Strings, double Megabytes, long DistinctValues, double MegabytesInCopies, IReadOnlyList<RepeatedString> RepeatedStringsTakingTheMost);
