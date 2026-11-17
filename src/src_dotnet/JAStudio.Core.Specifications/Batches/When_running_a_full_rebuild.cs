using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Batches;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Batches;

public class When_running_a_full_rebuild : SpecificationUsingACollection
{
   protected readonly LocalNoteUpdater _localNoteUpdater;

   public When_running_a_full_rebuild()
   {
      _localNoteUpdater = GetService<LocalNoteUpdater>();
      _localNoteUpdater.FullRebuild();
   }

   public class and_then_another_one : When_running_a_full_rebuild
   {
      readonly int _teventsObservedBeforeTheSecond;

      public and_then_another_one()
      {
         _teventsObservedBeforeTheSecond = TeventsObservedSoFar().Count;
         _localNoteUpdater.FullRebuild();
      }

      /// <summary>The type and question of each note that published a tevent in the second rebuild, each once.</summary>
      IReadOnlyList<string> NotesTheSecondRebuildChanged()
      {
         var collection = GetService<JPCollection>();
         return [.. TeventsObservedSoFar().Skip(_teventsObservedBeforeTheSecond).OfType<INoteTevent>().Select(it => it.NoteId).Distinct()
                                         .Select(it => collection.NoteFromNoteId(it) is {} note ? $"{note.GetType().Name} {note.GetQuestion()}" : "<not in the collection>")];
      }

      [XF] public void the_second_changes_no_note() => NotesTheSecondRebuildChanged().Must().BeEmpty();
   }
}
