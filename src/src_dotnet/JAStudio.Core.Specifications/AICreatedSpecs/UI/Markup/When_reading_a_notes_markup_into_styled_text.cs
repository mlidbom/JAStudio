using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.UI.Markup;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.UI.Markup;

/// <summary>
/// A note's text read into the runs JAStudio's Avalonia pages show (<see cref="StyledText"/>): each run styled as the markup around it says, HTML such<br/>
/// as <c>&lt;b&gt;</c>, or JAStudio's own tags, such as <c>&lt;kanji&gt;</c>, with the line breaks the cards show.
/// </summary>
public class When_reading_a_notes_markup_into_styled_text : IAIGeneratedSpec
{
   StyledText _text = StyledText.FromMarkup("");

   void Read(string markup) => _text = StyledText.FromMarkup(markup);

   public class of_plain_text : When_reading_a_notes_markup_into_styled_text
   {
      public of_plain_text() => Read("食べる: to eat");

      [XF] public void it_is_one_plain_run() => _text.Runs.Must().SequenceEqual([new StyledRun("食べる: to eat", TextStyle.Plain)]);
   }

   public class with_a_bold_word_in_it : When_reading_a_notes_markup_into_styled_text
   {
      public with_a_bold_word_in_it() => Read("やっと<b>会えた</b>。");

      [XF] public void the_bold_word_is_a_run_of_its_own() =>
         _text.Runs.Must().SequenceEqual([new StyledRun("やっと", TextStyle.Plain), new StyledRun("会えた", TextStyle.Bold), new StyledRun("。", TextStyle.Plain)]);
   }

   public class with_a_kanji_tag_inside_a_bold_word : When_reading_a_notes_markup_into_styled_text
   {
      public with_a_kanji_tag_inside_a_bold_word() => Read("<b>the <kanji>山</kanji></b>");

      [XF] public void the_kanji_is_both_bold_and_a_kanji() =>
         _text.Runs.Must().SequenceEqual([new StyledRun("the ", TextStyle.Bold), new StyledRun("山", TextStyle.Bold | TextStyle.Kanji)]);
   }

   public class of_JAStudios_own_tags : When_reading_a_notes_markup_into_styled_text
   {
      public of_JAStudios_own_tags() => Read("<tag>suff:</tag> <ja>山</ja> <rad>ground</rad> <voc>山登り</voc> <read>やま</read> <obsolete>hill</obsolete>");

      [XF] public void each_has_its_style() =>
         _text.Runs.Must().SequenceEqual([new StyledRun("suff:", TextStyle.Label), new StyledRun(" ", TextStyle.Plain),
                                          new StyledRun("山", TextStyle.Japanese), new StyledRun(" ", TextStyle.Plain),
                                          new StyledRun("ground", TextStyle.Radical), new StyledRun(" ", TextStyle.Plain),
                                          new StyledRun("山登り", TextStyle.Vocabulary), new StyledRun(" ", TextStyle.Plain),
                                          new StyledRun("やま", TextStyle.Reading), new StyledRun(" ", TextStyle.Plain),
                                          new StyledRun("hill", TextStyle.StruckThrough)]);
   }

   public class of_a_kanjis_readings_as_the_pages_list_them : When_reading_a_notes_markup_into_styled_text
   {
      public of_a_kanjis_readings_as_the_pages_list_them() => Read("""サン <span class="readingsSeparator">|</span> <primary>やま</primary>""");

      [XF] public void the_separator_and_the_primary_reading_have_their_styles() =>
         _text.Runs.Must().SequenceEqual([new StyledRun("サン ", TextStyle.Plain), new StyledRun("|", TextStyle.ReadingsSeparator),
                                          new StyledRun(" ", TextStyle.Plain), new StyledRun("やま", TextStyle.Primary)]);
   }

   public class with_entities : When_reading_a_notes_markup_into_styled_text
   {
      public with_entities() => Read("&lt;wbr&gt; &amp; more");

      [XF] public void they_are_the_characters_they_stand_for() => _text.ToString().Must().Be("<wbr> & more");
   }

   public class in_divs_as_Ankis_editor_writes_lines : When_reading_a_notes_markup_into_styled_text
   {
      public in_divs_as_Ankis_editor_writes_lines() => Read("<div>first</div><div><br></div><div>second</div>");

      [XF] public void each_div_is_a_line_and_the_one_with_only_a_line_break_is_empty() => _text.ToString().Must().Be("first\n\nsecond");
   }

   public class with_a_list_between_two_lines : When_reading_a_notes_markup_into_styled_text
   {
      public with_a_list_between_two_lines() => Read("Verbs:<ol><li><i>joutai</i> stative</li><li>action</li></ol>Done");

      [XF] public void each_item_is_a_line_of_its_own_with_a_bullet() => _text.ToString().Must().Be("Verbs:\n• joutai stative\n• action\nDone");
      [XF] public void the_italic_word_keeps_its_style() => _text.Runs.Must().Contain(new StyledRun("joutai", TextStyle.Italic));
   }

   public class with_line_breaks_at_its_start_and_end : When_reading_a_notes_markup_into_styled_text
   {
      public with_line_breaks_at_its_start_and_end() => Read("<div><br></div>text<br>");

      [XF] public void they_are_left_out() => _text.ToString().Must().Be("text");
   }

   public class with_line_breaks_in_the_text_itself : When_reading_a_notes_markup_into_styled_text
   {
      public with_line_breaks_in_the_text_itself() => Read("one\r\ntwo");

      [XF] public void they_are_kept() => _text.ToString().Must().Be("one\ntwo");
   }

   public class with_a_word_break_an_image_and_a_link : When_reading_a_notes_markup_into_styled_text
   {
      public with_a_word_break_an_image_and_a_link() => Read("""a<wbr>b <img src="x.png"><a href="https://jisho.org">jisho</a>""");

      [XF] public void the_word_break_and_the_image_show_nothing_and_the_link_its_text() =>
         _text.Runs.Must().SequenceEqual([new StyledRun("ab jisho", TextStyle.Plain)]);
   }
}
