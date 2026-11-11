using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Reservoir.Analysis;

namespace JAStudio.Core.Reservoir.Measures;

/// <summary>
/// The report of what the reservoir could give the corpus, written to a folder: <c>report.md</c>, readable at a glance, with the top of each list<br/>
/// and examples, and a tab-separated file for each list in full. It's made from the <see cref="ReservoirMeasures"/>, set against<br/>
/// <see cref="WhatTheCorpusHas"/>, and the <see cref="SentencesWorthTakingIn"/>.
/// </summary>
public sealed class ReservoirReport
{
   const int RowsInTheReport = 100;
   const int SentencesWantedForAVocab = 3;

   readonly ReservoirFile _reservoir;
   readonly ReservoirMeasures _measures;
   readonly WhatTheCorpusHas _corpus;
   readonly JPCollection _collection;
   readonly SentencesWorthTakingIn _picks;
   readonly DirectoryInfo _folder;
   readonly IReadOnlyDictionary<int, NumberedVocab> _vocabByNumber;
   readonly IReadOnlyDictionary<int, string> _reasonByNumber;
   readonly IReadOnlyDictionary<long, Sources.ReservoirSource> _sources;
   readonly StringBuilder _markdown = new();

   ReservoirReport(ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, JPCollection collection, SentencesWorthTakingIn picks, DirectoryInfo folder)
   {
      _reservoir = reservoir;
      _measures = measures;
      _corpus = corpus;
      _collection = collection;
      _picks = picks;
      _folder = folder;
      _vocabByNumber = measures.Numbers.Vocab.ToDictionary(it => it.Number);
      _reasonByNumber = measures.Numbers.Reasons.ToDictionary(it => it.Value, it => it.Key);
      _sources = reservoir.Sources();
   }

   /// <summary>Writes the report into <paramref name="folder"/>, created if need be.</summary>
   public static void WriteTo(DirectoryInfo folder, ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, JPCollection collection, SentencesWorthTakingIn picks)
   {
      folder.Create();
      var report = new ReservoirReport(reservoir, measures, corpus, collection, picks, folder);
      report.WriteTheReservoirsSize();
      report.WriteTheVocabTheCorpusLacksSentencesFor();
      report.WriteTheRejectionsOnlyTheReservoirHas();
      report.WriteTheMatchConditionsAtWork();
      report.WriteTheVocabTheReservoirAlwaysRejects();
      report.WriteTheWordsNoVocabHas(ReservoirWordKind.Dictionary, "Dictionary-only words the reservoir shows most", "dictionary-only-words");
      report.WriteTheWordsNoVocabHas(ReservoirWordKind.Missing, "Missing words the reservoir shows most: no vocab and no dictionary entry", "missing-words");
      report.WriteThePicks();
      report.WriteTheWorksWithTheMostToGive();
      File.WriteAllText(Path.Combine(folder.FullName, "report.md"), report._markdown.ToString());
   }

   void WriteTheReservoirsSize()
   {
      _markdown.AppendLine(CultureInfo.InvariantCulture, $"# What the reservoir could give the corpus ({DateTime.Now:yyyy-MM-dd HH:mm})").AppendLine();
      Table(["Collection", "Works", "Sentences", "New sentences"], _reservoir.CollectionSummaries().Select(it => new[] { it.Collection, $"{it.Works:N0}", $"{it.Sentences:N0}", $"{it.NewSentences:N0}" }));
      _markdown.AppendLine(CultureInfo.InvariantCulture, $"- **Analyzed:** {_measures.AnalyzedSentences:N0} sentences, against the corpus's {_corpus.SentenceCount:N0}.")
               .AppendLine(CultureInfo.InvariantCulture, $"- **Every displayed word a vocab:** {_measures.SentencesWithEveryDisplayedWordAVocab:N0} sentences, {Percent(_measures.SentencesWithEveryDisplayedWordAVocab, _measures.AnalyzedSentences)}.")
               .AppendLine();
   }

