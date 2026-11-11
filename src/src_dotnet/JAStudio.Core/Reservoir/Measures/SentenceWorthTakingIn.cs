using System.Collections.Generic;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>A reservoir sentence picked as worth taking into the working set (<see cref="SentencesWorthTakingIn"/>), with what it gives the corpus.</summary>
/// <param name="SentenceId">The sentence's id in the reservoir.</param>
/// <param name="SourceId">The id in the reservoir of the source it's kept from.</param>
/// <param name="WhatItGives">What it exercises that the corpus lacks, such as <c>the vocab 仰る</c>, each as a line of text.</param>
public sealed record SentenceWorthTakingIn(long SentenceId, long SourceId, IReadOnlyList<string> WhatItGives);
