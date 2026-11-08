using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.Note;

public class KanjiNote : JPNote
{
   static readonly Regex PrimaryReadingPattern = new("<primary>(.*?)</primary>", RegexOptions.Compiled);

   /// <summary>
   /// The corpus's own answer for the kanji: the canonical one. While it's empty the source's answer is shown instead (<see cref="GetAnswer"/>).
   /// </summary>
   public WritableStringValue CorpusAnswer { get; }

   /// <summary>The corpus's own mnemonic for the kanji. The card shows it, or while there's none, a default one: see <see cref="ShownMnemonic"/>.</summary>
   public WritableStringValue Mnemonic { get; }

   /// <summary>What the kanji's source gave it when it was imported.</summary>
   public KanjiSourceData SourceData { get; }

   public WritableStringValue ReadingOnHtml { get; }
   public WritableStringValue ReadingKunHtml { get; }
   public WritableStringValue ReadingNanHtml { get; }
   public WritableStringValue KanjiReferences { get; }

   /// <summary>An image for the kanji, which only a few have: see <see cref="KanjiDto.Image"/>.</summary>
   public WritableImageValue Image { get; }

   readonly WritableStringValue _question;
   readonly WritableStringValue _radicalsRaw;
   readonly WritableStringValue _primaryVocabRaw;
   readonly WritableStringValue _similarMeaningRaw;
   readonly WritableStringValue _confusedWithRaw;

   public KanjiNote(NoteServices services, KanjiDto? data = null) : base(services, data != null ? new KanjiId(data.Id) : KanjiId.New(), data?.Tags)
   {
      _question = new WritableStringValue(data?.Kanji ?? string.Empty, Guard);
      CorpusAnswer = new WritableStringValue(data?.UserAnswer ?? string.Empty, Guard);
      Mnemonic = new WritableStringValue(data?.UserMnemonic ?? string.Empty, Guard);
      SourceData = new KanjiSourceData(data, Guard);
      ReadingOnHtml = new WritableStringValue(data?.ReadingOnHtml ?? string.Empty, Guard);
      ReadingKunHtml = new WritableStringValue(data?.ReadingKunHtml ?? string.Empty, Guard);
      ReadingNanHtml = new WritableStringValue(data?.ReadingNanHtml ?? string.Empty, Guard);
      KanjiReferences = new WritableStringValue(data?.References ?? string.Empty, Guard);
      Image = new WritableImageValue(data?.Image ?? string.Empty, Guard);
      _radicalsRaw = new WritableStringValue(data?.Radicals != null ? string.Join(", ", data.Radicals) : string.Empty, Guard);
      _primaryVocabRaw = new WritableStringValue(data?.PrimaryVocab != null ? string.Join(", ", data.PrimaryVocab) : string.Empty, Guard);
      _similarMeaningRaw = new WritableStringValue(data?.SimilarMeaning != null ? string.Join(", ", data.SimilarMeaning) : string.Empty, Guard);
      _confusedWithRaw = new WritableStringValue(data?.ConfusedWith != null ? string.Join(", ", data.ConfusedWith) : string.Empty, Guard);
   }

   public override void UpdateInCache()
   {
      Services.Collection.Kanji.Cache.JpNoteUpdated(this);
   }

   public override CorpusNoteDto ToDto() => KanjiNoteConverter.ToDto(this);

   public override string GetQuestion() => _question.Value;

   public void SetQuestion(string value) => _question.Set(value);

   /// <summary>The answer shown: the <see cref="CorpusAnswer"/>, or the source's answer while the corpus has none.</summary>
   public override string GetAnswer() => CorpusAnswer.HasValue() ? CorpusAnswer.Value : SourceData.Answer.Value;

   public string AnswerText => StringExtensions.StripHtmlMarkup(GetAnswer());

   /// <summary>The question and answer, and the readings in kana and in romaji.</summary>
   public override IEnumerable<string> SearchableTexts()
   {
      var readings = ReadingsClean;
      return [..base.SearchableTexts(), ..readings, ..readings.Select(KanaUtils.Romanize)];
   }

   public override void UpdateGeneratedData()
   {
      base.UpdateGeneratedData();

      // Katakana sneaks in via yomitan etc
      ReadingOnHtml.Set(KanaUtils.KatakanaToHiragana(ReadingOnHtml.Value));
   }

   public List<string> ReadingsOn => StringExtensions.ExtractCommaSeparatedValues(
      StringExtensions.StripHtmlMarkup(ReadingOnHtml.Value));

   public List<string> ReadingOnListHtml => StringExtensions.ExtractCommaSeparatedValues(ReadingOnHtml.Value);

