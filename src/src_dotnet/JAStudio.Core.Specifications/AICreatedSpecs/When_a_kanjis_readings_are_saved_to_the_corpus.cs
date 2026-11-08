using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A kanji's readings (<see cref="KanjiNote.Readings"/>), written as the corpus's JSON and read back, as JAStudio saves and loads the corpus:<br/>
/// <c>readings</c> holds a list per kind, <c>on</c>, <c>kun</c> and <c>nanori</c>, of each reading's <c>kana</c> and whether it <c>isPrimary</c>.
/// </summary>
public class When_a_kanjis_readings_are_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly JsonElement _readings;
   readonly KanjiNote _readBack;

   public When_a_kanjis_readings_are_saved_to_the_corpus()
   {
      var kanji = CreateKanji("頁", "page", "<primary>ケツ</primary>, ヨウ", "かしら, <primary>ページ</primary>");
      var serializer = GetService<NoteSerializer>();
      var json = serializer.Serialize(kanji);
      _readings = JsonDocument.Parse(json).RootElement.GetProperty("readings");
      _readBack = serializer.DeserializeKanji(json);
   }

   static readonly List<KanjiReading> OnReadings = [new("けつ", IsPrimary: true), new("よう", IsPrimary: false)];
   static readonly List<KanjiReading> KunReadings = [new("かしら", IsPrimary: false), new("ページ", IsPrimary: true)];

   List<KanjiReading> Stored(string kind) =>
      [.. _readings.GetProperty(kind).EnumerateArray().Select(it => new KanjiReading(it.GetProperty("kana").ToString(), it.GetProperty("isPrimary").GetBoolean()))];

   [XF] public void the_corpus_json_stores_the_on_readings_in_hiragana_each_with_whether_it_is_primary() => Stored("on").Must().SequenceEqual(OnReadings);
   [XF] public void the_corpus_json_stores_the_kun_readings_each_with_whether_it_is_primary() => Stored("kun").Must().SequenceEqual(KunReadings);
   [XF] public void the_corpus_json_stores_an_empty_list_of_nanori_readings() => Stored("nanori").Must().BeEmpty();
   [XF] public void the_on_readings_are_read_back_as_they_were() => _readBack.Readings.On.All().Must().SequenceEqual(OnReadings);
   [XF] public void the_kun_readings_are_read_back_as_they_were() => _readBack.Readings.Kun.All().Must().SequenceEqual(KunReadings);
}
