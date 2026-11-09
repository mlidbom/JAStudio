using System.Collections.Generic;
using JAStudio.Core.Note.Commands;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.Sentences;

/// <summary>Every command of a sentence's menus (<see cref="INoteCommand{TNote}"/>): splitting a word of its question with a word-break tag.</summary>
/// <remarks>
/// The sentence's menus are built from these commands, and JAStudio's MCP server runs <see cref="All"/> of them, so the two do the same things.<br/>
/// Each menu item a person runs on a sentence is one of these, other than those that open notes or a dialog, and those that edit a field<br/>
/// (<see cref="SentenceEditableFields"/>), such as its highlighted vocab and its incorrect and hidden matches.
/// </remarks>
public static class SentenceCommands
{
   /// <summary>
   /// Puts a word-break tag (<see cref="SentenceQuestionField.WordBreakTag"/>) after the first character of a text, wherever it is in the question,<br/>
   /// so the sentence is parsed as if the text were two words.
   /// </summary>
   public static readonly NoteChangingCommandTakingAText<SentenceNote> SplitWithAWordBreakTag =
      new("splitWithAWordBreakTag", "Split with word-break tag in question", "the text of the question to split after its first character", (it, text) => it.Question.SplitTokenWithWordBreakTag(text));

   /// <summary>Every command above, in the same order.</summary>
   public static IReadOnlyList<INoteCommand<SentenceNote>> All { get; } = [SplitWithAWordBreakTag];
}
