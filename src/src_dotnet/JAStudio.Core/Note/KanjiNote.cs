using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Compze.Contracts;
using Compze.Internals.SystemCE;
using Compze.Teventive;
using Compze.Teventive.Taggregates;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Note.KanjiRelationships;
using JAStudio.Core.Note.KanjiTevents;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.Note;

public class KanjiNote : JPNote, ISnapshotable<KanjiNote, KanjiDto>, ITeventRouted
{
   static readonly TeventRouting<KanjiNote> Appliers = NoteAppliers.Extend<KanjiNote>(it => it
     .For<IKanjiTevent.Created>(static (kanji, tevent) => kanji._question = tevent.Question));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   /// <summary>
   /// The corpus's own answer for the kanji: the canonical one. While it's empty the source's answer is shown instead (<see cref="GetAnswer"/>).
   /// </summary>
   public WritableStringValue CorpusAnswer { get; private set; }

   /// <summary>The corpus's own mnemonic for the kanji. The card shows it, or while there's none, a default one: see <see cref="ShownMnemonic"/>.</summary>
   public WritableStringValue Mnemonic { get; private set; }

   /// <summary>What the kanji's source gave it when it was imported.</summary>
   public KanjiSourceData SourceData { get; private set; }

   /// <summary>The kanji's on, kun and nanori readings, each primary or secondary.</summary>
   public KanjiNoteReadings Readings { get; private set; }

   public WritableStringValue KanjiReferences { get; private set; }

   /// <summary>
   /// The kanji's images, which only a few have, all the corpus's own: pasted or from the web, as kanji have no source media. Shown nowhere yet: JAStudio<br/>
   /// only carries them, kept from the Anki notes before they were trimmed, since an image can't be made again the way the kanji's audio can be computed.
   /// </summary>
   public MediaReferenceList<ImageReference> Images { get; private set; }

   /// <summary>The radicals the kanji is made of, in their order.</summary>
   public KanjiRadicals Radicals { get; private set; }

   /// <summary>The vocab the kanji's page shows first among the vocab with the kanji. Most kanji have none, and show default ones: see <see cref="PrimaryVocabOrDefaults"/>.</summary>
   public KanjiPrimaryVocab PrimaryVocab { get; private set; }

   /// <summary>Other kanji with a similar meaning: a relationship that goes both ways.</summary>
   public MutualKanjiRelationship SimilarMeaning { get; private set; }

   /// <summary>Other kanji the kanji is easily confused with: a relationship that goes both ways.</summary>
   public MutualKanjiRelationship ConfusedWith { get; private set; }

   /// <summary>The kanji's character: what the note is about, so it never changes. A kanji with another character is another kanji.</summary>
   string _question;

   /// <summary>An empty kanji, which is no kanji yet: <see cref="RestoreFrom"/> makes it a stored one, and <see cref="KanjiTevent.Created"/> a new one.</summary>
   internal KanjiNote(NoteServices services) : base(services) => BuildThePartsFrom(null);

   /// <summary>A new kanji, for the character <paramref name="question"/>.</summary>
   public KanjiNote(NoteServices services, string question) : this(services) => Publish(new KanjiTevent.Created(KanjiId.New(), question));

   /// <summary>The kanji <paramref name="data"/> holds, as it was stored.</summary>
   public KanjiNote(NoteServices services, KanjiDto data) : this(services) => RestoreFrom(data);

   public KanjiDto TakeSnapshot() => KanjiNoteConverter.ToDto(this);

   public void RestoreFrom(KanjiDto snapshot)
   {
      RestoreTheNoteFrom(new KanjiId(snapshot.Id), snapshot);
      BuildThePartsFrom(snapshot);
   }

