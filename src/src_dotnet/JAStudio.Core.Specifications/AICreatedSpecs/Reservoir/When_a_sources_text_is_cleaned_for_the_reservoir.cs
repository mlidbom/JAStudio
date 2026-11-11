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
}
