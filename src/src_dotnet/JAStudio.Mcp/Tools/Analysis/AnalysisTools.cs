using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JAStudio.Core.Batches;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.TaskRunners;
using JAStudio.Mcp.Tools.Timing;
using JAStudio.Mcp.Tools.Memory;
using JAStudio.Mcp.Tools.Memory.Allocations;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using Compze.Threading;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>The tools that show how JAStudio analyzes text: how it's tokenized, which words it finds, and why each match is or isn't used.</summary>
[McpServerToolType]
public class AnalysisTools
{
   /// <summary>
   /// Makes the tools that parse every sentence of the corpus on every core run one at a time: a call made while another runs waits for it.<br/>
   /// Runs at the same time compete for the cores, so their times say nothing, and agents make such calls without meaning to: several calls in one<br/>
   /// message may run at once (2026-11-12).
   /// </summary>
   static readonly IMonitor OneParseOfTheWholeCorpusAtATime = IMonitor.New(LockTimeout.Minutes(10));

   readonly AnalysisServices _analysisServices;
   readonly JPCollection _collection;
   readonly LocalNoteUpdater _localNoteUpdater;

   public AnalysisTools(AnalysisServices analysisServices, JPCollection collection, LocalNoteUpdater localNoteUpdater)
   {
      _analysisServices = analysisServices;
      _collection = collection;
      _localNoteUpdater = localNoteUpdater;
   }

   [McpServerTool(Name = "analyze_sentence", ReadOnly = true, Idempotent = true)]
   [Description("""
                Analyzes a sentence as JAStudio does, and shows every step: the tokens, after JAStudio's preprocessing of the tokenizer's
                output; every candidate word that got a match, at its character index in the sentence, from one token or several; and each of
                its matches, to a vocab, to a dictionary entry, or missing, with whether it's valid, whether it's displayed, and the reasons
                it isn't. The displayed words are what the sentence breakdown shows. A sentence's stored parsing result holds its valid matches.
                Give either the sentence's text, analyzed with no configuration, or a sentence note's id, analyzed with the note's configuration
                of highlighted, incorrect and hidden matches.
                """)]
   public AnalyzedSentence AnalyzeSentence(
      [Description("The text to analyze. Give this or sentenceNoteId, not both.")] string? text = null,
      [Description("The id of the sentence note to analyze, as search_notes lists it. Give this or text, not both.")] string? sentenceNoteId = null,
      [Description("""
                   True, the default, analyzes as the sentence breakdown shows the sentence, which can hide compounds, as configured. False
                   analyzes as the sentence's stored parsing result is made.
                   """)] bool asTheSentenceBreakdownShowsIt = true) =>
      AnalyzedSentence.Of(AnalysisOf(text, sentenceNoteId, asTheSentenceBreakdownShowsIt));

   [McpServerTool(Name = "time_parsing_every_sentence", ReadOnly = true, Idempotent = true)]
   [Description("""
                Times parsing every sentence of the corpus on the given number of threads, twice: tokenizing each sentence's question, the
                tokenizer's share of the work, and then analyzing each sentence as its stored parsing result is made, which includes the
                tokenizing. Nothing is saved. Each pass reports the processor time it used and the machine's other processes used meanwhile,
                how long the garbage collector paused the threads, how much memory it allocated, and the most memory the process used
                meanwhile. For telling whether a change made parsing faster, and how parsing scales with threads: compare runs on the same
                corpus at the same thread count, and discard the first run after a start, which includes warming up. Other development work
                often runs on the machine: a pass during which the other processes used more than a little processor time was disturbed, and
                its time says little. The memory allocated is the same whatever else runs: sample_what_analyzing_every_sentence_allocates
                tells what it goes to. A call made while another call of it, or of count_the_candidate_words or
                sample_what_analyzing_every_sentence_allocates, runs waits for that one to finish, since runs at the same time would compete
                for the cores.
                """)]
   public TimeToParseEverySentence TimeParsingEverySentence(
      [Description("The threads to parse on. By default one per logical core of the machine.")] int? threads = null,
      [Description("""
                   True, the default, first collects all garbage, as describe_memory does, so that every run starts from the same memory, and
                   its peak memory use is its own rather than left over from an earlier run.
                   """)] bool collectAllGarbageFirst = true)
   {
      return OneParseOfTheWholeCorpusAtATime.Locked(TimeIt);

      TimeToParseEverySentence TimeIt()
      {
         if(collectAllGarbageFirst) TheGarbageCollector.CollectAllGarbage();
         var sentences = _collection.Sentences.All().ToList();
         var threadCount = ThreadCount.WithThreads(threads ?? Environment.ProcessorCount);
         var tokenizing = TimedPass.Of(() => Parallel.ForEach(sentences, threadCount.ParallelOptions, it => JNTokenizer.GetInstance().Tokenize(it.Question.WithInvisibleSpace())));
         var analyzing = TimedPass.Of(() => Parallel.ForEach(sentences, threadCount.ParallelOptions, it => it.CreateAnalysis()));
         return new TimeToParseEverySentence(sentences.Count, threadCount.Threads, TheGarbageCollector.Mode, tokenizing, analyzing);
      }
   }

