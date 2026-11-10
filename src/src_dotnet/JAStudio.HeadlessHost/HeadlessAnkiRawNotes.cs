using System;
using System.Collections.Generic;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.HeadlessHost;

/// <summary>
/// Anki's side of importing, as far as the headless JAStudio can do it without Anki: the note types for raw notes, read from the collection's<br/>
/// database as JAStudio inside Anki reads them, so that the import profiles window opens. The rest it refuses, since there is no Anki to search,<br/>
/// tag or delete raw notes in.
/// </summary>
class HeadlessAnkiRawNotes : IAnkiRawNotes
{
   readonly AnkiCollectionDatabase _database;

   /// <param name="database">The database of the collection the headless JAStudio was started with, its file (<see cref="AnkiCollectionFile"/>).</param>
   public HeadlessAnkiRawNotes(AnkiCollectionDatabase database) => _database = database;

   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes() => NoteTypesInAnki.ReadFrom(_database).ForRawNotes();
   public IReadOnlyList<RawNote> SelectedBy(ImportProfile profile) => throw NoAnki();
   public void TagAsImported(IReadOnlyList<RawNote> rawNotes) => throw NoAnki();
   public void Delete(IReadOnlyList<RawNote> rawNotes) => throw NoAnki();

   static NotSupportedException NoAnki() => new("The headless JAStudio has no Anki to import from.");
}
