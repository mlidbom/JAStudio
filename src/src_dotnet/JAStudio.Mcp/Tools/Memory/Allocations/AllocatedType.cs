namespace JAStudio.Mcp.Tools.Memory.Allocations;

/// <summary>A type an <see cref="AllocationSampler"/> sampled, with how much of what was allocated its objects took.</summary>
/// <param name="Type">The type's name, as the runtime reports it, such as <c>System.String</c> or <c>System.Collections.Generic.List`1[System.Int32]</c>.</param>
/// <param name="Megabytes">The megabytes the samples of the type cover.</param>
/// <param name="PercentOfTheSampled">Its share of all the megabytes sampled, in percent.</param>
public record AllocatedType(string Type, double Megabytes, double PercentOfTheSampled);