   /// <summary>Builds the parts that hold the kanji's stored data, from <paramref name="data"/>, or empty when it's null.</summary>
   [MemberNotNull(nameof(_question), nameof(CorpusAnswer), nameof(Mnemonic), nameof(SourceData), nameof(Readings), nameof(KanjiReferences), nameof(Images),
                  nameof(Radicals), nameof(PrimaryVocab), nameof(SimilarMeaning), nameof(ConfusedWith))]
   void BuildThePartsFrom(KanjiDto? data)
   {
      _question = data?.Kanji ?? string.Empty;
      CorpusAnswer = new WritableStringValue(data?.CorpusAnswer ?? string.Empty, Guard);
      Mnemonic = new WritableStringValue(data?.Mnemonic ?? string.Empty, Guard);
      SourceData = new KanjiSourceData(data?.SourceData, Guard);
      Readings = new KanjiNoteReadings(data?.Readings, Guard);
      KanjiReferences = new WritableStringValue(data?.References ?? string.Empty, Guard);
      Images = new MediaReferenceList<ImageReference>(data?.Images.Select(it => it.ToReference()) ?? [], Guard);
      Radicals = new KanjiRadicals(this, data?.Radicals ?? [], Guard);
      PrimaryVocab = new KanjiPrimaryVocab(this, data?.PrimaryVocab.Select(WordReference.FromDto) ?? [], Guard);
      SimilarMeaning = new MutualKanjiRelationship(this, data?.SimilarMeaning ?? [], Guard, it => it.SimilarMeaning);
      ConfusedWith = new MutualKanjiRelationship(this, data?.ConfusedWith ?? [], Guard, it => it.ConfusedWith);
   }


   public override CorpusNoteDto ToDto() => TakeSnapshot();

   protected override Type WrapperTeventImplementation => typeof(KanjiTevent<IKanjiTevent>);

   protected override Dictionary<string, string> StudyNoteFieldsBesidesTheId() => new() { [AnkiFieldNames.Question] = GetQuestion() };

   public override string GetQuestion() => _question;

   /// <summary>Whether <paramref name="character"/> is the kanji's own character: the one other notes name it by, as its radicals and related kanji do.</summary>
   public bool IsNamedBy(string character) => character == GetQuestion();

   /// <summary>The answer shown: the <see cref="CorpusAnswer"/>, or the source's answer while the corpus has none.</summary>
   public override string GetAnswer() => CorpusAnswer.HasValue() ? CorpusAnswer.Value : SourceData.Answer.Value;

   /// <summary>
   /// Makes the source's answer the corpus's own (<see cref="CorpusAnswer"/>), written as the corpus writes a kanji's answers: in lower case, the<br/>
   /// meanings separated by " | " where the source had markup or commas, and the words of each joined by dashes. Only while the kanji has no answer<br/>
   /// of its own.
   /// </summary>
   public void AcceptMeaning()
   {
      Contract.State.Assert(!CorpusAnswer.HasValue(), () => $"The kanji {GetQuestion()} has an answer of its own already: {CorpusAnswer.Value}");
      CorpusAnswer.Set(WrittenAsTheCorpusWritesAKanjisAnswers(SourceData.Answer.Value));

      static string WrittenAsTheCorpusWritesAKanjisAnswers(string sourceAnswer) =>
         sourceAnswer.Replace("<", "|")
                     .Replace(">", "|")
                     .Replace("[", "|")
                     .Replace("]", "|")
                     .ToLowerInvariant()
                     .Replace("||", "|")
                     .Replace("||", "|")
                     .Replace("||", "|")
                     .Replace(", ", "|")
                     .Replace(" ", "-")
                     .Replace("-|-", " | ")
                     .Trim('|');
   }

   public string AnswerText => StringExtensions.StripHtmlMarkup(GetAnswer());

   /// <summary>The question and answer, and the readings in kana and in romaji.</summary>
   public override IEnumerable<string> SearchableTexts()
   {
      var readings = Readings.AllKana();
      return [..base.SearchableTexts(), ..readings, ..readings.Select(KanaUtils.Romanize)];
   }

   /// <summary>
   /// The vocab the kanji's page shows first, and marks as primary: its <see cref="PrimaryVocab"/>, or while it has none, by default, for each of its<br/>
   /// primary readings, of the vocab studied for reading whose reading has it where the kanji is, the one with the most studied sentences.
   /// </summary>
   public List<VocabNote> PrimaryVocabOrDefaults => PrimaryVocab.Words.Count > 0 ? PrimaryVocab.Notes() : DefaultPrimaryVocab();