   public List<string> ReadingsKun => StringExtensions.ExtractCommaSeparatedValues(
      StringExtensions.StripHtmlMarkup(ReadingKunHtml.Value));

   public List<string> ReadingKunListHtml => StringExtensions.ExtractCommaSeparatedValues(ReadingKunHtml.Value);

   public List<string> ReadingNanListHtml => StringExtensions.ExtractCommaSeparatedValues(ReadingNanHtml.Value);

   public List<string> ReadingsClean
   {
      get
      {
         var allReadings = new List<string>();
         allReadings.AddRange(ReadingOnListHtml);
         allReadings.AddRange(ReadingKunListHtml);
         allReadings.AddRange(ReadingNanListHtml);
         return [.. allReadings.Select(StringExtensions.StripHtmlMarkup)];
      }
   }

   public List<string> PrimaryReadings
   {
      get
      {
         var readings = new List<string>();
         readings.AddRange(PrimaryReadingsOn);
         readings.AddRange(PrimaryReadingsKun);
         readings.AddRange(PrimaryReadingsNan);
         return readings;
      }
   }

   public List<string> PrimaryReadingsOn
   {
      get
      {
         var matches = PrimaryReadingPattern.Matches(ReadingOnHtml.Value);
         return [.. matches.Select(m => StringExtensions.StripHtmlMarkup(m.Groups[1].Value))];
      }
   }

   public List<string> PrimaryReadingsKun
   {
      get
      {
         var matches = PrimaryReadingPattern.Matches(ReadingKunHtml.Value);
         return [.. matches.Select(m => StringExtensions.StripHtmlMarkup(m.Groups[1].Value))];
      }
   }

   public List<string> PrimaryReadingsNan
   {
      get
      {
         var matches = PrimaryReadingPattern.Matches(ReadingNanHtml.Value);
         return [.. matches.Select(m => StringExtensions.StripHtmlMarkup(m.Groups[1].Value))];
      }
   }

   public void AddPrimaryOnReading(string reading)
   {
      ReadingOnHtml.Set(StringExtensions.ReplaceWord(reading, $"<primary>{reading}</primary>", ReadingOnHtml.Value));
   }

   public void RemovePrimaryOnReading(string reading)
   {
      ReadingOnHtml.Set(ReadingOnHtml.Value.Replace($"<primary>{reading}</primary>", reading));
   }

   public void AddPrimaryKunReading(string reading)
   {
      ReadingKunHtml.Set(StringExtensions.ReplaceWord(reading, $"<primary>{reading}</primary>", ReadingKunHtml.Value));
   }

   public void RemovePrimaryKunReading(string reading)
   {
      ReadingKunHtml.Set(ReadingKunHtml.Value.Replace($"<primary>{reading}</primary>", reading));
   }

   public List<string> Radicals
   {
      get
      {
         var question = GetQuestion();
         return [.. AllRadicals.Where(r => r != question)];
      }
   }

   public List<string> AllRadicals => StringExtensions.ExtractCommaSeparatedValues(_radicalsRaw.Value);

   /// <summary>The kanji's radicals as the note stores them: comma separated, exactly as typed. <see cref="AllRadicals"/> is them as a list.</summary>
   public string RadicalsRaw
   {
      get => _radicalsRaw.Value;
      set => _radicalsRaw.Set(value);
   }

   public void PositionPrimaryVocab(string vocab, int newIndex = -1)
   {
      vocab = vocab.Trim();
      var primaryVocabList = PrimaryVocab;

      if(primaryVocabList.Contains(vocab))
      {
         primaryVocabList.Remove(vocab);
      }

      if(newIndex == -1)
      {
         primaryVocabList.Add(vocab);
      } else
      {
         primaryVocabList.Insert(newIndex, vocab);
      }

      PrimaryVocab = primaryVocabList;
   }

   public void RemovePrimaryVocab(string vocab)
   {
      var primaryVocabList = PrimaryVocab;
      primaryVocabList.RemoveAll(v => v == vocab);
      PrimaryVocab = primaryVocabList;
   }

   public List<string> SimilarMeaning => StringExtensions.ExtractCommaSeparatedValues(_similarMeaningRaw.Value);

   public void AddSimilarMeaning(string newSynonymQuestion, bool isRecursiveCall = false)
   {
      var nearSynonymsQuestions = SimilarMeaning;
      if(!nearSynonymsQuestions.Contains(newSynonymQuestion))
      {
         nearSynonymsQuestions.Add(newSynonymQuestion);
      }

      _similarMeaningRaw.Set(string.Join(", ", nearSynonymsQuestions));

      if(!isRecursiveCall)
      {
         var newSynonym = Services.Collection.Kanji.WithKanji(newSynonymQuestion);
         if(newSynonym != null)
         {
            newSynonym.AddSimilarMeaning(GetQuestion(), isRecursiveCall: true);
         }
      }
   }

