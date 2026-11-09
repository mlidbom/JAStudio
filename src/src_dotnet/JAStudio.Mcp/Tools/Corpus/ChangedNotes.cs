using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Tools.Corpus;

/// <summary>
/// The notes whose files in the corpus checkout changed, by note type: those that differ from its git <c>HEAD</c>, as <see cref="CorpusTools.ListChangedNotes"/><br/>
/// reports them, or those a fast-forward changed, as <see cref="Reloading.ReloadTools.FastForwardTheCorpusCheckout"/> does.
/// </summary>
/// <param name="IdsOfNotesNotInTheCollection">The ids of changed note files whose notes the collection doesn't have, such as deleted notes.</param>
public record ChangedNotes(NotesOfOneType Vocab, NotesOfOneType Kanji, NotesOfOneType Sentences, IReadOnlyList<string> IdsOfNotesNotInTheCollection)
{
   /// <summary>The notes of <paramref name="collection"/> among <paramref name="idsOfTheChangedNotes"/>, listing at most <paramref name="maxNotesListedPerType"/> of each type.</summary>
   public static ChangedNotes In(JPCollection collection, IReadOnlyList<Guid> idsOfTheChangedNotes, int maxNotesListedPerType)
   {
      var notesById = idsOfTheChangedNotes.ToDictionary(it => it, it => collection.NoteFromNoteId(new NoteId(it)));
      var changedNotes = notesById.Values.OfType<JPNote>().ToList();

      return new ChangedNotes(NotesOfOneType.Of(changedNotes.OfType<VocabNote>(), maxNotesListedPerType),
                              NotesOfOneType.Of(changedNotes.OfType<KanjiNote>(), maxNotesListedPerType),
                              NotesOfOneType.Of(changedNotes.OfType<SentenceNote>(), maxNotesListedPerType),
                              [.. notesById.Where(it => it.Value == null).Select(it => it.Key.ToString())]);
   }
}
