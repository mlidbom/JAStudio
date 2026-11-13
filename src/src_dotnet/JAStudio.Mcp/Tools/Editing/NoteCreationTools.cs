using System;
using System.ComponentModel;
using JAStudio.Core.Batches;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The tool that creates a note from a text, as the context menu's "Create" does for the text selected: a vocab from the dictionary, a sentence or<br/>
/// a kanji.
/// </summary>
[McpServerToolType]
public class NoteCreationTools
{
   /// <summary>The answer the context menu gives a kanji it creates, for a person to replace.</summary>
   const string AnswerOfAKanjiCreated = "TODO";

   readonly VocabNoteFactory _vocabNoteFactory;
   readonly NoteServices _noteServices;
   readonly LocalNoteUpdater _localNoteUpdater;

   public NoteCreationTools(VocabNoteFactory vocabNoteFactory, NoteServices noteServices, LocalNoteUpdater localNoteUpdater)
   {
      _vocabNoteFactory = vocabNoteFactory;
      _noteServices = noteServices;
      _localNoteUpdater = localNoteUpdater;
   }

   [McpServerTool(Name = "create_a_note", Destructive = false)]
   [Description("""
                Creates a note from a text, as the context menu's "Create" does for the text selected, and adds it to the collection: saved to the
                corpus checkout, as any note created, with no study-note in Anki. A vocab is created from the dictionary, with its answer, readings and parts of
                speech and the tag source::jamdict when the dictionary has the word, and empty, tagged source::jastudio as our own, when it hasn't;
                the sentences that may contain it are then reparsed, as the menu does. A sentence is created with the text as its question. A kanji
                is created with the text as its question, the answer "TODO" and the tag source::jastudio, as the menu creates it. Reports the note
                created; edit_a_note edits it further, and delete_a_note deletes it.
                """)]
   public NoteSummary CreateANote(
      [Description("""What note to create: "vocab", "sentence" or "kanji".""")] string noteType,
      [Description("The text to create the note from: the vocab's question, the sentence, or the kanji.")] string text) =>
      NoteSummary.Of(noteType switch
      {
         "vocab"    => CreateAVocab(text),
         "sentence" => SentenceNote.Create(_noteServices, text),
         "kanji"    => KanjiNote.CreateByHand(_noteServices, text, AnswerOfAKanjiCreated),
         _          => throw new ArgumentException($"There is no note type named '{noteType}'. The note types are \"vocab\", \"sentence\" and \"kanji\".", nameof(noteType))
      });

   /// <summary>A vocab created from the dictionary, with the sentences that may contain it reparsed, as the context menu creates one.</summary>
   VocabNote CreateAVocab(string question)
   {
      var vocab = _vocabNoteFactory.CreateWithDictionary(question);
      _localNoteUpdater.ReparseSentencesForVocabWithoutProgressDialog(vocab);
      return vocab;
   }
}
