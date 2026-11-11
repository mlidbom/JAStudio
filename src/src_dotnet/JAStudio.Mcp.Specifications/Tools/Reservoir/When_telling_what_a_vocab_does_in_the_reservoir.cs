using System;
using System.Collections.Generic;
using System.IO;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir;
using JAStudio.Core.Reservoir.Sources;
using JAStudio.Mcp.Tools.Reservoir;
using Microsoft.Data.Sqlite;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Reservoir;

/// <summary>
/// Tells what the vocab 行く does in a reservoir of three sentences: two that have it, one of them inflected, as 行かない, and one that contains<br/>
/// none of its forms or their stems.<br/>
/// The reservoir has had no bulk analysis, so nothing counts as changed since one.
/// </summary>
public class When_telling_what_a_vocab_does_in_the_reservoir : McpServerSpecification
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-reservoir-tool-spec-");
   readonly VocabInTheReservoir _result;

   public When_telling_what_a_vocab_does_in_the_reservoir()
   {
      CreateVocab("行く", "to go", "いく");
      var file = new FileInfo(Path.Combine(_folder.FullName, "reservoir.sqlite"));
      using(var reservoir = ReservoirFile.OpenOrCreate(file))
         reservoir.Add(new SourceWork(new ReservoirSource("specs", "a work", ReservoirSourceKind.Subtitles, "nowhere"),
                                      [Sentence("明日学校に行く"), Sentence("学校に行かない"), Sentence("雨が降る")]));

      _result = ResultOf<VocabInTheReservoir>(CallTool("what_a_vocab_does_in_the_reservoir", new Dictionary<string, object?> { ["vocab"] = "行く", ["reservoirFile"] = file.FullName }));
   }

   [XF] public void the_sentences_containing_one_of_its_forms_or_their_stems_are_analyzed() => $"{_result.SentencesContainingTheTexts} contain them, {_result.SentencesAnalyzed} analyzed".Must().Be("2 contain them, 2 analyzed");
   [XF] public void it_matches_in_both_of_them_inflected_or_not() => _result.Matching.Examples.Must().SequenceEqual(["明日学校に行く", "学校に行かない"]);
   [XF] public void nothing_changed_since_a_bulk_analysis_there_was_none_of() => $"{_result.NewlyMatching.Count} newly, {_result.NoLongerMatching.Count} no longer".Must().Be("0 newly, 0 no longer");

   public override void Dispose()
   {
      base.Dispose();
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   static SourceSentence Sentence(string text) =>
      new(ReservoirSentenceText.CleanedFrom(text) ?? throw new ArgumentException($"{text} makes no sentence"), "spec", Translation: null, AudioFile: null, ImageFile: null);
}
