using System.Linq;
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
         Readings = note.Readings.ToDto(),
         Radicals = [.. note.Radicals.All],
         PrimaryVocab = [.. note.PrimaryVocab.Words.Select(it => it.ToDto())],
         References = note.KanjiReferences.Value,
         Mnemonic = note.Mnemonic.Value,
         SimilarMeaning = [.. note.SimilarMeaning.Kanji],
         ConfusedWith = [.. note.ConfusedWith.Kanji],
         Images = [..note.Images.All.Select(ImageReferenceDto.From)],
         SourceData = note.SourceData.ToDto(),
         Tags = note.Tags.ToStringList(),
      };
}