   [McpServerTool(Name = "time_reparsing_the_sentences_of_a_vocab", Destructive = false, Idempotent = true)]
   [Description("""
                Times the reparse the vocab editor runs after each change to a vocab, for the vocab with the given id: it reparses the sentences
                that may contain the vocab, and those it's matched in now, on the reanalysis threads, and updates the vocab's generated data
                before and after. Reports how many sentences it reparsed, how long finding them took, how long the whole reparse took, and the
                processor time it used and the machine's other processes used meanwhile. For telling how long editing a vocab keeps the editor
                waiting, and why: run it on a vocab with few sentences and on one with many, right after a start and again. On a corpus that's
                up to date it changes no note; a sentence or vocab it does change is saved, as the editor's reparse saves it. A call made while
                another parse of the corpus runs, such as time_parsing_every_sentence, waits for it to finish.
                """)]
   public TimeToReparseTheSentencesOfAVocab TimeReparsingTheSentencesOfAVocab(
      [Description("The id of the vocab note, as search_notes lists it.")] string vocabNoteId)
   {
      return OneParseOfTheWholeCorpusAtATime.Locked(TimeIt);

      TimeToReparseTheSentencesOfAVocab TimeIt()
      {
         var vocab = _collection.Vocab.WithIdOrNone(NoteId.Parse(vocabNoteId)) ?? throw new ArgumentException($"No vocab note has the id {vocabNoteId}.", nameof(vocabNoteId));

         var finding = Stopwatch.StartNew();
         var sentences = _localNoteUpdater.SentencesToReparseAfterAChangeTo(vocab).Count;
         var secondsFindingTheSentences = finding.Elapsed.TotalSeconds;

         var processorTimeUsedSinceTheStart = ProcessorTimeUsed.MeasuringFromNow();
         var reparsing = Stopwatch.StartNew();
         _localNoteUpdater.ReparseSentencesForVocabWithoutProgressDialog(vocab);
         var secondsTheReparseTook = reparsing.Elapsed.TotalSeconds;

         return new TimeToReparseTheSentencesOfAVocab(vocab.Question.DisambiguationName, sentences, secondsFindingTheSentences, secondsTheReparseTook, processorTimeUsedSinceTheStart());
      }
   }