   void WriteTheVocabTheCorpusLacksSentencesFor()
   {
      var vocab = _collection.Vocab.All().Select(it => (Vocab: it, Corpus: _corpus.SentencesMatching(it.GetId().ToString()))).ToList();
      var reservoirCountByNoteId = _measures.SentencesByVocab.ToDictionary(it => _vocabByNumber[it.Key].NoteId, it => it.Value);
      var lacking = vocab.Where(it => it.Corpus < SentencesWantedForAVocab && reservoirCountByNoteId.ContainsKey(it.Vocab.GetId().ToString()))
                         .Select(it => (it.Vocab, it.Corpus, Reservoir: reservoirCountByNoteId[it.Vocab.GetId().ToString()]))
                         .OrderByDescending(it => it.Reservoir.Count)
                         .ToList();
      _markdown.AppendLine(CultureInfo.InvariantCulture, $"## Vocab the corpus has fewer than {SentencesWantedForAVocab} sentences for, that the reservoir has").AppendLine()
               .AppendLine(CultureInfo.InvariantCulture, $"- Of {vocab.Count:N0} vocab, the corpus matches {vocab.Count(it => it.Corpus == 0):N0} in no sentence; the reservoir has sentences for {lacking.Count(it => it.Corpus == 0):N0} of them.")
               .AppendLine(CultureInfo.InvariantCulture, $"- {lacking.Count:N0} vocab have fewer than {SentencesWantedForAVocab} sentences in the corpus and some in the reservoir.").AppendLine();
      var rows = lacking.Select(it => new[] { it.Vocab.GetQuestion(), $"{it.Corpus}", $"{it.Reservoir.Count:N0}", Examples(it.Reservoir) }).ToList();
      TableWithTheRestInAFile("vocab-the-corpus-lacks-sentences-for", ["Vocab", "Corpus", "Reservoir", "Examples"], rows);
   }

   void WriteTheRejectionsOnlyTheReservoirHas()
   {
      var rejections = _measures.SentencesByRejection
                                .Select(it => (Vocab: _vocabByNumber[it.Key.VocabNumber], Reason: _reasonByNumber[it.Key.ReasonNumber], Reservoir: it.Value))
                                .Select(it => (it.Vocab, it.Reason, it.Reservoir, Corpus: _corpus.Rejections(it.Vocab.NoteId, it.Reason)))
                                .ToList();
      var onlyTheReservoirs = rejections.Where(it => it.Corpus == 0).OrderByDescending(it => it.Reservoir.Count).ToList();
      _markdown.AppendLine("## Rejections only the reservoir has: conditions the corpus never tests").AppendLine()
               .AppendLine(CultureInfo.InvariantCulture, $"- Of {rejections.Count:N0} pairs of a vocab and a failure reason rejecting its matches in the reservoir, the corpus has none of {onlyTheReservoirs.Count:N0}.")
               .AppendLine("- Each is a condition, or another requirement, deciding something the corpus never shows it deciding: right or wrong, it's untested.").AppendLine();
      var rows = onlyTheReservoirs.Select(it => new[] { it.Vocab.Question, it.Reason, $"{it.Reservoir.Count:N0}", Examples(it.Reservoir) }).ToList();
      TableWithTheRestInAFile("rejections-only-the-reservoir-has", ["Vocab", "Rejected because", "Sentences", "Examples"], rows);
   }

