using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>What <see cref="SharedConditionTools"/> found of the conditions several vocab's match-conditions hold: how many, and the first of them.</summary>
/// <param name="Found">How many conditions were found, every one counted, listed or not.</param>
/// <param name="Listed">The first of them: those held by the most vocab first, then the largest.</param>
public sealed record FoundSharedConditionCandidates(int Found, IReadOnlyList<ListedSharedConditionCandidate> Listed);
