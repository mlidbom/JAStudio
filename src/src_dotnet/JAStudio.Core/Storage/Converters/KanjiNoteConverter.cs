using JAStudio.Core.Note;
using JAStudio.Core.Note.CorpusData;

namespace JAStudio.Core.Storage.Converters;

static class KanjiNoteConverter
{
   public static KanjiData ToCorpusData(KanjiNote note) =>
      new()
      {
         Id = note.GetId().Value,
         Kanji = note.GetQuestion(),
         SourceAnswer = note.SourceAnswer.Value,
         UserAnswer = note.UserAnswer.Value,
         ActiveAnswer = note.ActiveAnswer.Value,
         ReadingOnHtml = note.ReadingOnHtml.Value,
         ReadingKunHtml = note.ReadingKunHtml.Value,
         ReadingNanHtml = note.ReadingNanHtml.Value,
         Radicals = note.AllRadicals,
         SourceMeaningMnemonic = note.SourceMeaningMnemonic.Value,
         MeaningInfo = note.MeaningInfo.Value,
         ReadingMnemonic = note.ReadingMnemonic.Value,
         ReadingInfo = note.ReadingInfo.Value,
         PrimaryVocab = note.PrimaryVocab,
         References = note.KanjiReferences.Value,
         UserMnemonic = note.UserMnemonic.Value,
         SimilarMeaning = note.UserSimilarMeaning,
         ConfusedWith = note.RelatedConfusedWith,
         Tags = note.Tags.ToStringList(),
      };
}
