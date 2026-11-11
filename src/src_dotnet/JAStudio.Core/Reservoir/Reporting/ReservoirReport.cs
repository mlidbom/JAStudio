using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches.StateTests.Head;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Analysis.Numbering;
using JAStudio.Core.Reservoir.Measures;
using JAStudio.Core.Reservoir.Picking;
using JAStudio.Core.Reservoir.Sources;
using static System.FormattableString;

namespace JAStudio.Core.Reservoir.Reporting;

/// <summary>
/// The report of what the reservoir could give the corpus, written to a folder (<see cref="ReportFolder"/>): <c>report.md</c>, readable at a<br/>
/// glance, with the top of each list and examples, and a tab-separated file for each list in full. It's made from the<br/>
/// <see cref="ReservoirMeasures"/>, set against <see cref="WhatTheCorpusHas"/>, and the <see cref="SentencesWorthTakingIn"/>.
/// </summary>
public sealed class ReservoirReport
{
   const int SentencesWantedForAVocab = 3;
   const int RejectionsForAVocabToCountAsAlwaysRejected = 10;

   readonly ReservoirFile _reservoir;
   readonly ReservoirMeasures _measures;
   readonly WhatTheCorpusHas _corpus;
   readonly JPCollection _collection;
   readonly SentencesWorthTakingIn _picks;
   readonly ReportFolder _folder;
   readonly IReadOnlyDictionary<long, ReservoirSource> _sources;

   ReservoirReport(ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, JPCollection collection, SentencesWorthTakingIn picks, ReportFolder folder)
   {
      _reservoir = reservoir;
      _measures = measures;
      _corpus = corpus;
      _collection = collection;
      _picks = picks;
      _folder = folder;
      _sources = reservoir.Sources();
   }

   /// <summary>Writes the report into <paramref name="folder"/>, created if need be.</summary>
   public static void WriteTo(DirectoryInfo folder, ReservoirFile reservoir, ReservoirMeasures measures, WhatTheCorpusHas corpus, JPCollection collection, SentencesWorthTakingIn picks)
   {
      var report = new ReservoirReport(reservoir, measures, corpus, collection, picks, new ReportFolder(folder));
      report.WriteTheReservoirsSize();
      report.WriteTheVocabTheCorpusLacksSentencesFor();
      report.WriteTheRejectionsOnlyTheReservoirHas();
      report.WriteTheMatchConditionsAtWork();
      report.WriteTheVocabTheReservoirAlwaysRejects();
      report.WriteTheWordsNoVocabHas(ReservoirWordKind.Dictionary, "Dictionary-only words the reservoir shows most", "dictionary-only-words");
      report.WriteTheWordsNoVocabHas(ReservoirWordKind.Missing, "Missing words the reservoir shows most: no vocab and no dictionary entry", "missing-words");
      report.WriteThePicks();
      report.WriteTheWorksWithTheMostToGive();
      report._folder.SaveTheMarkdown();
   }

   void WriteTheReservoirsSize()
   {
      _folder.Title(Invariant($"What the reservoir could give the corpus ({DateTime.Now:yyyy-MM-dd HH:mm})"));
      _folder.Table(["Collection", "Works", "Sentences", "New sentences"], _reservoir.CollectionSummaries().Select(it => new[] { it.Collection, $"{it.Works:N0}", $"{it.Sentences:N0}", $"{it.NewSentences:N0}" }));
      _folder.Bullets(Invariant($"**Analyzed:** {_measures.AnalyzedSentences:N0} sentences, against the corpus's {_corpus.SentenceCount:N0}."),
                      Invariant($"**Every displayed word a vocab:** {_measures.SentencesWithEveryDisplayedWordAVocab:N0} sentences, {Percent(_measures.SentencesWithEveryDisplayedWordAVocab, _measures.AnalyzedSentences)}."));
   }

   void WriteTheVocabTheCorpusLacksSentencesFor()
   {
      var vocab = _collection.Vocab.All().Select(it => (Vocab: it, Corpus: _corpus.SentencesMatching(NoteIdOf(it)), Reservoir: _measures.SentencesMatching(it))).ToList();
      var lacking = vocab.Where(it => it.Corpus < SentencesWantedForAVocab && it.Reservoir.Count > 0)
                         .OrderByDescending(it => it.Reservoir.Works).ThenByDescending(it => it.Reservoir.Count)
                         .ToList();
      _folder.Section(Invariant($"Vocab the corpus has fewer than {SentencesWantedForAVocab} sentences for, that the reservoir has"),
                      Invariant($"Of {vocab.Count:N0} vocab, the corpus matches {vocab.Count(it => it.Corpus == 0):N0} in no sentence; the reservoir has sentences for {lacking.Count(it => it.Corpus == 0):N0} of them."),
                      Invariant($"{lacking.Count:N0} vocab have fewer than {SentencesWantedForAVocab} sentences in the corpus and some in the reservoir."));
      _folder.TableWithTheRestInAFile("vocab-the-corpus-lacks-sentences-for",
                                      ["Vocab", "Corpus", "Reservoir", "Works", "Examples"],
                                      [.. lacking.Select(it => new[] { it.Vocab.GetQuestion(), $"{it.Corpus}", $"{it.Reservoir.Count:N0}", $"{it.Reservoir.Works}", Examples(it.Reservoir) })]);
   }

