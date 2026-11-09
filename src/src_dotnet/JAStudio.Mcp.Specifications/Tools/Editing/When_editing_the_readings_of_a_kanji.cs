using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Tools.Editing;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

/// <summary>The kanji 行 has the on readings こう, primary, and ぎょう, and the kun reading い, primary.</summary>
public class When_editing_the_readings_of_a_kanji : McpServerSpecification
{
   protected readonly KanjiNote Kanji;

   public When_editing_the_readings_of_a_kanji() => Kanji = CreateKanji("行", "go", "<primary>コウ</primary>, ギョウ", "<primary>い</primary>");

   protected CallToolResult CallWithTheKanji(string toolName, Dictionary<string, object?> arguments) =>
      CallTool(toolName, new Dictionary<string, object?>(arguments) { ["kanjiNoteId"] = Kanji.GetId().ToString() });

   protected KanjiNoteReadings ReadingsStored() => GetService<INoteRepository>().LoadAll().Kanji.Single(it => it.GetId() == Kanji.GetId()).Readings;

   public class adding_the_kun_reading_ゆ : When_editing_the_readings_of_a_kanji
   {
      readonly KanjiReadingsChange _change;

      public adding_the_kun_reading_ゆ() =>
         _change = ResultOf<KanjiReadingsChange>(CallWithTheKanji("add_a_reading_to_a_kanji", new Dictionary<string, object?> { ["kind"] = "kun", ["kana"] = "ゆ" }));

      [XF] public void the_change_names_the_kanji() => _change.Kanji.Must().Be("行");
      [XF] public void the_kun_readings_before_are_い() => _change.Before.Kun.Must().SequenceEqual([new KanjiReading("い", true)]);
      [XF] public void the_kun_readings_after_are_い_then_ゆ_as_a_secondary_reading() => _change.After.Kun.Must().SequenceEqual([new KanjiReading("い", true), new KanjiReading("ゆ", false)]);
      [XF] public void the_on_readings_are_unchanged() => _change.After.On.Must().SequenceEqual(_change.Before.On);
      [XF] public void the_kanji_is_saved_with_it() => ReadingsStored().Kun.AllKana().Must().SequenceEqual(["い", "ゆ"]);
   }

   public class removing_the_on_reading_ギョウ_given_in_katakana : When_editing_the_readings_of_a_kanji
   {
      readonly KanjiReadingsChange _change;

      public removing_the_on_reading_ギョウ_given_in_katakana() =>
         _change = ResultOf<KanjiReadingsChange>(CallWithTheKanji("remove_a_reading_from_a_kanji", new Dictionary<string, object?> { ["kind"] = "on", ["kana"] = "ギョウ" }));

      [XF] public void the_on_readings_after_are_こう() => _change.After.On.Must().SequenceEqual([new KanjiReading("こう", true)]);
      [XF] public void the_kanji_is_saved_without_it() => ReadingsStored().On.AllKana().Must().SequenceEqual(["こう"]);
   }

   public class removing_a_reading_the_kanji_does_not_have : When_editing_the_readings_of_a_kanji
   {
      readonly CallToolResult _result;

      public removing_a_reading_the_kanji_does_not_have() =>
         _result = CallWithTheKanji("remove_a_reading_from_a_kanji", new Dictionary<string, object?> { ["kind"] = "kun", ["kana"] = "や" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_says_the_reading_is_not_one_of_the_kanjis_readings_of_that_kind() => TextOf(_result).Must().Contain("や isn't one of the Kun readings");
   }

   public class making_the_on_reading_ぎょう_primary : When_editing_the_readings_of_a_kanji
   {
      readonly KanjiReadingsChange _change;

      public making_the_on_reading_ぎょう_primary() =>
         _change = ResultOf<KanjiReadingsChange>(CallWithTheKanji("set_whether_a_kanji_reading_is_primary",
                                                                  new Dictionary<string, object?> { ["kind"] = "on", ["kana"] = "ぎょう", ["isPrimary"] = true }));

      [XF] public void the_on_readings_after_are_both_primary() => _change.After.On.Must().SequenceEqual([new KanjiReading("こう", true), new KanjiReading("ぎょう", true)]);
      [XF] public void the_kanji_is_saved_with_it_primary() => ReadingsStored().On.IsPrimary("ぎょう").Must().BeTrue();
   }

   public class making_the_on_reading_こう_secondary : When_editing_the_readings_of_a_kanji
   {
      readonly KanjiReadingsChange _change;

      public making_the_on_reading_こう_secondary() =>
         _change = ResultOf<KanjiReadingsChange>(CallWithTheKanji("set_whether_a_kanji_reading_is_primary",
                                                                  new Dictionary<string, object?> { ["kind"] = "on", ["kana"] = "こう", ["isPrimary"] = false }));

      [XF] public void the_on_readings_after_are_both_secondary() => _change.After.On.Must().SequenceEqual([new KanjiReading("こう", false), new KanjiReading("ぎょう", false)]);
   }

   public class giving_a_kind_of_reading_there_is_not : When_editing_the_readings_of_a_kanji
   {
      readonly CallToolResult _result;

      public giving_a_kind_of_reading_there_is_not() =>
         _result = CallWithTheKanji("add_a_reading_to_a_kanji", new Dictionary<string, object?> { ["kind"] = "jukujikun", ["kana"] = "や" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_kinds_there_are() => TextOf(_result).Must().Contain("The kinds are: \"on\", \"kun\", \"nanori\"");
      [XF] public void the_kanji_keeps_its_readings() => Kanji.Readings.AllKana().Must().SequenceEqual(["こう", "ぎょう", "い"]);
   }
}
