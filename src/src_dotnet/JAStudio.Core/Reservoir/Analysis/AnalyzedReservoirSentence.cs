using System;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>A reservoir sentence with its analysis, as the measures over the reservoir read them.</summary>
/// <param name="Id">The sentence's id in the reservoir.</param>
/// <param name="SourceId">The id in the reservoir of the source it's kept from.</param>
/// <param name="Occurrences">How often it was found, in every source.</param>
/// <param name="HasAudio">Whether the source it's kept from has its audio, as a deck does.</param>
public sealed record AnalyzedReservoirSentence(long Id, string Text, long SourceId, int Occurrences, bool HasAudio, ReservoirSentenceAnalysis Analysis)
{
   /// <summary>The length of a short spoken line, read at a glance: the length <see cref="WorthAsAnExample"/> prefers.</summary>
   const int ReadableLength = 20;

   /// <summary>How good the sentence is as an example: one with audio can be studied, and one of a short spoken line's length is read at a glance.</summary>
   public double WorthAsAnExample => (HasAudio ? 100 : 0) - Math.Abs(Text.Length - ReadableLength);
}