   /// <summary>
   /// Every condition at the root of every vocab's match-condition, with how many matches it rejects in the corpus and in the reservoir: what it<br/>
   /// decides, the generality measure of <c>dev_docs/matching-configuration.md</c>. One that rejects nothing anywhere decides nothing yet.
   /// </summary>
   void WriteTheMatchConditionsAtWork()
   {
      var reservoirVocabNumberByNoteId = _vocabByNumber.Values.ToDictionary(it => it.NoteId, it => it.Number);
      var reasonNumberByText = _reasonByNumber.ToDictionary(it => it.Value, it => it.Key);
      var conditions = _collection.Vocab.All()
                                  .SelectMany(vocab => vocab.MatchingConfiguration.MatchCondition.Conditions.Select(condition => (Vocab: vocab, Reason: $"not_met::{condition}")))
                                  .Select(it => (it.Vocab, it.Reason, Corpus: _corpus.Rejections(it.Vocab.GetId().ToString(), it.Reason), Reservoir: ReservoirRejections(it.Vocab.GetId().ToString(), it.Reason)))
                                  .OrderBy(it => it.Corpus + (it.Reservoir?.Count ?? 0))
                                  .ToList();
      var decidingNothing = conditions.Where(it => it.Corpus == 0 && it.Reservoir is null).ToList();
      _markdown.AppendLine("## The match-conditions at work: how many matches each condition rejects").AppendLine()
               .AppendLine(CultureInfo.InvariantCulture, $"- {conditions.Count:N0} conditions at the root of {conditions.Select(it => it.Vocab).Distinct().Count():N0} vocab's match-conditions.")
               .AppendLine(CultureInfo.InvariantCulture, $"- {decidingNothing.Count:N0} reject nothing, in the corpus or the reservoir: they decide nothing yet. All conditions, with their counts, are in `match-conditions-at-work.tsv`.").AppendLine();
      Table(["Vocab", "Condition that rejects nothing", "Corpus sentences matching", "Reservoir sentences matching"],
            decidingNothing.Take(RowsInTheReport).Select(it => new[] { it.Vocab.GetQuestion(), it.Reason["not_met::".Length..].ReplaceLineEndings(" "), $"{_corpus.SentencesMatching(it.Vocab.GetId().ToString())}", $"{VocabSentencesInTheReservoir(it.Vocab.GetId().ToString())}" }));
      File.WriteAllLines(Path.Combine(_folder.FullName, "match-conditions-at-work.tsv"),
                         conditions.Select(it => string.Join('\t', it.Vocab.GetQuestion(), it.Reason.ReplaceLineEndings(" "), it.Corpus, it.Reservoir?.Count ?? 0, it.Reservoir is null ? "" : Examples(it.Reservoir)))
                                   .Prepend("Vocab\tCondition\tRejections in the corpus\tRejections in the reservoir\tExamples"));
      return;

      CountWithExamples? ReservoirRejections(string noteId, string reason) =>
         reservoirVocabNumberByNoteId.TryGetValue(noteId, out var vocabNumber) && reasonNumberByText.TryGetValue(reason, out var reasonNumber)
            ? _measures.SentencesByRejection.GetValueOrDefault((vocabNumber, reasonNumber))
            : null;

      int VocabSentencesInTheReservoir(string noteId) =>
         reservoirVocabNumberByNoteId.TryGetValue(noteId, out var vocabNumber) ? _measures.SentencesByVocab.GetValueOrDefault(vocabNumber)?.Count ?? 0 : 0;
   }

   /// <summary>
   /// The vocab whose form reservoir sentences have, but which never matches there. Rejections by design are left out: a poison word's, which<br/>
   /// never matches, and an owned form's, which its owner takes.
   /// </summary>
   void WriteTheVocabTheReservoirAlwaysRejects()
   {
      var alwaysRejected = _measures.SentencesByRejection
                                    .Where(it => !IsByDesign(_reasonByNumber[it.Key.ReasonNumber]))
                                    .GroupBy(it => it.Key.VocabNumber)
                                    .Where(it => !_measures.SentencesByVocab.ContainsKey(it.Key))
                                    .Select(it => (Vocab: _vocabByNumber[it.Key], Rejections: it.Sum(rejection => rejection.Value.Count), Top: it.MaxBy(rejection => rejection.Value.Count)))
                                    .Where(it => it.Rejections >= 10)
                                    .OrderByDescending(it => it.Rejections)
                                    .ToList();
      _markdown.AppendLine("## Vocab the reservoir always rejects: its form is there, but it never matches").AppendLine()
               .AppendLine(CultureInfo.InvariantCulture, $"- {alwaysRejected.Count:N0} vocab match no sentence of the reservoir, though ten or more of its sentences have their form, rejections by design (a poison word, an owned form) left out.")
               .AppendLine("- A condition that's too strict, or a word whose form is truly always another word's: the examples tell which.").AppendLine();
      var rows = alwaysRejected.Select(it => new[] { it.Vocab.Question, $"{it.Rejections:N0}", _reasonByNumber[it.Top.Key.ReasonNumber], Examples(it.Top.Value) }).ToList();
      TableWithTheRestInAFile("vocab-the-reservoir-always-rejects", ["Vocab", "Rejections", "Most often because", "Examples"], rows);
   }

