namespace JAStudio.Core.Reservoir.Picking;

/// <summary>How many features of one kind (<see cref="SentenceFeatureKind"/>) the candidate sentences give, and how many of them a picking left lacking.</summary>
/// <param name="Found">The features of the kind that the candidate sentences give, such as the vocab they match.</param>
/// <param name="StillLackingSentences">
/// Those of them the corpus still lacks sentences of after the picking: the reservoir has too few candidate sentences giving them, or the picking<br/>
/// stopped at its maximum first.
/// </param>
public record FeatureCount(int Found, int StillLackingSentences);
