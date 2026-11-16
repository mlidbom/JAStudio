using System.Collections.Generic;
using JAStudio.Core.Note.Commands;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences.ParsingCases;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// Every command of a sentence's menus (<see cref="INoteCommand{TNote}"/>): splitting a word of its question with a word-break tag, making it a<br/>
/// parsing case (<see cref="ParsingCase"/>), accepting what it parses to, and making it an ordinary sentence again.
/// </summary>
/// <remarks>
/// The sentence's menus are built from these commands, and JAStudio's MCP server runs <see cref="All"/> of them, so the two do the same things.<br/>
/// Each menu item a person runs on a sentence is one of these, other than those that open notes or a dialog, and those that edit a field<br/>
/// (<see cref="SentenceEditableFields"/>), such as its highlighted vocab, its incorrect and hidden matches, and the name, the why and the<br/>
/// verdict of its parsing case.
/// </remarks>
public static class SentenceCommands
{
   /// <summary>
   /// Puts a word-break tag (<see cref="SentenceQuestionField.WordBreakTag"/>) after the first character of a text, wherever it is in the question,<br/>
   /// so the sentence is parsed as if the text were two words.
   /// </summary>
   public static readonly NoteChangingCommandTakingAText<SentenceNote> SplitWithAWordBreakTag =
      new("splitWithAWordBreakTag", "Split with word-break tag in question", "the text of the question to split after its first character", (it, text) => it.Question.SplitTokenWithWordBreakTag(text));

   /// <summary>Makes the sentence a parsing case that expects what it parses to now (<see cref="SentenceNote.MarkAsAParsingCase"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> MarkAsAParsingCase =
      new("markAsAParsingCase", "Mark as a parsing case", it => it.MarkAsAParsingCase());

   /// <summary>Makes what the sentence parses to now what its parsing case expects (<see cref="SentenceNote.AcceptWhatItParsesToAsAParsingCase"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> AcceptWhatItParsesToAsAParsingCase =
      new("acceptWhatItParsesToAsAParsingCase", "Accept what it parses to now", it => it.AcceptWhatItParsesToAsAParsingCase());

   /// <summary>Makes the sentence an ordinary sentence again (<see cref="SentenceNote.StopBeingAParsingCase"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> StopBeingAParsingCase =
      new("stopBeingAParsingCase", "Stop being a parsing case", it => it.StopBeingAParsingCase());

   /// <summary>Every command above, in the same order.</summary>
   public static IReadOnlyList<INoteCommand<SentenceNote>> All { get; } =
   [
      SplitWithAWordBreakTag, MarkAsAParsingCase, AcceptWhatItParsesToAsAParsingCase, StopBeingAParsingCase
   ];
}
