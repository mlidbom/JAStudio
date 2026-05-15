using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.Note;

public class KanjiNote : JPNote
{
   static readonly Regex PrimaryReadingPattern = new("<primary>(.*?)</primary>", RegexOptions.Compiled);

   public WritableStringValue SourceAnswer { get; }
   public WritableStringValue UserAnswer { get; }
   public WritableStringValue ActiveAnswer { get; }
   public WritableStringValue MeaningInfo { get; }
   public WritableStringValue ReadingMnemonic { get; }
   public WritableStringValue ReadingInfo { get; }
   public WritableStringValue ReadingOnHtml { get; }
   public WritableStringValue ReadingKunHtml { get; }
   public WritableStringValue ReadingNanHtml { get; }
   public WritableStringValue SourceMeaningMnemonic { get; }
   public WritableStringValue UserMnemonic { get; }
   public WritableStringValue PrimaryReadingsTtsAudio { get; }
   public WritableStringValue KanjiReferences { get; }
   public WritableAudioValue Audio { get; }
   public WritableImageValue Image { get; }

   readonly WritableStringValue _question;
   readonly WritableStringValue _radicalsRaw;
   readonly WritableStringValue _primaryVocabRaw;
   readonly WritableStringValue _similarMeaningRaw;
   readonly WritableStringValue _confusedWithRaw;

   public KanjiNote(NoteServices services, KanjiData? data = null) : base(services, data != null ? new KanjiId(data.Id) : KanjiId.New(), data?.Tags)
   {
      _question = new WritableStringValue(data?.Kanji ?? string.Empty, Guard);
      SourceAnswer = new WritableStringValue(data?.SourceAnswer ?? string.Empty, Guard);
      UserAnswer = new WritableStringValue(data?.UserAnswer ?? string.Empty, Guard);
      ActiveAnswer = new WritableStringValue(data?.ActiveAnswer ?? string.Empty, Guard);
      MeaningInfo = new WritableStringValue(data?.MeaningInfo ?? string.Empty, Guard);
      ReadingMnemonic = new WritableStringValue(data?.ReadingMnemonic ?? string.Empty, Guard);
      ReadingInfo = new WritableStringValue(data?.ReadingInfo ?? string.Empty, Guard);
      ReadingOnHtml = new WritableStringValue(data?.ReadingOnHtml ?? string.Empty, Guard);
      ReadingKunHtml = new WritableStringValue(data?.ReadingKunHtml ?? string.Empty, Guard);
      ReadingNanHtml = new WritableStringValue(data?.ReadingNanHtml ?? string.Empty, Guard);
      SourceMeaningMnemonic = new WritableStringValue(data?.SourceMeaningMnemonic ?? string.Empty, Guard);
      UserMnemonic = new WritableStringValue(data?.UserMnemonic ?? string.Empty, Guard);
      PrimaryReadingsTtsAudio = new WritableStringValue(data?.PrimaryReadingsTtsAudio ?? string.Empty, Guard);
      KanjiReferences = new WritableStringValue(data?.References ?? string.Empty, Guard);
      Audio = new WritableAudioValue(data?.Audio ?? string.Empty, Guard);
      _radicalsRaw = new WritableStringValue(data?.Radicals != null ? string.Join(", ", data.Radicals) : string.Empty, Guard);
      _primaryVocabRaw = new WritableStringValue(data?.PrimaryVocab != null ? string.Join(", ", data.PrimaryVocab) : string.Empty, Guard);
      _similarMeaningRaw = new WritableStringValue(data?.SimilarMeaning != null ? string.Join(", ", data.SimilarMeaning) : string.Empty, Guard);
      _confusedWithRaw = new WritableStringValue(data?.ConfusedWith != null ? string.Join(", ", data.ConfusedWith) : string.Empty, Guard);
      Image = new WritableImageValue(data?.Image ?? string.Empty, Guard);
   }

   public override void UpdateInCache()
   {
      Services.Collection.Kanji.Cache.JpNoteUpdated(this);
   }

   public override CorpusObjectData ToCorpusData() => KanjiNoteConverter.ToCorpusData(this);

   public override string GetQuestion() => _question.Value;

   public void SetQuestion(string value) => _question.Set(value);

   public override string GetAnswer()
   {
      var userAnswer = UserAnswer.Value;
      return !string.IsNullOrEmpty(userAnswer) ? userAnswer : SourceAnswer.Value;
   }

   public string AnswerText => StringExtensions.StripHtmlMarkup(GetAnswer());

