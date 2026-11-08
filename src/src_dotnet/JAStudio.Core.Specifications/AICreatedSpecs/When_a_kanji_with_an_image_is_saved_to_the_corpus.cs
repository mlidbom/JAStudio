using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A kanji whose image is set, written as the corpus's JSON and read back, as JAStudio saves and loads the corpus.</summary>
public class When_a_kanji_with_an_image_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   const string ImageMarkup = "<img src=\"Ishikari-River.jpg\">";
   readonly KanjiNote _readBack;

   public When_a_kanji_with_an_image_is_saved_to_the_corpus()
   {
      var kanji = CreateKanji("川", "river", "セン", "かわ");
      kanji.Image.SetRawValue(ImageMarkup);
      var serializer = GetService<NoteSerializer>();
      _readBack = serializer.DeserializeKanji(serializer.Serialize(kanji));
   }

   [XF] public void it_is_read_back_with_its_image() => _readBack.Image.RawValue().Must().Be(ImageMarkup);

   [XF] public void the_image_counts_as_a_media_file_the_kanji_uses_so_deleting_another_note_never_deletes_it() =>
      _readBack.MediaReferences.Select(it => it.FileName).Must().SequenceEqual(["Ishikari-River.jpg"]);
}