   public List<string> RelatedConfusedWith => StringExtensions.ExtractCommaSeparatedValues(_confusedWithRaw.Value);

   public void AddRelatedConfusedWith(string newConfusedWith)
   {
      var confusedWith = RelatedConfusedWith;
      if(!confusedWith.Contains(newConfusedWith))
      {
         confusedWith.Add(newConfusedWith);
      }

      _confusedWithRaw.Set(string.Join(", ", confusedWith));
   }

   public List<string> PrimaryVocabsOrDefaults
   {
      get
      {
         var primaryVocab = PrimaryVocab;
         return primaryVocab.Count > 0 ? primaryVocab : GenerateDefaultPrimaryVocab();
      }
   }

   public List<string> PrimaryVocab
   {
      get => StringExtensions.ExtractCommaSeparatedValues(_primaryVocabRaw.Value);
      set => _primaryVocabRaw.Set(string.Join(", ", value));
   }

   /// <summary>The kanji's primary vocab as the note stores them: comma separated, exactly as typed. <see cref="PrimaryVocab"/> is them as a list.</summary>
   public string PrimaryVocabRaw
   {
      get => _primaryVocabRaw.Value;
      set => _primaryVocabRaw.Set(value);
   }

   public List<string> GenerateDefaultPrimaryVocab()
   {
      var result = new List<string>();

      var studyingReadingVocabInDescendingStudyingSentencesOrder = GetVocabNotes()
                                                                  .Where(v => v.IsStudying(CardTypes.Reading))
                                                                  .OrderByDescending(v => v.Sentences.Studying().Count)
                                                                  .ThenBy(v => v.GetQuestion().Length)
                                                                  .ToList();

      var primaryReadings = PrimaryReadings.Distinct().ToList();

      foreach(var primaryReading in primaryReadings)
      {
         foreach(var vocab in studyingReadingVocabInDescendingStudyingSentencesOrder)
         {
            var readings = vocab.GetReadings();
            if(readings.Count > 0 && ReadingInVocabReading(primaryReading, readings[0], vocab.GetQuestion()))
            {
               result.Add(vocab.GetQuestion());
               break;
            }
         }
      }

      return result;
   }

   static readonly Regex AnyWordPattern = new(@"\b[-\w]+\b", RegexOptions.Compiled);
   static readonly Regex ParenthesizedWordPattern = new(@"\([-\w]+\)", RegexOptions.Compiled);

   public string PrimaryMeaning
   {
      get
      {
         var radicalMeaningMatch = AnyWordPattern.Match(AnswerText.Replace("{", "").Replace("}", ""));
         return radicalMeaningMatch.Success ? radicalMeaningMatch.Groups[0].Value : "";
      }
   }

   public string PrimaryRadicalMeaning
   {
      get
      {
         string GetDedicatedRadicalPrimaryMeaning()
         {
            var radicalMeaningMatch = ParenthesizedWordPattern.Match(AnswerText);
            return radicalMeaningMatch.Success
                      ? radicalMeaningMatch.Groups[0].Value.Replace("(", "").Replace(")", "")
                      : "";
         }

         var result = GetDedicatedRadicalPrimaryMeaning();
         return !string.IsNullOrEmpty(result) ? result : PrimaryMeaning;
      }
   }

   public List<KanjiNote> GetRadicalsNotes()
   {
      return
      [
         .. Radicals
           .Select(radical => Services.Collection.Kanji.WithKanji(radical))
           .Where(k => k != null)
           .Cast<KanjiNote>()
      ];
   }

   public List<string> TagVocabReadings(VocabNote vocab)
   {
      string PrimaryReading(string read) => $"<span class=\"kanjiReadingPrimary\">{read}</span>";
      string SecondaryReading(string read) => $"<span class=\"kanjiReadingSecondary\">{read}</span>";

      var primaryReadings = PrimaryReadings;
      var secondaryReadings = ReadingsClean
                             .Where(reading => !primaryReadings.Contains(reading) && !string.IsNullOrEmpty(reading))
                             .ToList();

      var result = new List<string>();
      var vocabForm = vocab.GetQuestion();

      foreach(var vocabReading in vocab.GetReadings())
      {
         var found = false;

         foreach(var kanjiReading in primaryReadings)
         {
            if(ReadingInVocabReading(kanjiReading, vocabReading, vocabForm))
            {
               result.Add(vocabReading.Replace(kanjiReading, PrimaryReading(kanjiReading)));
               found = true;
               break;
            }
         }

         if(!found)
         {
            foreach(var kanjiReading in secondaryReadings)
            {
               if(ReadingInVocabReading(kanjiReading, vocabReading, vocabForm))
               {
                  result.Add(vocabReading.Replace(kanjiReading, SecondaryReading(kanjiReading)));
                  found = true;
                  break;
               }
            }

            if(!found)
            {
               result.Add(vocabReading);
            }
         }
      }

      return result;
   }

