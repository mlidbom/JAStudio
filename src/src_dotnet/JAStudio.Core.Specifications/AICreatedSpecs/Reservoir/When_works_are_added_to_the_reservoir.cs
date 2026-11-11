using System;
using System.IO;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir;
using JAStudio.Core.Reservoir.Sources;
using Microsoft.Data.Sqlite;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// Works added to the reservoir (<see cref="ReservoirFile"/>): each distinct sentence is kept once, counted each time it's found, and kept from<br/>
/// the first source that has its audio, since a sentence with audio is the one worth taking into the working set.
/// </summary>
public class When_works_are_added_to_the_reservoir : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-reservoir-spec-");
   readonly ReservoirFile _reservoir;
   readonly WorkAddedToTheReservoir _subtitlesAdded;
   readonly WorkAddedToTheReservoir _deckAdded;

   static readonly ReservoirSource Subtitles = new("kitsunekko", "Natsume", ReservoirSourceKind.Subtitles, @"E:\subs\Natsume");
   static readonly ReservoirSource Deck = new("mediafire", "Natsume.apkg", ReservoirSourceKind.Deck, @"E:\decks\Natsume.apkg");

   public When_works_are_added_to_the_reservoir()
   {
      _reservoir = ReservoirFile.OpenOrCreate(new FileInfo(Path.Combine(_folder.FullName, "reservoir.sqlite")));
      _subtitlesAdded = _reservoir.Add(new SourceWork(Subtitles, [Sentence("友人帳を寄越せ", "01.srt 00:10:01"), Sentence("いやだ", "01.srt 00:10:05"), Sentence("いやだ", "02.srt 00:01:00")]));
      _deckAdded = _reservoir.Add(new SourceWork(Deck, [Sentence("友人帳を寄越せ", "note 7", audioFile: "ny_01.mp3"), Sentence("先生", "note 8", audioFile: "ny_02.mp3")]));
   }

   [XF] public void a_work_counts_its_sentences_and_those_new_to_the_reservoir() => (_subtitlesAdded.Sentences, _subtitlesAdded.NewSentences).Must().Be((3L, 2L));
   [XF] public void a_later_work_counts_as_new_only_what_the_reservoir_lacked() => (_deckAdded.Sentences, _deckAdded.NewSentences).Must().Be((2L, 1L));
   [XF] public void each_distinct_sentence_is_kept_once() => _reservoir.SentenceCount.Must().Be(3L);
   [XF] public void the_reservoir_has_each_work_it_was_given() => (_reservoir.Has(Subtitles) && _reservoir.Has(Deck)).Must().BeTrue();

   [XF] public void a_sentence_found_again_with_audio_is_kept_from_the_source_with_its_audio() =>
      KeptAs("友人帳を寄越せ").Must().Be("Natsume.apkg note 7 ny_01.mp3, found 2 times");

   [XF] public void a_sentence_found_again_without_audio_is_kept_from_where_it_was_first_found() =>
      KeptAs("いやだ").Must().Be("Natsume 01.srt 00:10:05 -, found 2 times");

   public void Dispose()
   {
      _reservoir.Dispose();
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   string KeptAs(string text)
   {
      var sources = _reservoir.Sources();
      var sentence = _reservoir.SentencesWithIds([1, 2, 3]).Values.Single(it => it.Text == text);
      return $"{sources[sentence.SourceId].Work} {sentence.Position} {sentence.AudioFile ?? "-"}, found {sentence.Occurrences} times";
   }

   static SourceSentence Sentence(string text, string position, string? audioFile = null) =>
      new(ReservoirSentenceText.CleanedFrom(text) ?? throw new ArgumentException($"{text} makes no sentence"), position, Translation: null, audioFile, ImageFile: null);
}
