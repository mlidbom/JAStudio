using System;
using Compze.Internals.Logging;

namespace JAStudio.Core.Note;

public class NoteFlushGuard
{
   readonly JPNote _note;
   int _depth;

   public NoteFlushGuard(JPNote note)
   {
      _note = note;
      _depth = 0;
   }

   public IDisposable PauseFlushing()
   {
      if(_depth != 0)
      {
         throw new InvalidOperationException("We don't support nested flushing since the complexities have not been figured out yet");
      }

      _depth++;
      return new FlushPauseScope(this);
   }

   bool ShouldFlush() => _depth == 0;

   public bool IsFlushing => _depth > 0;

   public void Flush()
   {
      if(ShouldFlush())
      {
         using(PauseFlushing())
         {
            if(_note.Services.Settings.LogWhenFlushingNotes())
            {
               this.Log().Info($"Flushing {_note.GetType().Name}: {_note.GetQuestion()}");
            }

            _note.UpdateInCache();
         }
      }
   }

   class FlushPauseScope : IDisposable
   {
      readonly NoteFlushGuard _guard;

      public FlushPauseScope(NoteFlushGuard guard) => _guard = guard;

      public void Dispose()
      {
         _guard._depth--;
      }
   }
}
