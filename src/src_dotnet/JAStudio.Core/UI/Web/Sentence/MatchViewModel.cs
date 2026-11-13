using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.UI.Web.Sentence;

public class MatchViewModel
{
   readonly Settings _settings;
   readonly List<string> _metaTags;

   public Match Match { get; }
   public bool MatchIsDisplayed { get; }
   public VocabMatch? VocabMatch { get; }
   public CandidateWordVariantViewModel WordVariantVm { get; }
   public bool IsDisplayWord { get; }
   public string ParsedForm { get; }
   public string Answer { get; }
   public string VocabForm { get; }
   public List<CompoundPartViewModel> CompoundParts { get; }
   public bool IsHighlighted { get; }
   public string Readings { get; }
   public string MetaTagsHtml { get; }

   /// <summary>The meta tags of the vocab matched, as <see cref="MetaTagsHtml"/> shows them; none for a match of no vocab.</summary>
   public IReadOnlyList<VocabMetaTag> MetaTags { get; }
   public bool DisplayVocabForm { get; }
   public bool MatchOwnsForm { get; }
   public bool DisplayReadings { get; }

   public MatchViewModel(CandidateWordVariantViewModel wordVariantVm, Match match, Settings settings, VocabCollection vocab)
   {
      Match = match;
      _settings = settings;
      MatchIsDisplayed = match.IsDisplayed;
      VocabMatch = match as VocabMatch;
      var config = wordVariantVm.CandidateWord.Word.Analysis.Configuration;
      WordVariantVm = wordVariantVm;
      IsDisplayWord = wordVariantVm.IsDisplayWord;
      ParsedForm = match.ParsedForm;
      Answer = match.Answer;
      VocabForm = !settings.ShowBreakdownInEditMode() ? match.MatchForm : match.ExclusionForm;
      CompoundParts = [];
      IsHighlighted = config.HighlightedWords.Contains(ParsedForm) || config.HighlightedWords.Contains(VocabForm);
      Readings = string.Join(", ", match.Readings);
      MetaTagsHtml = string.Empty;
      MetaTags = [];
      _metaTags = [];
      DisplayVocabForm = ParsedForm != VocabForm;
      MatchOwnsForm = ParsedForm == VocabForm;
      DisplayReadings = ParsedForm != Readings;

      if(VocabMatch != null)
      {
         CompoundParts = CompoundPartViewModel.GetCompoundPartsRecursive(this, VocabMatch.Vocab, config, settings, vocab);
         _metaTags = VocabMatch.Vocab.GetMetaTags().ToList();
         MetaTagsHtml = VocabMatch.Vocab.MetaData.MetaTagsHtml(displayExtendedSentenceStatistics: false);
         MetaTags = VocabMatch.Vocab.MetaData.MetaTags(displayExtendedSentenceStatistics: false);
         MatchOwnsForm = VocabMatch.Vocab.Forms.IsOwnedForm(ParsedForm);
         if(ParsedForm != VocabForm)
         {
            DisplayVocabForm = true;
            DisplayReadings = DisplayReadings && VocabForm != Readings;
         }
      }
   }

   public string MetaTagsString => string.Join(" ", MetaTagsList);

   public List<string> MetaTagsList
   {
      get
      {
         var result = new List<string>(_metaTags);
         if(IsHighlighted) result.Add("highlighted");
         result.AddRange(HidingReasons);
         return result;
      }
   }

   public List<string> IncorrectReasons => Match.FailureReasons.ToList();

   public List<string> HidingReasons => Match.HidingReasons.ToList();

   public bool IsDisplayed
   {
      get
      {
         if(_settings.ShowBreakdownInEditMode()) return true;
         return IsDisplayWord && Match.IsDisplayed;
      }
   }

   public bool ShowKanji => Kanji.Any() && !_settings.ShowBreakdownInEditMode() && _settings.ShowKanjiInSentenceBreakdown();

   public bool ShowKanjiMnemonics => _settings.ShowKanjiMnemonicsInSentenceBreakdown();

   public bool ShowCompoundParts => CompoundParts.Any() && !_settings.ShowBreakdownInEditMode();

   public List<string> Kanji
   {
      get
      {
         var kanjiChars = KanaUtils.ExtractKanji(ParsedForm + VocabForm);
         return kanjiChars.Distinct().ToList();
      }
   }

   public override string ToString()
   {
      var flags = new List<string> { ParsedForm };
      if(IsDisplayWord) flags.Add("is_display_word");
      if(IsDisplayed) flags.Add("displayed");
      return string.Join(" ", flags);
   }
}
