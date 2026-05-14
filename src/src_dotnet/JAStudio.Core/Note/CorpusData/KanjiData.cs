using System.Collections.Generic;
using JAStudio.Core.Anki;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

[MemoryPackable]
public partial class KanjiData : CorpusObjectData
{
   public string Kanji { get; init; } = string.Empty;
   public string SourceAnswer { get; init; } = string.Empty;
   public string UserAnswer { get; init; } = string.Empty;
   public string ActiveAnswer { get; init; } = string.Empty;
   public string ReadingOnHtml { get; init; } = string.Empty;
   public string ReadingKunHtml { get; init; } = string.Empty;
   public string ReadingNanHtml { get; init; } = string.Empty;
   public List<string> Radicals { get; init; } = [];
   public string SourceMeaningMnemonic { get; init; } = string.Empty;
   public string MeaningInfo { get; init; } = string.Empty;
   public string ReadingMnemonic { get; init; } = string.Empty;
   public string ReadingInfo { get; init; } = string.Empty;
   public List<string> PrimaryVocab { get; init; } = [];
   public string Audio { get; init; } = string.Empty;
   public string PrimaryReadingsTtsAudio { get; init; } = string.Empty;
   public string References { get; init; } = string.Empty;
   public string UserMnemonic { get; init; } = string.Empty;
   public List<string> SimilarMeaning { get; init; } = [];
   public List<string> ConfusedWith { get; init; } = [];
   public string Image { get; init; } = string.Empty;

   protected override NoteId CreateTypedId() => new KanjiId(Id);

   protected override void PopulateAnkiNoteFields(Dictionary<string, string> fields)
   {
      fields[NoteFieldsConstants.Kanji.Question] = Kanji;
      fields[NoteFieldsConstants.Kanji.SourceAnswer] = SourceAnswer;
      fields[NoteFieldsConstants.Kanji.UserAnswer] = UserAnswer;
      fields[NoteFieldsConstants.Kanji.ActiveAnswer] = ActiveAnswer;
      fields[NoteFieldsConstants.Kanji.ReadingOn] = ReadingOnHtml;
      fields[NoteFieldsConstants.Kanji.ReadingKun] = ReadingKunHtml;
      fields[NoteFieldsConstants.Kanji.ReadingNan] = ReadingNanHtml;
      fields[NoteFieldsConstants.Kanji.Radicals] = string.Join(", ", Radicals);
      fields[NoteFieldsConstants.Kanji.SourceMeaningMnemonic] = SourceMeaningMnemonic;
      fields[NoteFieldsConstants.Kanji.MeaningInfo] = MeaningInfo;
      fields[NoteFieldsConstants.Kanji.ReadingMnemonic] = ReadingMnemonic;
      fields[NoteFieldsConstants.Kanji.ReadingInfo] = ReadingInfo;
      fields[NoteFieldsConstants.Kanji.PrimaryVocab] = string.Join(", ", PrimaryVocab);
      fields[NoteFieldsConstants.Kanji.Audio] = Audio;
      fields[NoteFieldsConstants.Kanji.PrimaryReadingsTTSAudio] = PrimaryReadingsTtsAudio;
      fields[NoteFieldsConstants.Kanji.References] = References;
      fields[NoteFieldsConstants.Kanji.UserMnemonic] = UserMnemonic;
      fields[NoteFieldsConstants.Kanji.UserSimilarMeaning] = string.Join(", ", SimilarMeaning);
      fields[NoteFieldsConstants.Kanji.RelatedConfusedWith] = string.Join(", ", ConfusedWith);
      fields[NoteFieldsConstants.Kanji.Image] = Image;
   }

   /// Creates KanjiData from raw AnkiNoteData
   public static KanjiData FromAnkiNoteData(AnkiNoteData data) => FromAnki(new AnkiKanjiNote(data));

   public static KanjiData FromAnki(AnkiKanjiNote anki) =>
      new()
      {
         Id = (anki.Id ?? KanjiId.New()).Value,
         Tags = [..anki.Tags],
         Kanji = anki.Question,
         SourceAnswer = anki.SourceAnswer,
         ActiveAnswer = anki.ActiveAnswer,
         Audio = anki.Audio,
         PrimaryReadingsTtsAudio = anki.PrimaryReadingsTtsAudio,
         Image = anki.Image,
      };

   /// Merges Anki-owned fields (every field exposed by <see cref="AnkiKanjiNote"/>, plus tags) into existing data, preserving all fields Anki does not store.
   public KanjiData MergeAnkiData(AnkiNoteData ankiData)
   {
      var anki = new AnkiKanjiNote(ankiData);
      return new KanjiData
             {
                Id = Id,
                Tags = [..anki.Tags],
                Kanji = anki.Question,
                SourceAnswer = anki.SourceAnswer,
                UserAnswer = UserAnswer,
                ActiveAnswer = anki.ActiveAnswer,
                ReadingOnHtml = ReadingOnHtml,
                ReadingKunHtml = ReadingKunHtml,
                ReadingNanHtml = ReadingNanHtml,
                Radicals = Radicals,
                SourceMeaningMnemonic = SourceMeaningMnemonic,
                MeaningInfo = MeaningInfo,
                ReadingMnemonic = ReadingMnemonic,
                ReadingInfo = ReadingInfo,
                PrimaryVocab = PrimaryVocab,
                Audio = anki.Audio,
                PrimaryReadingsTtsAudio = anki.PrimaryReadingsTtsAudio,
                References = References,
                UserMnemonic = UserMnemonic,
                SimilarMeaning = SimilarMeaning,
                ConfusedWith = ConfusedWith,
                Image = anki.Image,
             };
   }
}
