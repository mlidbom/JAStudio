using System.Collections.Concurrent;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Batches;
using JAStudio.Core.Note.Collection;

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
      readonly ConcurrentQueue<string> _theNotesTheSecondRebuildUpdated = new();

      public and_then_another_one()
      {
         GetService<JPCollection>().NoteUpdated.Subscribe(it => _theNotesTheSecondRebuildUpdated.Enqueue($"{it.GetType().Name} {it.GetQuestion()}"));
         _localNoteUpdater.FullRebuild();
      }

      [XF] public void the_second_updates_no_note() => _theNotesTheSecondRebuildUpdated.Must().BeEmpty();
   }
}
