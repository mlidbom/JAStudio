using JAStudio.Mcp.Tools.Analysis;

namespace JAStudio.Mcp.Tools.Memory.Allocations;

/// <summary>What <see cref="AnalysisTools.SampleWhatAnalyzingEverySentenceAllocates"/> reports: what analyzing every sentence of the corpus allocated.</summary>
/// <param name="Sentences">How many sentences were analyzed.</param>
/// <param name="Threads">How many threads they were analyzed on.</param>
/// <param name="AllocatedMegabytes">The megabytes the process allocated during the pass, counted exactly.</param>
/// <param name="Sampled">What the sampling found: the types that took the most of it.</param>
public record WhatAnalyzingEverySentenceAllocates(int Sentences, int Threads, double AllocatedMegabytes, SampledAllocations Sampled);