   [McpServerTool(Name = "sample_what_analyzing_every_sentence_allocates", ReadOnly = true, Idempotent = true)]
   [Description("""
                Analyzes every sentence of the corpus as its stored parsing result is made, on the given number of threads, while sampling what
                the process allocates, by type: the runtime reports an allocation about every 100 KB each heap allocates, with the type of the
                object, and the reports are summed by type. Reports how much the pass allocated, counted exactly, how much of it the samples
                cover, and the types that took the most, each with its megabytes and its share of the sampled. For finding what parsing could
                allocate less of, which a profiler's times don't show: it allocates gigabytes a pass. A sample, not a count: the share of a type
                allocated often is right, and one allocated rarely may be missed. Nothing is saved. A call made while another parse of every
                sentence runs, such as time_parsing_every_sentence, waits for it to finish.
                """)]
   public WhatAnalyzingEverySentenceAllocates SampleWhatAnalyzingEverySentenceAllocates(
      [Description("The threads to parse on. By default one per logical core of the machine.")] int? threads = null,
      [Description("The most types to list, the one that took the most first.")] int maxTypesListed = 40)
   {
      return OneParseOfTheWholeCorpusAtATime.Locked(SampleIt);

      WhatAnalyzingEverySentenceAllocates SampleIt()
      {
         var sentences = _collection.Sentences.All().ToList();
         var threadCount = ThreadCount.WithThreads(threads ?? Environment.ProcessorCount);
         var allocatedBytesBefore = GC.GetTotalAllocatedBytes(precise: true);
         using var sampler = new AllocationSampler();
         Parallel.ForEach(sentences, threadCount.ParallelOptions, it => it.CreateAnalysis());
         var sampled = sampler.StopSampling(maxTypesListed);
         var allocatedMegabytes = Math.Round((GC.GetTotalAllocatedBytes(precise: true) - allocatedBytesBefore) / 1_000_000.0, 1);
         return new WhatAnalyzingEverySentenceAllocates(sentences.Count, threadCount.Threads, allocatedMegabytes, sampled);
      }
   }

   [McpServerTool(Name = "count_the_candidate_words", ReadOnly = true, Idempotent = true)]
   [Description("""
                Analyzes every sentence of the corpus as its stored parsing result is made, on every core, and counts the candidate words the
                analysis built: at each token, one for each number of tokens it may span, as long as a vocab form or a dictionary word form
                starts with the surface of all its tokens but the last. It counts, in all and by how many tokens the words span: how many were
                built, how many have a form a vocab or a dictionary entry has, the only ones that can get a match, and how many got a valid
                match. A compound none of whose forms a vocab or a dictionary entry has is analyzed for nothing. For telling how much of the
                analysis's work goes to such candidate words, and how a change to which candidate words are built changes it. Nothing is saved.
                """)]
   public CandidateWordCounts CountTheCandidateWords()
   {
      return OneParseOfTheWholeCorpusAtATime.Locked(CountThem);

      CandidateWordCounts CountThem()
      {
         var sentences = _collection.Sentences.All().ToList();
         var tally = sentences.AsParallel()
                              .Aggregate(() => new CandidateWordTally(),
                                         (threadsTally, sentence) => threadsTally.AddEveryCandidateWordOf(sentence.CreateAnalysis()),
                                         (tally, otherTally) => tally.Add(otherTally),
                                         it => it);
         return new CandidateWordCounts(sentences.Count, tally.ByTokensSpanned());
      }
   }

   TextAnalysis AnalysisOf(string? text, string? sentenceNoteId, bool forTheSentenceBreakdown) => (text, sentenceNoteId, forTheSentenceBreakdown) switch
   {
      ({} sentenceText, null, true) => new TextAnalysis(_analysisServices, sentenceText, SentenceConfiguration.Empty(), _analysisServices.Settings.BreakdownParsingOptions()),
      ({} sentenceText, null, false) => new TextAnalysis(_analysisServices, sentenceText, SentenceConfiguration.Empty()),
      (null, {} noteId, true) => SentenceNoteWithTheId(noteId).CreateBreakdownAnalysis(_analysisServices.Settings.BreakdownParsingOptions()),
      (null, {} noteId, false) => SentenceNoteWithTheId(noteId).CreateAnalysis(),
      _ => throw new ArgumentException("Give either text or sentenceNoteId, not both, and not neither.")
   };

   SentenceNote SentenceNoteWithTheId(string noteId) =>
      _collection.Sentences.WithIdOrNone(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No sentence note has the id {noteId}.", nameof(noteId));
}
