using System;
using System.Collections.Generic;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.HeadlessHost;

/// <summary>Refuses Anki's side of importing: the headless JAStudio has no Anki to search, tag or delete raw notes in.</summary>
class HeadlessAnkiRawNotes : IAnkiRawNotes
{
   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes() => throw NoAnki();
   public IReadOnlyList<RawNote> SelectedBy(ImportProfile profile) => throw NoAnki();
   public void TagAsImported(IReadOnlyList<RawNote> rawNotes) => throw NoAnki();
   public void Delete(IReadOnlyList<RawNote> rawNotes) => throw NoAnki();

   static NotSupportedException NoAnki() => new("The headless JAStudio has no Anki to import from.");
}