   List<VocabNote> DefaultPrimaryVocab()
   {
      var result = new List<VocabNote>();

      var studyingReadingVocabInDescendingStudyingSentencesOrder = GetVocabNotes()
                                                                  .Where(v => v.IsStudying(CardTypes.Reading))
                                                                  .OrderByDescending(v => v.Sentences.Studying().Count)
                                                                  .ThenBy(v => v.GetQuestion().Length)
                                                                  .ToList();

      var primaryReadings = Readings.PrimaryKana().Distinct().ToList();

      foreach(var primaryReading in primaryReadings)
      {
         foreach(var vocab in studyingReadingVocabInDescendingStudyingSentencesOrder)
         {
            var readings = vocab.GetReadings();
            if(readings.Count > 0 && ReadingInVocabReading(primaryReading, readings[0], vocab.GetQuestion()))
            {
               result.Add(vocab);
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

   public List<string> TagVocabReadings(VocabNote vocab)
   {
      string PrimaryReading(string read) => $"<span class=\"kanjiReadingPrimary\">{read}</span>";
      string SecondaryReading(string read) => $"<span class=\"kanjiReadingSecondary\">{read}</span>";

      var primaryReadings = Readings.PrimaryKana();
      var secondaryReadings = Readings.AllKana()
                             .Where(reading => !primaryReadings.Contains(reading))
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

      var coveringReadings = Readings.AllKana()
                            .Where(r => kanjiReading != r && r.Contains(kanjiReading))
                            .ToList();

      if(coveringReadings.Any(coveringReading => ReadingInVocabReading(coveringReading, vocabReading, vocabForm)))
      {
         return false;
      }

      if(vocabForm.StartsWithOrdinal(GetQuestion()))
      {
         return vocabReading.StartsWithOrdinal(kanjiReading);
      }

      if(vocabForm.EndsWithOrdinal(GetQuestion()))
      {
         return vocabReading.EndsWithOrdinal(kanjiReading);
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

   public List<VocabNote> GetVocabNotes() => Services.Collection.Vocab.WithKanjiInAnyForm(this);

   public override IEnumerable<MediaReference> MediaReferences => Images.All;

   public List<VocabNote> GetVocabNotesSorted() =>
      VocabNoteSorting.SortVocabListByStudyingStatus(
         GetVocabNotes(),
         PrimaryVocabOrDefaults,
         preferredKanji: GetQuestion());

   public void BootstrapMnemonicFromRadicals()
   {
      Mnemonic.Set(Services.KanjiNoteMnemonicMaker.CreateDefaultMnemonic(this));
   }

   /// <summary>
   /// Adds to the radicals the kanji the mnemonic names in its <c>&lt;rad&gt;</c> tags, in the order it names them, those it hasn't already: each<br/>
   /// kanji whose answer has a tag's text as a word of its own.
   /// </summary>
   public void PopulateRadicalsFromMnemonicTags()
   {
      foreach(var radical in DetectRadicalsFromMnemonic().Where(it => !Radicals.Contains(it)))
         Radicals.Add(radical);

      List<string> DetectRadicalsFromMnemonic()
      {
         var radicalNames = Regex.Matches(Mnemonic.Value, "<rad>(.*?)</rad>")
                                 .Select(m => m.Groups[1].Value)
                                 .ToList();

         var everyKanji = Services.Collection.Kanji.All();
         return
         [
            .. radicalNames.SelectMany(name => everyKanji.Where(kanji => KanjiAnswerContainsRadicalNameAsASeparateWord(name, kanji)))
                           .Select(kanji => kanji.GetQuestion())
                           .Distinct()
         ];

         static bool KanjiAnswerContainsRadicalNameAsASeparateWord(string radicalName, KanjiNote kanji) => Regex.IsMatch(kanji.GetAnswer(), @"\b" + Regex.Escape(radicalName) + @"\b");
      }
   }

   /// <summary>Creates a kanji with no readings and adds it to the collection.</summary>
   public static KanjiNote Create(NoteServices services, string question, string answer)
   {
      var note = new KanjiNote(services, question);
      note.CorpusAnswer.Set(answer);
      services.Collection.Kanji.Add(note);
      return note;
   }

   /// <summary>
   /// Creates a kanji by hand, as the note menus' "Create" does: one of our own, such as a radical or a component, tagged<br/>
   /// <see cref="Tagging.Tags.Source.Jastudio"/>. See <see cref="Create"/>.
   /// </summary>
   public static KanjiNote CreateByHand(NoteServices services, string question, string answer)
   {
      var note = Create(services, question, answer);
      note.Edit(it => it.Tags.Set(Tagging.Tags.Source.Jastudio));
      return note.Reference().Current;
   }
}
