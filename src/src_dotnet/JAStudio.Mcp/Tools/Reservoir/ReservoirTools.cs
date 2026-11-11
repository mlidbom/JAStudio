using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Reservoir.Feedback;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Reservoir;

/// <summary>
/// The tools that work with a reservoir, a SQLite file of sentences imported but not loaded or parsed in normal use<br/>
/// (<see cref="JAStudio.Core.Reservoir.ReservoirFile"/>, <c>dev_docs/todo/expanding-the-sentence-corpus.md</c>).
/// </summary>
[McpServerToolType]
public class ReservoirTools
{
   readonly AnalysisServices _analysisServices;
   readonly OpenReservoirs _openReservoirs;

   public ReservoirTools(AnalysisServices analysisServices, OpenReservoirs openReservoirs)
   {
      _analysisServices = analysisServices;
      _openReservoirs = openReservoirs;
   }

   [McpServerTool(Name = "what_a_vocab_does_in_the_reservoir", ReadOnly = true, Idempotent = true)]
   [Description("""
                Tells what a vocab does, as JAStudio has it now, in the millions of sentences of a reservoir: the sentences that could hold it,
                those containing one of its forms or of their stems, are analyzed anew, a sample of them when there are many, and set against
                the reservoir's bulk analysis. It says in which sentences the vocab matches, which reject its matches and why, and in which its
                matching changed since the bulk analysis, which is what an edit to the vocab did. The reservoir's texts load at its first use,
                in up to a minute; every later use takes seconds.
                """)]
   public VocabInTheReservoir WhatAVocabDoesInTheReservoir(
      [Description("The vocab: its note id, as search_notes lists it, or its question, when only one vocab has it.")] string vocab,
      [Description("The reservoir's file, such as E:\\SUBS2SRS\\reservoir.sqlite.")] string reservoirFile,
      [Description("The most sentences to analyze. A vocab whose forms are in more has an evenly spread sample of them analyzed.")] int mostSentencesAnalyzed = 20_000)
   {
      var (reservoir, texts) = _openReservoirs.In(new FileInfo(reservoirFile));
      return VocabInTheReservoir.Of(Core.Reservoir.Feedback.WhatAVocabDoesInTheReservoir.Of(TheVocab(vocab), reservoir, texts, _analysisServices, mostSentencesAnalyzed));
   }

   VocabNote TheVocab(string noteIdOrQuestion)
   {
      if(Guid.TryParse(noteIdOrQuestion, out _))
         return _analysisServices.Vocab.WithIdOrNone(NoteId.Parse(noteIdOrQuestion)) ?? throw new ArgumentException($"No vocab has the id {noteIdOrQuestion}.");

      return _analysisServices.Vocab.WithQuestion(noteIdOrQuestion) switch
      {
         [var only] => only,
         [] => throw new ArgumentException($"No vocab has the question {noteIdOrQuestion}."),
         var several => throw new ArgumentException($"{several.Count} vocab have the question {noteIdOrQuestion}: give one's note id, {string.Join(", ", several.Select(it => it.GetId()))}.")
      };
   }
}