   void WriteTheRejectionsOnlyTheReservoirHas()
   {
      var rejections = _measures.SentencesByRejection
                                .Select(it => (Vocab: VocabNumbered(it.Key.VocabNumber), Reason: ReasonNumbered(it.Key.ReasonNumber), Reservoir: it.Value))
                                .Select(it => (it.Vocab, it.Reason, it.Reservoir, Corpus: _corpus.Rejections(it.Vocab.NoteId, it.Reason)))
                                .ToList();
      var onlyTheReservoirs = rejections.Where(it => it.Corpus == 0).OrderByDescending(it => it.Reservoir.Works).ThenByDescending(it => it.Reservoir.Count).ToList();
      _folder.Section("Rejections only the reservoir has: conditions the corpus never tests",
                      Invariant($"Of {rejections.Count:N0} pairs of a vocab and a failure reason rejecting its matches in the reservoir, the corpus has none of {onlyTheReservoirs.Count:N0}."),
                      "Each is a condition, or another requirement, deciding something the corpus never shows it deciding: right or wrong, it's untested.");
      _folder.TableWithTheRestInAFile("rejections-only-the-reservoir-has",
                                      ["Vocab", "Rejected because", "Sentences", "Works", "Examples"],
                                      [.. onlyTheReservoirs.Select(it => new[] { it.Vocab.Question, it.Reason, $"{it.Reservoir.Count:N0}", $"{it.Reservoir.Works}", Examples(it.Reservoir) })]);
   }

   /// <summary>
   /// Every condition at the root of every vocab's match-condition, with how many matches it rejects in the corpus and in the reservoir: what it<br/>
   /// decides, the generality measure of <c>dev_docs/matching-configuration.md</c>. One that rejects nothing anywhere decides nothing yet.
   /// </summary>
   void WriteTheMatchConditionsAtWork()
   {
      var conditions = _collection.Vocab.All()
                                  .SelectMany(vocab => vocab.MatchingConfiguration.MatchCondition.Conditions.Select(condition => (Vocab: vocab, Condition: condition, Reason: FailedMatchRequirement.NotMet(condition).Reason)))
                                  .Select(it => (it.Vocab, it.Condition, it.Reason, Corpus: _corpus.Rejections(NoteIdOf(it.Vocab), it.Reason), Reservoir: _measures.Rejections(it.Vocab, it.Reason)))
                                  .OrderBy(it => it.Corpus + it.Reservoir.Count)
                                  .ToList();
      var decidingNothing = conditions.Where(it => it.Corpus == 0 && it.Reservoir.Count == 0).ToList();
      _folder.Section("The match-conditions at work: how many matches each condition rejects",
                      Invariant($"{conditions.Count:N0} conditions at the root of {conditions.Select(it => it.Vocab).Distinct().Count():N0} vocab's match-conditions."),
                      Invariant($"{decidingNothing.Count:N0} reject nothing, in the corpus or the reservoir: they decide nothing yet. All conditions, with their counts, are in `match-conditions-at-work.tsv`."));
      _folder.Table(["Vocab", "Condition that rejects nothing", "Corpus sentences matching", "Reservoir sentences matching"],
                    decidingNothing.Take(ReportFolder.RowsInTheMarkdown)
                                   .Select(it => new[] { it.Vocab.GetQuestion(), $"{it.Condition}".ReplaceLineEndings(" "), $"{_corpus.SentencesMatching(NoteIdOf(it.Vocab))}", $"{_measures.SentencesMatching(it.Vocab).Count}" }));
      _folder.TabSeparatedFile("match-conditions-at-work",
                               ["Vocab", "Condition", "Rejections in the corpus", "Rejections in the reservoir", "Examples"],
                               conditions.Select(it => new[] { it.Vocab.GetQuestion(), it.Reason.ReplaceLineEndings(" "), $"{it.Corpus}", $"{it.Reservoir.Count}", Examples(it.Reservoir) }));
   }