   void WriteTheWordsNoVocabHas(ReservoirWordKind kind, string heading, string fileName)
   {
      var words = _measures.SentencesByUnmatchedWord.Where(it => it.Key.Kind == kind).OrderByDescending(it => it.Value.Count).ToList();
      _markdown.AppendLine(CultureInfo.InvariantCulture, $"## {heading}").AppendLine()
               .AppendLine(CultureInfo.InvariantCulture, $"- {words.Count:N0} distinct words, displayed in {words.Sum(it => it.Value.Count):N0} sentences in all.").AppendLine();
      var rows = words.Select(it => new[] { it.Key.Form, $"{it.Value.Count:N0}", $"{_corpus.SentencesDisplayingUnmatched(it.Key.Form)}", Examples(it.Value) }).ToList();
      TableWithTheRestInAFile(fileName, ["Word", "Reservoir", "Corpus", "Examples"], rows);
   }

   void WriteThePicks()
   {
      var sentences = _reservoir.SentencesWithIds(_picks.Picked.Select(it => it.SentenceId));
      _markdown.AppendLine("## The sentences most worth taking into the working set").AppendLine()
               .AppendLine(CultureInfo.InvariantCulture, $"- {_picks.Picked.Count:N0} sentences picked, each giving the most of what the corpus still lacked: {_picks.Picked.Sum(it => it.WhatItGives.Count):N0} things in all.")
               .AppendLine(CultureInfo.InvariantCulture, $"- {_picks.Picked.Count(it => sentences[it.SentenceId].AudioFile is not null):N0} of them have audio.").AppendLine();
      var rows = _picks.Picked.Select(it => new[] { sentences[it.SentenceId].Text, WorkOf(it.SourceId), sentences[it.SentenceId].AudioFile is null ? "" : "audio", string.Join("; ", it.WhatItGives) }).ToList();
      TableWithTheRestInAFile("sentences-worth-taking-in", ["Sentence", "From", "Audio", "Gives"], rows);
   }

   void WriteTheWorksWithTheMostToGive()
   {
      var works = _picks.GainsBySource.OrderByDescending(it => it.Value).ToList();
      var picksBySource = _picks.Picked.GroupBy(it => it.SourceId).ToDictionary(it => it.Key, it => it.Count());
      _markdown.AppendLine("## The works with the most to give").AppendLine()
               .AppendLine("- What each work's sentences give that the corpus lacks, before any is picked, and how many of the picks are its.").AppendLine();
      var rows = works.Select(it => new[] { _sources[it.Key].Work, _sources[it.Key].Collection, $"{it.Value:N0}", $"{picksBySource.GetValueOrDefault(it.Key)}" }).ToList();
      TableWithTheRestInAFile("works-with-the-most-to-give", ["Work", "Collection", "Gives", "Picked"], rows);
   }

   string WorkOf(long sourceId) => _sources[sourceId].Work;

   static bool IsByDesign(string reason) =>
      reason.StartsWith("forbids::poison_word", StringComparison.Ordinal) || reason.StartsWith("forbids::another_match_owns_the_form", StringComparison.Ordinal);

   string Examples(CountWithExamples count)
   {
      var sentences = _reservoir.SentencesWithIds(count.ExampleIds);
      return string.Join(" / ", count.ExampleIds.Select(id => $"「{sentences[id].Text}」{(sentences[id].AudioFile is null ? "" : "🔊")}"));
   }

   void TableWithTheRestInAFile(string fileName, string[] headings, IReadOnlyList<string[]> rows)
   {
      Table(headings, rows.Take(RowsInTheReport));
      if(rows.Count > RowsInTheReport) _markdown.AppendLine(CultureInfo.InvariantCulture, $"The first {RowsInTheReport} of {rows.Count:N0}; all are in `{fileName}.tsv`.").AppendLine();
      File.WriteAllLines(Path.Combine(_folder.FullName, $"{fileName}.tsv"), rows.Select(it => string.Join('\t', it.Select(cell => cell.Replace('\t', ' ')))).Prepend(string.Join('\t', headings)));
   }

   void Table(string[] headings, IEnumerable<string[]> rows)
   {
      _markdown.AppendLine($"| {string.Join(" | ", headings)} |").AppendLine($"|{string.Concat(headings.Select(_ => "---|"))}");
      foreach(var row in rows) _markdown.AppendLine($"| {string.Join(" | ", row.Select(it => it.Replace("|", "\\|", StringComparison.Ordinal)))} |");
      _markdown.AppendLine();
   }

   static string Percent(long part, long whole) => whole == 0 ? "-" : $"{100.0 * part / whole:F1}%";
}
