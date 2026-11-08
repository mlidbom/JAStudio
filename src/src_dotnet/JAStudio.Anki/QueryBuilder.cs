using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.Anki;

/// <summary>
/// Builds Anki search queries, for what only Anki does with notes, such as showing their cards in Anki's browser or previewer.<br/>
/// Finding notes is otherwise JAStudio's own: the workspace's navigation pane lists them.
/// </summary>
public class QueryBuilder
{
   readonly ExternalNoteIdMap _externalNoteIdMap;

   public QueryBuilder(ExternalNoteIdMap externalNoteIdMap) => _externalNoteIdMap = externalNoteIdMap;

   /// <summary>A query for <paramref name="notes"/> in Anki, by their Anki ids. See <see cref="NotesByIds"/>.</summary>
   public string NotesLookup(IEnumerable<JPNote> notes)
   {
      return NotesByIds(notes.Select(n => n.GetId()));
   }

   /// <summary>
   /// A query for the notes with the ids <paramref name="noteIds"/> in Anki, by their Anki ids.<br/>
   /// A note that isn't in Anki has no Anki id, so it's left out; if none of them is in Anki, the query is empty.
   /// </summary>
   string NotesByIds(IEnumerable<NoteId> noteIds)
   {
      var externalIds = noteIds
                       .Select(id => _externalNoteIdMap.ToExternalId(id))
                       .Where(id => id.HasValue)
                       .Select(id => id!.Value)
                       .ToList();
      return externalIds.Count > 0 ? $"{Builtin.NoteId}:{string.Join(",", externalIds)}" : "";
   }
}
