using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.NoteFields;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Note.NoteFields;

public class When_parsing_media_references
{
   public class when_parsing_audio_references : When_parsing_media_references
   {
      [XF] public void it_extracts_simple_sound_tag() =>
         MediaFieldParsing.ParseAudioReferences("[sound:test.mp3]")
                          .Count.Must().Be(1);

      [XF] public void it_extracts_filename_from_sound_tag() =>
         MediaFieldParsing.ParseAudioReferences("[sound:test.mp3]")[0]
                          .FileName.Must().Be("test.mp3");

      [XF] public void it_handles_apostrophe_in_filename() =>
         MediaFieldParsing.ParseAudioReferences("[sound:A_Girls'_Last_Tour_S01_03_0.03.36.299-0.03.38.218.mp3]")[0]
                          .FileName.Must().Be("A_Girls'_Last_Tour_S01_03_0.03.36.299-0.03.38.218.mp3");

      [XF] public void it_handles_multiple_sound_tags() =>
         MediaFieldParsing.ParseAudioReferences("[sound:a.mp3][sound:b.mp3]")
                          .Count.Must().Be(2);

      [XF] public void it_returns_empty_for_blank_input() =>
         MediaFieldParsing.ParseAudioReferences("").Count.Must().Be(0);
   }

   public class when_parsing_image_references : When_parsing_media_references
   {
      [XF] public void it_extracts_simple_img_tag() =>
         MediaFieldParsing.ParseImageReferences("<img src=\"test.jpg\">")
                          .Count.Must().Be(1);

      [XF] public void it_extracts_filename_from_img_tag() =>
         MediaFieldParsing.ParseImageReferences("<img src=\"test.jpg\">")[0]
                          .FileName.Must().Be("test.jpg");

      [XF] public void it_handles_apostrophe_in_double_quoted_filename() =>
         MediaFieldParsing.ParseImageReferences("<img src=\"A_Girls'_Last_Tour_S01_03_0.03.37.259.jpg\">")[0]
                          .FileName.Must().Be("A_Girls'_Last_Tour_S01_03_0.03.37.259.jpg");

      [XF] public void it_handles_single_quoted_src() =>
         MediaFieldParsing.ParseImageReferences("<img src='test.jpg'>")[0]
                          .FileName.Must().Be("test.jpg");

      [XF] public void it_handles_apostrophe_in_single_quoted_filename() =>
         MediaFieldParsing.ParseImageReferences("<img src='A_Girls&#39;_Last_Tour.jpg'>")[0]
                          .FileName.Must().Be("A_Girls&#39;_Last_Tour.jpg");

      [XF] public void it_returns_empty_for_blank_input() =>
         MediaFieldParsing.ParseImageReferences("").Count.Must().Be(0);

      [XF] public void it_handles_extra_attributes() =>
         MediaFieldParsing.ParseImageReferences("<img class=\"photo\" src=\"test.jpg\" alt=\"pic\">")[0]
                          .FileName.Must().Be("test.jpg");
   }
}
