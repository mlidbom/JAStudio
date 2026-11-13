using System.Collections.Generic;
using Compze.Contracts;
using JAStudio.Core.Anki;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.StandaloneHost;

/// <summary>
/// Anki's side of importing, as the standalone JAStudio has it: no note types and no notes, since it has no Anki. Importing is offered only while<br/>
/// there's an Anki (<see cref="AnkiAvailability"/>).
/// </summary>
sealed class NoAnkiRawNotes : IAnkiRawNotes
{
   public IReadOnlyList<AnkiNoteType> NoteTypesForRawNotes() => [];
   public IReadOnlyList<RawNote> SelectedBy(ImportProfile profile) => [];
   public void TagAsImported(IReadOnlyList<RawNote> rawNotes) => AssertNoneOf(rawNotes);
   public void Delete(IReadOnlyList<RawNote> rawNotes) => AssertNoneOf(rawNotes);

   /// <summary>Asserts that <paramref name="rawNotes"/>, notes in Anki, are none: without Anki, there are none.</summary>
   static void AssertNoneOf(IReadOnlyList<RawNote> rawNotes) =>
      Contract.Argument.Assert(rawNotes.Count == 0, () => $"The standalone JAStudio has no Anki, so it has no raw notes, but {rawNotes.Count} were given.");
}
