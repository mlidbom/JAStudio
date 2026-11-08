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
         CorpusAnswer = note.CorpusAnswer.Value,
         ReadingOnHtml = note.ReadingOnHtml.Value,
         ReadingKunHtml = note.ReadingKunHtml.Value,
         ReadingNanHtml = note.ReadingNanHtml.Value,
         Radicals = note.AllRadicals,
         PrimaryVocab = note.PrimaryVocab,
         References = note.KanjiReferences.Value,
         Mnemonic = note.Mnemonic.Value,
         SimilarMeaning = note.SimilarMeaning,
         ConfusedWith = note.RelatedConfusedWith,
         Image = note.Image.RawValue(),
         SourceData = note.SourceData.ToDto(),
         Tags = note.Tags.ToStringList(),
      };
}
