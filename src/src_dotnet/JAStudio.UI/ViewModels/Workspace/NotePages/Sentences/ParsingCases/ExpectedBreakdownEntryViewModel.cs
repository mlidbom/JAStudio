using System;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences.ParsingCases;

/// <summary>
/// A word a sentence's parsing case expects the breakdown to display (<see cref="BreakdownWord"/>), as the page shows it beside the breakdown the<br/>
/// sentence displays now: the word as written, linked to the vocab it was matched to, with the vocab's form, readings and answer as they are now.
/// </summary>
/// <remarks>
/// A parsing case holds the vocab by its id alone, so the vocab's question, readings and answer are the vocab's of today. A vocab deleted since the<br/>
/// case was recorded leaves only the word as written, and says that its vocab is gone.
/// </remarks>
sealed class ExpectedBreakdownEntryViewModel
{
   public ExpectedBreakdownEntryViewModel(BreakdownWord expected, JPCollection collection)
   {
      var vocab = expected.VocabId is {} vocabId ? collection.Vocab.WithIdOrNone(new VocabId(Guid.Parse(vocabId))) : null;

      Word = expected.Form;
      WordLinkedToItsVocab = vocab is null ? null : new NoteLinkViewModel(expected.Form, vocab.GetId());
      FormShownBesideTheWord = expected.ShownAs;
      Readings = vocab is null ? null : string.Join(", ", vocab.GetReadings());
      Answer = vocab?.GetAnswer() ?? (expected.VocabId != null ? "(its vocab is no longer in the corpus)" : "");
   }

   /// <summary>The word as it's written in the sentence.</summary>
   public string Word { get; }

   /// <summary>The <see cref="Word"/>, linked to the vocab it was matched to; none when it was matched to no vocab, or the vocab is gone.</summary>
   public NoteLinkViewModel? WordLinkedToItsVocab { get; }

   /// <summary>The form the breakdown showed beside the word, when that wasn't the word as written, such as 食べる for 食べ.</summary>
   public string? FormShownBesideTheWord { get; }

   /// <summary>The readings of the vocab the word was matched to, as they are now; none for a word matched to no vocab.</summary>
   public string? Readings { get; }

   /// <summary>The answer of the vocab the word was matched to, as it is now, with its markup; empty for a word matched to no vocab.</summary>
   public string Answer { get; }
}