   public bool ReadingInVocabReading(string kanjiReading, string vocabReading, string vocabForm)
   {
      vocabForm = vocabForm.StripHtmlAndBracketMarkupAndNoiseCharacters();

      var coveringReadings = ReadingsClean
                            .Where(r => kanjiReading != r && r.Contains(kanjiReading))
                            .ToList();

      if(coveringReadings.Any(coveringReading => ReadingInVocabReading(coveringReading, vocabReading, vocabForm)))
      {
         return false;
      }

      if(vocabForm.StartsWith(GetQuestion()))
      {
         return vocabReading.StartsWith(kanjiReading);
      }

      if(vocabForm.EndsWith(GetQuestion()))
      {
         return vocabReading.EndsWith(kanjiReading);
      }

      return vocabReading.Length >= 2
                ? vocabReading.Substring(1, vocabReading.Length - 2).Contains(kanjiReading)
                : kanjiReading == "";
   }

   /// <summary>
   /// The mnemonic the kanji's card shows: the corpus's own <see cref="Mnemonic"/>, or while there's none, a default one made from the kanji's readings<br/>
   /// (<see cref="KanjiNoteMnemonicMaker"/>). The source's mnemonics are never shown.
   /// </summary>
   public string ShownMnemonic => Mnemonic.HasValue() ? Mnemonic.Value : $"# {Services.KanjiNoteMnemonicMaker.CreateDefaultMnemonic(this)}";

   public string SimilarMeaningRaw
   {
      get => _similarMeaningRaw.Value;
      set => _similarMeaningRaw.Set(value);
   }

   public string RelatedConfusedWithRaw
   {
      get => _confusedWithRaw.Value;
      set => _confusedWithRaw.Set(value);
   }

   public List<VocabNote> GetVocabNotes() => Services.Collection.Vocab.WithKanjiInAnyForm(this);

   public override List<MediaReference> MediaReferences => Image.GetMediaReferences();

   public List<VocabNote> GetVocabNotesSorted() =>
      VocabNoteSorting.SortVocabListByStudyingStatus(
         GetVocabNotes(),
         PrimaryVocabsOrDefaults,
         preferredKanji: GetQuestion());

   public void BootstrapMnemonicFromRadicals()
   {
      Mnemonic.Set(Services.KanjiNoteMnemonicMaker.CreateDefaultMnemonic(this));
   }

   public void PopulateRadicalsFromMnemonicTags()
   {
      List<string> DetectRadicalsFromMnemonic()
      {
         var radicalNames = Regex.Matches(Mnemonic.Value, "<rad>(.*?)</rad>")
                                 .Select(m => m.Groups[1].Value)
                                 .ToList();

         bool KanjiAnswerContainsRadicalNameAsASeparateWord(string radicalName, KanjiNote kanji) => Regex.IsMatch(kanji.GetAnswer(), @"\b" + Regex.Escape(radicalName) + @"\b");

         bool KanjiAnswerContainsAnyRadicalNameAsASeparateWord(KanjiNote kanji)
         {
            return radicalNames.Any(name => KanjiAnswerContainsRadicalNameAsASeparateWord(name, kanji));
         }

         return
         [
            .. Services.Collection.Kanji.All()
                       .Where(KanjiAnswerContainsAnyRadicalNameAsASeparateWord)
                       .Select(kanji => kanji.GetQuestion())
         ];
      }

      var radicals = Radicals;
      foreach(var radical in DetectRadicalsFromMnemonic())
      {
         if(!radicals.Contains(radical))
         {
            radicals.Add(radical);
         }
      }

      RadicalsRaw = string.Join(", ", radicals);
   }

   public static KanjiNote Create(NoteServices services, string question, string answer, string onReadings, string kunReading)
   {
      var note = new KanjiNote(services);
      note.SetQuestion(question);
      note.CorpusAnswer.Set(answer);
      note.ReadingOnHtml.Set(onReadings);
      note.ReadingKunHtml.Set(kunReading);
      services.Collection.Kanji.Add(note);
      return note;
   }
}
