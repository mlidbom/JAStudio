using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A sentence's text, as a deck's field or a subtitle's line gives it, cleaned for the reservoir (<see cref="ReservoirSentenceText"/>): two<br/>
/// sources giving one sentence must give one text, whatever they wrapped it in.
/// </summary>
public class When_a_sources_text_is_cleaned_for_the_reservoir : IAIGeneratedSpec
{
   /// <summary><paramref name="raw"/> cleaned, or <c>(no sentence)</c> when it makes none, so that a spec expecting a text fails saying so.</summary>
   static string Cleaned(string raw) => ReservoirSentenceText.CleanedFrom(raw)?.Text ?? "(no sentence)";

   [XF] public void html_tags_go_and_entities_become_their_characters() => Cleaned("<div>お前&nbsp;<b>やりすぎ</b></div>").Must().Be("お前 やりすぎ");
   [XF] public void a_line_break_becomes_a_space() => Cleaned("待って<br>まだ").Must().Be("待って まだ");
   [XF] public void ruby_readings_go_and_the_base_text_stays() => Cleaned("<ruby>漢字<rt>かんじ</rt></ruby>を読む").Must().Be("漢字を読む");
   [XF] public void a_speakers_name_in_brackets_goes() => Cleaned("（ユーリ）これも武器なのかな？").Must().Be("これも武器なのかな？");
   [XF] public void subtitle_formatting_goes_and_its_line_break_becomes_a_space() => Cleaned(@"{\an8}行くぞ\Nみんな").Must().Be("行くぞ みんな");
   [XF] public void music_notes_go() => Cleaned("♪～ 風が吹いている ♪").Must().Be("風が吹いている");
   [XF] public void a_dialogue_dash_at_the_start_goes() => Cleaned("－ 早く来い").Must().Be("早く来い");
   [XF] public void ideographic_spaces_become_single_spaces() => Cleaned("おい　　これ").Must().Be("おい これ");
   [XF] public void a_direction_mark_goes() => Cleaned($"{(char)0x200E}あの…").Must().Be("あの…");
   [XF] public void half_width_katakana_become_full_width() => Cleaned("ﾎﾟｹﾓﾝ泥棒").Must().Be("ポケモン泥棒");
   [XF] public void a_line_with_no_japanese_is_no_sentence() => Cleaned("Hey, which way is it?").Must().Be("(no sentence)");
   [XF] public void a_speakers_name_alone_is_no_sentence() => Cleaned("（ユーリ）").Must().Be("(no sentence)");
   [XF] public void a_line_in_chinese_with_kanji_but_no_kana_is_no_sentence() => Cleaned("警察抓到的小偷会被关进监狱").Must().Be("(no sentence)");
   [XF] public void a_short_line_of_kanji_alone_is_a_sentence() => Cleaned("了解").Must().Be("了解");

   public class with_the_conventions_of_japanese_tv_captions : When_a_sources_text_is_cleaned_for_the_reservoir
   {
      [XF] public void the_arrow_continuing_a_line_goes() => Cleaned("あんときは→").Must().Be("あんときは");
      [XF] public void the_marks_of_speech_off_screen_go_and_the_speech_stays() => Cleaned("((お前 何してんだ))").Must().Be("お前 何してんだ");
      [XF] public void the_marks_of_speech_in_thought_go_and_the_speech_stays() => Cleaned("≪くそっ ブロックが高い≫").Must().Be("くそっ ブロックが高い");
      [XF] public void a_sounds_source_in_square_brackets_goes() => Cleaned("[TEL] ハハハハッ。➡").Must().Be("ハハハハッ。");
      [XF] public void the_code_of_a_character_the_caption_could_not_show_goes() => Cleaned("[外:EC0D160D9254B2E11D601546DB93002D] ったく…").Must().Be("ったく…");
      [XF] public void a_speakers_name_before_a_colon_goes() => Cleaned("お妙：もう！ 何回言ったらわかるの？").Must().Be("もう！ 何回言ったらわかるの？");
      [XF] public void a_time_is_no_speakers_name() => Cleaned("12:30に会おう").Must().Be("12:30に会おう");
   }
}
