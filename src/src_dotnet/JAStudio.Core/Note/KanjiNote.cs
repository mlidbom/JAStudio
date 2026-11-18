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
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.SysUtils;

namespace JAStudio.Core.Note;

public class KanjiNote : JPNote, ISnapshotable<KanjiNote, KanjiDto>
{
   static readonly TeventRouting<KanjiNote, INoteTevent> Appliers = NoteAppliers.Extend<KanjiNote>(it => it
     .For<IKanjiTevent.Created>(static (kanji, tevent) => kanji._question = tevent.Question)
     .For<IKanjiTevent.PropertyUpdated.CorpusAnswer>(static (kanji, tevent) => kanji.CorpusAnswer = tevent.CorpusAnswer)
     .For<IKanjiTevent.PropertyUpdated.Mnemonic>(static (kanji, tevent) => kanji.Mnemonic = tevent.Mnemonic)
     .For<IKanjiTevent.PropertyUpdated.KanjiReferences>(static (kanji, tevent) => kanji.KanjiReferences = tevent.KanjiReferences)
     .For<IKanjiTevent.PropertyUpdated.Radicals>(static (kanji, tevent) => kanji.Radicals.Apply(tevent.Radicals))
     .For<IKanjiTevent.PropertyUpdated.PrimaryVocab>(static (kanji, tevent) => kanji.PrimaryVocab.Apply(tevent.PrimaryVocab))
     .For<IKanjiTevent.PropertyUpdated.SimilarMeaning>(static (kanji, tevent) => kanji.SimilarMeaning.Apply(tevent.SimilarMeaning))
     .For<IKanjiTevent.PropertyUpdated.ConfusedWith>(static (kanji, tevent) => kanji.ConfusedWith.Apply(tevent.ConfusedWith))
     .For<IKanjiTevent.PropertyUpdated.Images>(static (kanji, tevent) => kanji.Images.Apply(tevent.Images))
     .Component(static kanji => kanji.SourceData)
     .Component(static kanji => kanji.Readings));

   protected override TeventRouting TeventAppliers => Appliers;

   /// <summary>
   /// The corpus's own answer for the kanji: the canonical one. While it's empty the source's answer is shown instead (<see cref="GetAnswer"/>).
   /// </summary>
   public string CorpusAnswer { get; private set; }

   /// <summary>The corpus's own mnemonic for the kanji. The card shows it, or while there's none, a default one: see <see cref="ShownMnemonic"/>.</summary>
   public string Mnemonic { get; private set; }

   /// <summary>Gives the kanji <paramref name="answer"/>, trimmed, as its <see cref="CorpusAnswer"/>.</summary>
   public void SetCorpusAnswer(string answer) => PublishIfChanged(CorpusAnswer, answer.Trim(), static it => new KanjiTevent.CorpusAnswerUpdated(it));

   /// <summary>Gives the kanji <paramref name="mnemonic"/>, trimmed, as its <see cref="Mnemonic"/>.</summary>
   public void SetMnemonic(string mnemonic) => PublishIfChanged(Mnemonic, mnemonic.Trim(), static it => new KanjiTevent.MnemonicUpdated(it));

   /// <summary>What the kanji's source gave it when it was imported.</summary>
   public KanjiSourceData SourceData { get; private set; }

   /// <summary>The kanji's on, kun and nanori readings, each primary or secondary.</summary>
   public KanjiNoteReadings Readings { get; private set; }

   /// <summary>The corpus's references for the kanji, such as where its mnemonic comes from.</summary>
   public string KanjiReferences { get; private set; }

   /// <summary>Gives the kanji <paramref name="kanjiReferences"/>, trimmed, as its <see cref="KanjiReferences"/>.</summary>
   public void SetKanjiReferences(string kanjiReferences) =>
      PublishIfChanged(KanjiReferences, kanjiReferences.Trim(), static it => new KanjiTevent.KanjiReferencesUpdated(it));

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
      CorpusAnswer = data?.CorpusAnswer ?? string.Empty;
      Mnemonic = data?.Mnemonic ?? string.Empty;
      SourceData = new KanjiSourceData(this, data?.SourceData);
      Readings = new KanjiNoteReadings(this, data?.Readings);
      KanjiReferences = data?.References ?? string.Empty;
      Images = new MediaReferenceList<ImageReference>(this, data?.Images.Select(it => it.ToReference()) ?? [], static it => new KanjiTevent.ImagesUpdated(it));
      Radicals = new KanjiRadicals(this, data?.Radicals ?? []);
      PrimaryVocab = new KanjiPrimaryVocab(this, data?.PrimaryVocab.Select(WordReference.FromDto) ?? []);
      SimilarMeaning = MutualKanjiRelationship.SimilarMeaningOf(this, data?.SimilarMeaning ?? []);
      ConfusedWith = MutualKanjiRelationship.ConfusedWithOf(this, data?.ConfusedWith ?? []);
   }


   public override CorpusNoteDto ToDto() => TakeSnapshot();

   internal override void Delete() => Publish(new KanjiTevent.Deleted());

   protected override Type WrapperTeventImplementation => typeof(KanjiTevent<IKanjiTevent>);

   protected override Dictionary<string, string> StudyNoteFieldsBesidesTheId() => new() { [AnkiFieldNames.Question] = GetQuestion() };

   public override string GetQuestion() => _question;

   /// <summary>Whether <paramref name="character"/> is the kanji's own character: the one other notes name it by, as its radicals and related kanji do.</summary>
   public bool IsNamedBy(string character) => character == GetQuestion();

   /// <summary>The answer shown: the <see cref="CorpusAnswer"/>, or the source's answer while the corpus has none.</summary>
   public override string GetAnswer() => CorpusAnswer is not "" ? CorpusAnswer : SourceData.Answer;

   /// <summary>
   /// Makes the source's answer the corpus's own (<see cref="CorpusAnswer"/>), written as the corpus writes a kanji's answers: in lower case, the<br/>
   /// meanings separated by " | " where the source had markup or commas, and the words of each joined by dashes. Only while the kanji has no answer<br/>
   /// of its own.
   /// </summary>
   public void AcceptMeaning()
   {
      Contract.State.Assert(CorpusAnswer is "", () => $"The kanji {GetQuestion()} has an answer of its own already: {CorpusAnswer}");
      SetCorpusAnswer(WrittenAsTheCorpusWritesAKanjisAnswers(SourceData.Answer));

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
   public string ShownMnemonic => Mnemonic is not "" ? Mnemonic : $"# {Services.KanjiNoteMnemonicMaker.CreateDefaultMnemonic(this)}";

   public List<VocabNote> GetVocabNotes() => Services.Collection.Vocab.WithKanjiInAnyForm(this);

   public override IEnumerable<MediaReference> MediaReferences => Images.All;

   public List<VocabNote> GetVocabNotesSorted() =>
      VocabNoteSorting.SortVocabListByStudyingStatus(
         GetVocabNotes(),
         PrimaryVocabOrDefaults,
         preferredKanji: GetQuestion());

   public void BootstrapMnemonicFromRadicals()
   {
      SetMnemonic(Services.KanjiNoteMnemonicMaker.CreateDefaultMnemonic(this));
   }

   /// <summary>Creates a kanji with no readings and adds it to the collection.</summary>
   public static KanjiNote Create(NoteServices services, string question, string answer)
   {
      var note = new KanjiNote(services, question);
      note.SetCorpusAnswer(answer);
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
