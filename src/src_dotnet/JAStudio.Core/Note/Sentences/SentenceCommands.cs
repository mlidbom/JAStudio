using System.Collections.Generic;
using JAStudio.Core.Note.Commands;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Sentences.ParsingCases;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// Every command of a sentence's menus (<see cref="INoteCommand{TNote}"/>): splitting a word of its question with a word-break tag, and making it a<br/>
/// parsing case (<see cref="ParsingCase"/>), judging it and accepting what it parses to.
/// </summary>
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

   /// <summary>Makes the sentence a parsing case that expects what it parses to now (<see cref="SentenceNote.MarkAsAParsingCase"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> MarkAsAParsingCase =
      new("markAsAParsingCase", "Mark as a parsing case", it => it.MarkAsAParsingCase());

   /// <summary>Names the sentence's parsing case for what it's about.</summary>
   public static readonly NoteChangingCommandTakingAText<SentenceNote> NameTheParsingCase =
      new("nameTheParsingCase", "Name the parsing case", "what the parsing case is about, such as: ichidan imperative", (it, name) => it.NameTheParsingCase(name));

   /// <summary>Says why the sentence's parsing case exists, or what is wrong with how it parses.</summary>
   public static readonly NoteChangingCommandTakingAText<SentenceNote> SayWhyItIsAParsingCase =
      new("sayWhyItIsAParsingCase", "Say why it is a parsing case", "why the parsing case exists, or what is wrong with how it parses", (it, why) => it.SayWhyItIsAParsingCase(why));

   /// <summary>Says that a person has found the vocab the parsing case expects the breakdown to display right (<see cref="Verdict.Verified"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> VerifyTheDisplayedVocab =
      new("verifyTheDisplayedVocab", "The displayed vocab are right: verified", it => it.SetTheVerdictOnTheDisplayedVocab(Verdict.Verified));

   /// <summary>Says that a person has found the vocab the parsing case expects the breakdown to display wrong (<see cref="Verdict.Broken"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> MarkTheDisplayedVocabBroken =
      new("markTheDisplayedVocabBroken", "The displayed vocab are wrong: broken", it => it.SetTheVerdictOnTheDisplayedVocab(Verdict.Broken));

   /// <summary>Takes back a person's verdict on the vocab the parsing case expects the breakdown to display (<see cref="Verdict.Unverified"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> MarkTheDisplayedVocabUnverified =
      new("markTheDisplayedVocabUnverified", "The displayed vocab are unverified", it => it.SetTheVerdictOnTheDisplayedVocab(Verdict.Unverified));

   /// <summary>Makes what the sentence parses to now what its parsing case expects (<see cref="SentenceNote.AcceptWhatItParsesToAsAParsingCase"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> AcceptWhatItParsesToAsAParsingCase =
      new("acceptWhatItParsesToAsAParsingCase", "Parsing case: accept what it parses to now", it => it.AcceptWhatItParsesToAsAParsingCase());

   /// <summary>Makes the sentence an ordinary sentence again (<see cref="SentenceNote.StopBeingAParsingCase"/>).</summary>
   public static readonly NoteChangingCommand<SentenceNote> StopBeingAParsingCase =
      new("stopBeingAParsingCase", "Stop being a parsing case", it => it.StopBeingAParsingCase());

   /// <summary>Every command above, in the same order.</summary>
   public static IReadOnlyList<INoteCommand<SentenceNote>> All { get; } =
   [
      SplitWithAWordBreakTag, MarkAsAParsingCase, NameTheParsingCase, SayWhyItIsAParsingCase, VerifyTheDisplayedVocab, MarkTheDisplayedVocabBroken,
      MarkTheDisplayedVocabUnverified, AcceptWhatItParsesToAsAParsingCase, StopBeingAParsingCase
   ];
}
