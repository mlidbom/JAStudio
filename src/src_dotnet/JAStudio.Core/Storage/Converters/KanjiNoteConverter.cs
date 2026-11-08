using JAStudio.Core.Note;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Storage.Converters;

static class KanjiNoteConverter
{
   public static KanjiDto ToDto(KanjiNote note) =>
      new()
      {
         Id = note.GetId().Value,
         Kanji = note.GetQuestion(),
         SourceAnswer = note.SourceData.Answer.Value,
         UserAnswer = note.CorpusAnswer.Value,
         ActiveAnswer = note.GetAnswer(),
         ReadingOnHtml = note.ReadingOnHtml.Value,
         ReadingKunHtml = note.ReadingKunHtml.Value,
         ReadingNanHtml = note.ReadingNanHtml.Value,
         Radicals = note.AllRadicals,
         SourceMeaningMnemonic = note.SourceData.MeaningMnemonic.Value,
         MeaningInfo = note.SourceData.MeaningInfo.Value,
         ReadingMnemonic = note.SourceData.ReadingMnemonic.Value,
         ReadingInfo = note.SourceData.ReadingInfo.Value,
         PrimaryVocab = note.PrimaryVocab,
         References = note.KanjiReferences.Value,
         UserMnemonic = note.Mnemonic.Value,
         SimilarMeaning = note.SimilarMeaning,
         ConfusedWith = note.RelatedConfusedWith,
         Image = note.Image.RawValue(),
         Tags = note.Tags.ToStringList(),
      };
}
