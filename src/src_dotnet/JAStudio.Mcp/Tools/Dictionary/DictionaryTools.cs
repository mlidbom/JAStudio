using System;
using System.ComponentModel;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Dictionary;

/// <summary>The tools that show what JAStudio's dictionary says about a word.</summary>
[McpServerToolType]
public class DictionaryTools
{
   readonly DictLookup _dictLookup;
   readonly JPCollection _collection;

   public DictionaryTools(DictLookup dictLookup, JPCollection collection)
   {
      _dictLookup = dictLookup;
      _collection = collection;
   }

   [McpServerTool(Name = "look_up_in_the_dictionary", ReadOnly = true, Idempotent = true)]
   [Description("""
                Looks a word up in JAStudio's dictionary, JMdict, or in its dictionary of names when JMdict has no entry for it, and shows each
                entry found: its kanji and kana forms, and its senses, each with its glosses, the dictionary's own parts of speech and its
                miscellanea, such as "word usually written using kana alone". Each entry also has the parts of speech JAStudio maps the dictionary's
                to, the ones a vocab can have, and the lookup has them for all its entries together. Give either a word, or a vocab's id. A vocab
                is looked up as JAStudio looks it up, by its question, keeping only the entries with one of its readings: the lookup that the
                vocab's "Repopulate parts of speech" sets its parts of speech from.
                """)]
   public DictionaryLookup LookUpInTheDictionary(
      [Description("The word to look up. Give this or vocabNoteId, not both.")] string? word = null,
      [Description("The id of the vocab to look up, as search_notes lists it. Give this or word, not both.")] string? vocabNoteId = null) =>
      DictionaryLookup.Of(LookupOf(word, vocabNoteId));

   DictLookupResult LookupOf(string? word, string? vocabNoteId) => (word, vocabNoteId) switch
   {
      ({} wordToLookUp, null) => _dictLookup.LookupWordOrName(wordToLookUp),
      (null, {} noteId) => _dictLookup.LookupVocabWordOrName(VocabWithTheId(noteId)),
      _ => throw new ArgumentException("Give either word or vocabNoteId, not both, and not neither.")
   };

   VocabNote VocabWithTheId(string noteId) =>
      _collection.Vocab.WithIdOrNone(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No vocab has the id {noteId}.", nameof(noteId));
}
