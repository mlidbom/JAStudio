using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.Media;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Anki;

/// <summary>Anki's markup for media in a note's fields, read where Anki's fields come in and written where they go out (<see cref="AnkiMediaMarkup"/>).</summary>
public class When_reading_and_writing_Ankis_media_markup
{
   public class reading_audio : When_reading_and_writing_Ankis_media_markup
   {
      [XF] public void a_sound_tag_is_one_file() => AnkiMediaMarkup.AudioIn("[sound:test.mp3]").Single().FileName.Must().Be("test.mp3");

      [XF] public void an_apostrophe_is_part_of_the_name() =>
         AnkiMediaMarkup.AudioIn("[sound:A_Girls'_Last_Tour_S01_03_0.03.36.299-0.03.38.218.mp3]").Single().FileName.Must().Be("A_Girls'_Last_Tour_S01_03_0.03.36.299-0.03.38.218.mp3");

      [XF] public void spaces_and_parentheses_are_part_of_the_name() =>
         AnkiMediaMarkup.AudioIn("[sound:ATTS しとく (azure ja-jp daichineural).mp3]").Single().FileName.Must().Be("ATTS しとく (azure ja-jp daichineural).mp3");

      [XF] public void the_files_are_in_the_order_the_field_has_them_and_what_is_between_them_is_dropped() =>
         AnkiMediaMarkup.AudioIn("[sound:b.mp3], [sound:a.mp3]<br>&nbsp;[sound:c.mp3]").Select(it => it.FileName).Must().SequenceEqual(["b.mp3", "a.mp3", "c.mp3"]);

      [XF] public void nothing_is_known_about_a_file() => AnkiMediaMarkup.AudioIn("[sound:test.mp3]").Single().Must().Be(new AudioReference("test.mp3"));

      [XF] public void an_empty_field_has_none() => AnkiMediaMarkup.AudioIn("").Must().BeEmpty();
   }

   public class reading_images : When_reading_and_writing_Ankis_media_markup
   {
      [XF] public void an_img_tag_is_one_file() => AnkiMediaMarkup.ImagesIn("<img src=\"test.jpg\">").Single().FileName.Must().Be("test.jpg");

      [XF] public void an_apostrophe_in_a_double_quoted_name_is_part_of_it() =>
         AnkiMediaMarkup.ImagesIn("<img src=\"A_Girls'_Last_Tour_S01_03_0.03.37.259.jpg\">").Single().FileName.Must().Be("A_Girls'_Last_Tour_S01_03_0.03.37.259.jpg");

      [XF] public void a_single_quoted_name_is_read() => AnkiMediaMarkup.ImagesIn("<img src='test.jpg'>").Single().FileName.Must().Be("test.jpg");

      [XF] public void an_entity_in_a_name_is_decoded() => AnkiMediaMarkup.ImagesIn("<img src='A_Girls&#39;_Last_Tour.jpg'>").Single().FileName.Must().Be("A_Girls'_Last_Tour.jpg");

      [XF] public void other_attributes_are_skipped() => AnkiMediaMarkup.ImagesIn("<IMG class=\"photo\" src=\"test.jpg\" style=\"height: 200px\">").Single().FileName.Must().Be("test.jpg");

      [XF] public void the_alt_text_is_kept_decoded() =>
         AnkiMediaMarkup.ImagesIn("<img src=\"x.jpg\" alt=\"Definition &amp; Meaning of &quot;Bothersome&quot;\">").Single().AltText.Must().NotBeNull().Be("Definition & Meaning of \"Bothersome\"");

      [XF] public void an_alt_text_holding_angle_brackets_does_not_end_the_tag() =>
         AnkiMediaMarkup.ImagesIn("<img alt=\"a <b> c\" src=\"x.jpg\"><img src=\"y.jpg\">").Must().SequenceEqual([new ImageReference("x.jpg") { AltText = "a <b> c" }, new ImageReference("y.jpg")]);

      [XF] public void without_an_alt_text_it_has_none() => AnkiMediaMarkup.ImagesIn("<img src=\"test.jpg\">").Single().AltText.Must().BeNull();

      [XF] public void a_tag_without_a_src_is_no_file() => AnkiMediaMarkup.ImagesIn("<img alt=\"nothing\">").Must().BeEmpty();

      [XF] public void an_empty_field_has_none() => AnkiMediaMarkup.ImagesIn("").Must().BeEmpty();
   }

   public class writing_media : When_reading_and_writing_Ankis_media_markup
   {
      [XF] public void audio_is_a_sound_tag_for_each_file_one_after_the_other() =>
         AnkiMediaMarkup.SoundTagsFor([new AudioReference("a.mp3"), new AudioReference("b.mp3")]).Must().Be("[sound:a.mp3][sound:b.mp3]");

      [XF] public void no_audio_is_an_empty_field() => AnkiMediaMarkup.SoundTagsFor([]).Must().Be("");

      [XF] public void an_image_without_an_alt_text_is_an_img_tag_with_its_src() =>
         AnkiMediaMarkup.ImgTagsFor([new ImageReference("a.jpg")]).Must().Be("<img src=\"a.jpg\">");

      [XF] public void an_alt_text_is_written_with_the_characters_that_would_break_it_as_entities() =>
         AnkiMediaMarkup.ImgTagsFor([new ImageReference("a.jpg") { AltText = "Tom & \"Jerry\" <3" }]).Must().Be("<img src=\"a.jpg\" alt=\"Tom &amp; &quot;Jerry&quot; &lt;3\">");

      [XF] public void images_read_back_as_written() =>
         AnkiMediaMarkup.ImagesIn(AnkiMediaMarkup.ImgTagsFor([new ImageReference("a.jpg") { AltText = "Tom & \"Jerry\" <3>" }, new ImageReference("b.jpg")]))
                        .Must().SequenceEqual([new ImageReference("a.jpg") { AltText = "Tom & \"Jerry\" <3>" }, new ImageReference("b.jpg")]);
   }
}
