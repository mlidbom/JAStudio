using System;
using System.ComponentModel;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>The tools that show how JAStudio analyzes text: how it's tokenized, which words it finds, and why each match is or isn't used.</summary>
[McpServerToolType]
public class AnalysisTools
{
   readonly AnalysisServices _analysisServices;
   readonly JPCollection _collection;

   public AnalysisTools(AnalysisServices analysisServices, JPCollection collection)
   {
      _analysisServices = analysisServices;
      _collection = collection;
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
