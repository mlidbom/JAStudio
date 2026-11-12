using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Memory;

/// <summary>What an <see cref="AllocationSampler"/> sampled: how much it covers, and the types that took the most.</summary>
/// <param name="SampledMegabytes">The megabytes the samples cover, of every type: about all the process allocated while it sampled.</param>
/// <param name="TypesThatTookTheMost">The types that took the most, the one that took the most first.</param>
public record SampledAllocations(double SampledMegabytes, IReadOnlyList<AllocatedType> TypesThatTookTheMost);
