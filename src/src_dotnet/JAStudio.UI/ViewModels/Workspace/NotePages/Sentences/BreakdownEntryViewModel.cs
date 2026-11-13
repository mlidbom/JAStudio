using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.UI.Web.Sentence;
using JAStudio.Core.ViewModels.KanjiList;

namespace JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

/// <summary>
/// A word of a sentence's breakdown, as the breakdown lists it (<see cref="MatchViewModel"/>): the word as it's written in the sentence, linked to the<br/>
/// vocab it matched, if any, with the vocab's audio, form, readings, meta tags and answer; in edit mode, the reasons it doesn't match or isn't shown;<br/>
/// and below it, the compound's parts and the word's kanji, when the breakdown shows those.
/// </summary>
sealed class BreakdownEntryViewModel
{
   public BreakdownEntryViewModel(MatchViewModel match, Settings settings, SentenceKanjiListViewModel kanjiLists)
   {
      var vocab = match.VocabMatch?.Vocab;
      Reasons = settings.ShowBreakdownInEditMode()
                   ? [.. match.IncorrectReasons.Select(it => new MatchReasonViewModel(it, MakesTheMatchIncorrect: true)),
                      .. match.HidingReasons.Select(it => new MatchReasonViewModel(it, MakesTheMatchIncorrect: false))]
                   : [];
      Audio = AudioButtonViewModel.For(vocab?.PreferredAudio);
      Word = match.ParsedForm;
      WordLinkedToItsVocab = vocab is null ? null : new NoteLinkViewModel(match.ParsedForm, vocab.GetId());
      VocabForm = match.DisplayVocabForm ? match.VocabForm : null;
      Readings = match.DisplayReadings ? match.Readings : null;
      MetaTags = MetaTagViewModel.RowOf(match.MetaTags);
      Answer = match.Answer;
      IsHighlighted = match.IsHighlighted;
      IsStudyingReading = match.MetaTagsList.Contains("is_studying_reading");
      IsStudyingListening = match.MetaTagsList.Contains("is_studying_listening");
      CompoundParts = match.ShowCompoundParts ? [.. match.CompoundParts.Select(it => new CompoundPartEntryViewModel(it))] : [];
      Kanji = match.ShowKanji ? [.. kanjiLists.Create(match.Kanji).KanjiList.Select(it => new BreakdownKanjiViewModel(it, match.ShowKanjiMnemonics))] : [];
   }

   /// <summary>In edit mode, the reasons the word doesn't match the vocab, then those it isn't shown; none otherwise.</summary>
   public IReadOnlyList<MatchReasonViewModel> Reasons { get; }

   public AudioButtonViewModel? Audio { get; }

   /// <summary>The word as it's written in the sentence.</summary>
   public string Word { get; }

   /// <summary>The <see cref="Word"/>, linked to the vocab it matched; none when it matched no vocab, such as a word only the dictionary has.</summary>
   public NoteLinkViewModel? WordLinkedToItsVocab { get; }

   /// <summary>The form of the vocab that matched, unless it's the <see cref="Word"/> as written, such as 食べる for 食べ.</summary>
   public string? VocabForm { get; }

   /// <summary>The readings of what matched, unless they're what the word shows.</summary>
   public string? Readings { get; }

   public IReadOnlyList<MetaTagViewModel> MetaTags { get; }

   /// <summary>The answer of what matched, with its markup.</summary>
   public string Answer { get; }

   /// <summary>Whether the sentence highlights the word.</summary>
   public bool IsHighlighted { get; }

   /// <summary>Whether the vocab matched is being studied for reading, which a bar before the word shows.</summary>
   public bool IsStudyingReading { get; }

   /// <summary>Whether the vocab matched is being studied for listening, which a bar before the word shows.</summary>
   public bool IsStudyingListening { get; }

   public IReadOnlyList<CompoundPartEntryViewModel> CompoundParts { get; }

   public IReadOnlyList<BreakdownKanjiViewModel> Kanji { get; }
}
