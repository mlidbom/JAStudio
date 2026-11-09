using System.Collections.Generic;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// Every field of a sentence that a person edits in the sentence's editor in the workspace (<see cref="IEditableField{TNote}"/>), in the editor's order:<br/>
/// the corpus's own question, answer, comments and reading; what its source gave it; its audio, screenshot and source name; its tags.
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

   /// <summary>The sentence's text-to-speech audio, as Anki's <c>[sound:file]</c> reference.</summary>
   public static readonly EditableTextField<SentenceNote> Audio =
      new("audio", "Audio", TextFieldFormat.Line, it => it.TextToSpeechAudio.RawValue(), (it, audio) => it.TextToSpeechAudio.SetRawValue(audio));

   /// <summary>The sentence's screenshot, as Anki's <c>&lt;img src="…"&gt;</c> markup.</summary>
   public static readonly EditableTextField<SentenceNote> Screenshot =
      new("screenshot", "Screenshot", TextFieldFormat.Line, it => it.Screenshot.RawValue(), (it, screenshot) => it.Screenshot.SetRawValue(screenshot));

   /// <summary>The name of the source the sentence was imported from (<see cref="SentenceNote.SourceName"/>).</summary>
   public static readonly EditableTextField<SentenceNote> SourceName = new("sourceName", "Source name", TextFieldFormat.Line, it => it.SourceName);

   public static readonly EditableTagsField<SentenceNote> Tags = new();

   /// <summary>Every field above, in the same order.</summary>
   public static IReadOnlyList<IEditableField<SentenceNote>> All { get; } =
   [
      Question, Answer, Comments, Reading, SourceQuestion, SourceAnswer, SourceComments, SourceExternalId, Audio, Screenshot, SourceName, Tags
   ];
}
