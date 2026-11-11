using System.Linq;
using System.Text;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir.Sources.Subtitles;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>A subtitle file read as the sentences it shows (<see cref="SubtitleFile"/>), each line of Japanese with the time it's shown at.</summary>
public class When_a_subtitle_file_is_read_for_the_reservoir : IAIGeneratedSpec
{
   public class in_the_srt_format : When_a_subtitle_file_is_read_for_the_reservoir
   {
      const string Srt = "1\r\n00:00:01,000 --> 00:00:02,500\r\nおい\r\nこれどっちだ\r\n\r\n2\r\n00:00:03,000 --> 00:00:04,000\r\nHello there\r\n\r\n3\r\n00:00:05,000 --> 00:00:06,000\r\n行くぞ\r\n";
      readonly string[] _sentences = [.. new SubtitleFile("Natsume 01.srt", Encoding.UTF8.GetBytes(Srt)).Sentences().Select(it => $"{it.Position}: {it.Text}")];

      [XF] public void a_line_on_two_rows_is_one_sentence_at_its_start_time() => _sentences[0].Must().Be("Natsume 01.srt 00:00:01,000: おい これどっちだ");
      [XF] public void a_line_in_english_is_no_sentence_and_the_last_line_is_one_without_a_blank_row_after_it() => _sentences.Length.Must().Be(2);
      [XF] public void the_last_line_is_read() => _sentences[1].Must().Be("Natsume 01.srt 00:00:05,000: 行くぞ");
   }

   public class in_the_ass_format : When_a_subtitle_file_is_read_for_the_reservoir
   {
      const string Ass = "[Events]\nFormat: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text\n" +
                         @"Dialogue: 0,0:00:10.00,0:00:12.00,Default,,0,0,0,,{\i1}待って,まだ\Nだめ" + "\nComment: 0,0:00:13.00,0:00:14.00,Default,,0,0,0,,これは注釈\n";
      readonly string[] _sentences = [.. new SubtitleFile("Aria 01.ass", Encoding.UTF8.GetBytes(Ass)).Sentences().Select(it => $"{it.Position}: {it.Text}")];

      [XF] public void a_dialogue_rows_text_is_all_that_follows_its_ninth_comma_its_formatting_gone() => _sentences.Must().SequenceEqual(["Aria 01.ass 0:00:10.00: 待って,まだ だめ"]);
   }

   public class in_shift_jis : When_a_subtitle_file_is_read_for_the_reservoir
   {
      [XF] public void it_is_decoded_as_shift_jis_since_it_is_no_valid_utf8() =>
         new SubtitleFile("old.srt", ShiftJis().GetBytes("1\r\n00:00:01,000 --> 00:00:02,000\r\n夏目です\r\n")).Sentences().Single().Text.Text.Must().Be("夏目です");

      static Encoding ShiftJis()
      {
         Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
         return Encoding.GetEncoding(932);
      }
   }
}