   /// <summary>
   /// The vocab whose form reservoir sentences have, but which never matches there. Rejections by design are left out: a poison word's, which<br/>
   /// never matches, and an owned form's, which its owner takes.
   /// </summary>
   void WriteTheVocabTheReservoirAlwaysRejects()
   {
      var alwaysRejected = _measures.SentencesByRejection
                                    .Where(it => !IsByDesign(ReasonNumbered(it.Key.ReasonNumber)))
                                    .GroupBy(it => it.Key.VocabNumber)
                                    .Where(it => !_measures.SentencesByVocab.ContainsKey(it.Key))
                                    .Select(it => (Vocab: VocabNumbered(it.Key), Rejections: it.Sum(rejection => rejection.Value.Count), MostOften: it.MaxBy(rejection => rejection.Value.Count)))
                                    .Where(it => it.Rejections >= RejectionsForAVocabToCountAsAlwaysRejected)
                                    .OrderByDescending(it => it.Rejections)
                                    .ToList();
      _folder.Section("Vocab the reservoir always rejects: its form is there, but it never matches",
                      Invariant($"{alwaysRejected.Count:N0} vocab match no sentence of the reservoir, though ten or more of its sentences have their form, rejections by design (a poison word, an owned form) left out."),
                      "A condition that's too strict, or a word whose form is truly always another word's: the examples tell which.");
      _folder.TableWithTheRestInAFile("vocab-the-reservoir-always-rejects",
                                      ["Vocab", "Rejections", "Most often because", "Examples"],
                                      [.. alwaysRejected.Select(it => new[] { it.Vocab.Question, $"{it.Rejections:N0}", ReasonNumbered(it.MostOften.Key.ReasonNumber), Examples(it.MostOften.Value) })]);
   }

   void WriteTheWordsNoVocabHas(ReservoirWordKind kind, string heading, string fileName)
   {
      var words = _measures.SentencesByUnmatchedWord.Where(it => it.Key.Kind == kind).OrderByDescending(it => it.Value.Works).ThenByDescending(it => it.Value.Count).ToList();
      _folder.Section(heading, Invariant($"{words.Count:N0} distinct words, displayed in {words.Sum(it => it.Value.Count):N0} sentences in all."));
      _folder.TableWithTheRestInAFile(fileName,
                                      ["Word", "Reservoir", "Works", "Corpus", "Examples"],
                                      [.. words.Select(it => new[] { it.Key.Form, $"{it.Value.Count:N0}", $"{it.Value.Works}", $"{_corpus.SentencesDisplayingUnmatched(it.Key.Form)}", Examples(it.Value) })]);
   }

   void WriteThePicks()
   {
      var sentences = _reservoir.SentencesWithIds(_picks.Picked.Select(it => it.SentenceId));
      _folder.Section("The sentences most worth taking into the working set",
                      Invariant($"{_picks.Picked.Count:N0} sentences picked, each giving the most of what the corpus still lacked: {_picks.Picked.Sum(it => it.WhatItGives.Count):N0} things in all."),
                      Invariant($"{_picks.Picked.Count(it => SentenceOf(it).AudioFile is not null):N0} of them have audio."));
      _folder.TableWithTheRestInAFile("sentences-worth-taking-in",
                                      ["Sentence", "From", "Audio", "Gives"],
                                      [.. _picks.Picked.Select(it => new[] { SentenceOf(it).Text, WorkOf(it.SourceId), SentenceOf(it).AudioFile is null ? "" : "audio", string.Join("; ", it.WhatItGives) })]);
      return;

      StoredReservoirSentence SentenceOf(SentenceWorthTakingIn pick) => sentences[pick.SentenceId];
   }

   void WriteTheWorksWithTheMostToGive()
   {
      var works = _picks.GainsBySource.OrderByDescending(it => it.Value).ToList();
      var picksBySource = _picks.Picked.GroupBy(it => it.SourceId).ToDictionary(it => it.Key, it => it.Count());
      _folder.Section("The works with the most to give", "What each work's sentences give that the corpus lacks, before any is picked, and how many of the picks are its.");
      _folder.TableWithTheRestInAFile("works-with-the-most-to-give",
                                      ["Work", "Collection", "Gives", "Picked"],
                                      [.. works.Select(it => new[] { _sources[it.Key].Work, _sources[it.Key].Collection, $"{it.Value:N0}", $"{picksBySource.GetValueOrDefault(it.Key)}" })]);
   }

   string WorkOf(long sourceId) => _sources[sourceId].Work;

   NumberedVocab VocabNumbered(int vocabNumber) => _measures.Numbers.VocabNumbered(vocabNumber);

   string ReasonNumbered(int reasonNumber) => _measures.Numbers.ReasonNumbered(reasonNumber);

   /// <summary>The examples of <paramref name="count"/>, each sentence in brackets, marked when it has audio.</summary>
   string Examples(CountWithExamples count)
   {
      var sentences = _reservoir.SentencesWithIds(count.ExampleIds);
      return string.Join(" / ", count.ExampleIds.Select(id => $"「{sentences[id].Text}」{(sentences[id].AudioFile is null ? "" : "🔊")}"));
   }

   /// <summary>Whether <paramref name="reason"/> rejects matches by design: a poison word's, which never matches, or an owned form's, which its owner takes.</summary>
   static bool IsByDesign(string reason) => reason == ForbidsIsPoisonWord.Failed.Reason || reason == ForbidsAnotherMatchIsHigherPriority.Failed.Reason;

   static string NoteIdOf(VocabNote vocab) => vocab.GetId().ToString();

   static string Percent(long part, long whole) => whole == 0 ? "-" : $"{100.0 * part / whole:F1}%";
}