   public override void UpdateGeneratedData()
   {
      base.UpdateGeneratedData();

      // Katakana sneaks in via yomitan etc
      ReadingOnHtml.Set(KanaUtils.KatakanaToHiragana(ReadingOnHtml.Value));

      void UpdatePrimaryAudios()
      {
         var vocabWeShouldPlay = PrimaryVocab
                                .SelectMany(question => Services.Collection.Vocab.WithQuestion(question))
                                .ToList();

         var audioString = vocabWeShouldPlay.Count > 0
                              ? string.Join("", vocabWeShouldPlay.Select(vo => vo.Audio.PrimaryAudio))
                              : string.Empty;

         Audio.SetRawValue(audioString);
      }

      ActiveAnswer.Set(GetAnswer());
      UpdatePrimaryAudios();
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
         return allReadings.Select(StringExtensions.StripHtmlMarkup).ToList();
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
         return matches.Select(m => StringExtensions.StripHtmlMarkup(m.Groups[1].Value)).ToList();
      }
   }

   public List<string> PrimaryReadingsKun
   {
      get
      {
         var matches = PrimaryReadingPattern.Matches(ReadingKunHtml.Value);
         return matches.Select(m => StringExtensions.StripHtmlMarkup(m.Groups[1].Value)).ToList();
      }
   }

   public List<string> PrimaryReadingsNan
   {
      get
      {
         var matches = PrimaryReadingPattern.Matches(ReadingNanHtml.Value);
         return matches.Select(m => StringExtensions.StripHtmlMarkup(m.Groups[1].Value)).ToList();
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
         return AllRadicals.Where(r => r != question).ToList();
      }
   }

   public List<string> AllRadicals => StringExtensions.ExtractCommaSeparatedValues(_radicalsRaw.Value);

   public void SetRadicals(string value) => _radicalsRaw.Set(value);

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

   public List<string> UserSimilarMeaning => StringExtensions.ExtractCommaSeparatedValues(_similarMeaningRaw.Value);

   public void AddUserSimilarMeaning(string newSynonymQuestion, bool isRecursiveCall = false)
   {
      var nearSynonymsQuestions = UserSimilarMeaning;
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
            newSynonym.AddUserSimilarMeaning(GetQuestion(), isRecursiveCall: true);
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
      return Radicals
            .Select(radical => Services.Collection.Kanji.WithKanji(radical))
            .Where(k => k != null)
            .Cast<KanjiNote>()
            .ToList();
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

   public string ActiveMnemonic
   {
      get
      {
         if(!string.IsNullOrEmpty(UserMnemonic.Value))
         {
            return UserMnemonic.Value;
         }

         if(Services.Config.PreferDefaultMnemonicsToSourceMnemonics.Value)
         {
            return $"# {Services.KanjiNoteMnemonicMaker.CreateDefaultMnemonic(this)}";
         }

         return SourceMeaningMnemonic.Value;
      }
   }

   public string UserSimilarMeaningRaw
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

   public override List<MediaReference> MediaReferences
   {
      get
      {
         var refs = Audio.GetMediaReferences();
         refs.AddRange(Image.GetMediaReferences());
         return refs;
      }
   }

   public IReadOnlyList<AudioAttachment> AudioAttachments => Services.MediaFileIndex.GetAudioAttachments(Audio.RawValue());
   public AudioAttachment?               PreferredAudio   => AudioAttachments.FirstOrDefault();
   public IReadOnlyList<ImageAttachment> Images           => Services.MediaFileIndex.GetImageAttachments(Image.RawValue());

   public List<VocabNote> GetVocabNotesSorted() =>
      VocabNoteSorting.SortVocabListByStudyingStatus(
         GetVocabNotes(),
         PrimaryVocabsOrDefaults,
         preferredKanji: GetQuestion());

   public void BootstrapMnemonicFromRadicals()
   {
      UserMnemonic.Set(Services.KanjiNoteMnemonicMaker.CreateDefaultMnemonic(this));
   }

   public void PopulateRadicalsFromMnemonicTags()
   {
      List<string> DetectRadicalsFromMnemonic()
      {
         var radicalNames = Regex.Matches(UserMnemonic.Value, "<rad>(.*?)</rad>")
                                 .Select(m => m.Groups[1].Value)
                                 .ToList();

         bool KanjiAnswerContainsRadicalNameAsASeparateWord(string radicalName, KanjiNote kanji) => Regex.IsMatch(kanji.GetAnswer(), @"\b" + Regex.Escape(radicalName) + @"\b");

         bool KanjiAnswerContainsAnyRadicalNameAsASeparateWord(KanjiNote kanji)
         {
            return radicalNames.Any(name => KanjiAnswerContainsRadicalNameAsASeparateWord(name, kanji));
         }

         return Services.Collection.Kanji.All()
                        .Where(KanjiAnswerContainsAnyRadicalNameAsASeparateWord)
                        .Select(kanji => kanji.GetQuestion())
                        .ToList();
      }

      var radicals = Radicals;
      foreach(var radical in DetectRadicalsFromMnemonic())
      {
         if(!radicals.Contains(radical))
         {
            radicals.Add(radical);
         }
      }

      SetRadicals(string.Join(", ", radicals));
   }

   public static KanjiNote Create(NoteServices services, string question, string answer, string onReadings, string kunReading)
   {
      var note = new KanjiNote(services);
      note.SetQuestion(question);
      note.UserAnswer.Set(answer);
      note.ReadingOnHtml.Set(onReadings);
      note.ReadingKunHtml.Set(kunReading);
      services.Collection.Kanji.Add(note);
      return note;
   }
}
