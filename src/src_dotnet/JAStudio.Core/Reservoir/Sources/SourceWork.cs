using System.Collections.Generic;

namespace JAStudio.Core.Reservoir.Sources;

/// <summary>A work to add to the reservoir: its <see cref="ReservoirSource"/>, and its sentences, read as they're enumerated.</summary>
public sealed record SourceWork(ReservoirSource Source, IEnumerable<SourceSentence> Sentences);
