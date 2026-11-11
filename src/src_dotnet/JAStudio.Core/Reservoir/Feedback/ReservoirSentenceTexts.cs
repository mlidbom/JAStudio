using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Reservoir.Feedback;

/// <summary>
/// The text of every sentence in the reservoir, held in memory, to find in a second or two those that contain any of some texts, such as the<br/>
/// texts a vocab's sentences contain one of (<see cref="Note.Vocabulary.VocabNote.TextsASentenceItCanMatchInContainsOneOf"/>). Millions of<br/>
/// sentences take a few hundred MB, and a few tens of seconds to load.
/// </summary>
public sealed class ReservoirSentenceTexts
{
   readonly ReservoirSentence[] _sentences;

   ReservoirSentenceTexts(ReservoirSentence[] sentences) => _sentences = sentences;

   public static ReservoirSentenceTexts LoadedFrom(ReservoirFile reservoir) => new([.. reservoir.EverySentence()]);

   /// <summary>How many sentences the reservoir held when they were loaded.</summary>
   public int Count => _sentences.Length;

   /// <summary>Every sentence containing one or more of <paramref name="texts"/>, in the order they were added to the reservoir.</summary>
   public IReadOnlyList<ReservoirSentence> ContainingAnyOf(IReadOnlyCollection<string> texts) =>
      [.. _sentences.AsParallel().AsOrdered().Where(sentence => texts.Any(text => sentence.Text.Contains(text, StringComparison.Ordinal)))];
}
