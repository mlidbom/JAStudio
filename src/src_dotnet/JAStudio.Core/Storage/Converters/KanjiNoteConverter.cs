using JAStudio.Core.Anki;
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
         Radicals = note.AllRadicals,
         PrimaryVocab = note.PrimaryVocab,
         References = note.KanjiReferences.Value,
         Mnemonic = note.Mnemonic.Value,
         SimilarMeaning = note.SimilarMeaning,
         ConfusedWith = note.RelatedConfusedWith,
         //wip: slice 9's step 3 stores the images themselves, rather than in the media field of today (TodaysMediaFields).
         Image = AnkiMediaMarkup.ImgTagsFor(note.Images.All),
         SourceData = note.SourceData.ToDto(),
         Tags = note.Tags.ToStringList(),
      };
}
