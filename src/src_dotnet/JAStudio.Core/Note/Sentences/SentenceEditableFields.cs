using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// Every field of a sentence that a person edits (<see cref="IEditableField{TNote}"/>): first those of the sentence's editor in the workspace, in the<br/>
/// editor's order, the corpus's own question, answer, comments and reading, what its source gave it, its audio, screenshot and source name, and its tags;<br/>
/// then its configuration, the vocab highlighted in it and the matches hidden or marked incorrect, which the sentence's menus edit.
/// </summary>
/// <remarks>
/// The sentence's editor is built from these fields, and JAStudio's MCP server edits <see cref="All"/> of them, so the two edit the same fields.
/// </remarks>
public static class SentenceEditableFields
{
   /// <summary>The corpus's own question, which takes the place of the source's (<see cref="SourceQuestion"/>).</summary>
   public static readonly EditableTextField<SentenceNote> Question = new("corpusQuestion", "Question", TextFieldFormat.Paragraphs, it => it.CorpusQuestion);

   /// <summary>The corpus's own answer, which takes the place of the source's (<see cref="SourceAnswer"/>).</summary>
   public static readonly EditableTextField<SentenceNote> Answer = new("corpusAnswer", "Answer", TextFieldFormat.Paragraphs, it => it.CorpusAnswer);

   /// <summary>The corpus's own comments, shown beside the source's (<see cref="SourceComments"/>).</summary>
   public static readonly EditableTextField<SentenceNote> Comments = new("comments", "Comments", TextFieldFormat.Paragraphs, it => it.Comments);

   public static readonly EditableTextField<SentenceNote> Reading = new("reading", "Reading", TextFieldFormat.Line, it => it.Reading);

   /// <summary>The question the sentence's source gave it (<see cref="SentenceNote.SourceData"/>).</summary>
   public static readonly EditableTextField<SentenceNote> SourceQuestion = new("sourceData.question", "Question", TextFieldFormat.Paragraphs, it => it.SourceData.Question);

   /// <summary>The answer the sentence's source gave it.</summary>
   public static readonly EditableTextField<SentenceNote> SourceAnswer = new("sourceData.answer", "Answer", TextFieldFormat.Paragraphs, it => it.SourceData.Answer);

   /// <summary>The comments the sentence's source gave it.</summary>
   public static readonly EditableTextField<SentenceNote> SourceComments = new("sourceData.comments", "Comments", TextFieldFormat.Paragraphs, it => it.SourceData.Comments);

   /// <summary>The id the sentence has in its source.</summary>
   public static readonly EditableTextField<SentenceNote> SourceExternalId = new("sourceData.externalId", "External id", TextFieldFormat.Line, it => it.SourceData.ExternalId);

   /// <summary>The name of the source the sentence was imported from (<see cref="SentenceNote.SourceName"/>).</summary>
   public static readonly EditableTextField<SentenceNote> SourceName = new("sourceName", "Source name", TextFieldFormat.Line, it => it.SourceName);

   public static readonly EditableTagsField<SentenceNote> Tags = new();

   /// <summary>The vocab the sentence's breakdown highlights, each by its question or disambiguation name. Changing them parses the sentence again.</summary>
   public static readonly EditableTextSetField<SentenceNote> HighlightedVocab =
      new("configuration.highlightedWords", "Highlighted vocab", it => it.Configuration.HighlightedWords, (it, vocab) => it.Configuration.AddHighlightedWord(vocab), (it, vocab) => it.Configuration.RemoveHighlightedWord(vocab));

   /// <summary>The words the sentence's breakdown must not match, anywhere or at one index. Changing them parses the sentence again.</summary>
   public static readonly EditableSetField<SentenceNote, WordExclusion> IncorrectMatches = WordExclusions("configuration.incorrectMatches", "Incorrect matches", it => it.IncorrectMatches);

   /// <summary>The words the sentence's breakdown matches but doesn't show, anywhere or at one index. Changing them parses the sentence again.</summary>
   public static readonly EditableSetField<SentenceNote, WordExclusion> HiddenMatches = WordExclusions("configuration.hiddenMatches", "Hidden matches", it => it.HiddenMatches);

   /// <summary>Every field above, in the same order.</summary>
   public static IReadOnlyList<IEditableField<SentenceNote>> All { get; } =
   [
      Question, Answer, Comments, Reading, SourceQuestion, SourceAnswer, SourceComments, SourceExternalId, SourceName, Tags,
      HighlightedVocab, IncorrectMatches, HiddenMatches
   ];

   /// <summary>One of the sentence's sets of <see cref="WordExclusion"/>s, which <paramref name="exclusions"/> finds, in the order of their words, then their indexes.</summary>
   static EditableSetField<SentenceNote, WordExclusion> WordExclusions(string name, string label, Func<CachingSentenceConfigurationField, WordExclusionSet> exclusions) =>
      new(name,
          label,
          it => exclusions(it.Configuration).Get(),
          (it, exclusion) => exclusions(it.Configuration).Add(exclusion),
          (it, exclusion) => exclusions(it.Configuration).Remove(exclusion),
          inOrder: all => all.OrderBy(it => it.Word, StringComparer.Ordinal).ThenBy(it => it.Index));
}
