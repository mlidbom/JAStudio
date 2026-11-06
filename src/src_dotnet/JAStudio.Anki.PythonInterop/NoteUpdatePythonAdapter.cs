using System;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.PythonInterop;

namespace JAStudio.Anki.PythonInterop;

/// <summary>
/// Adapts Python callable objects to typed .NET delegates for note update listeners.
/// </summary>
public static class NoteUpdatePythonAdapter
{
   /// <summary>
   /// Subscribes a Python callable to <see cref="IExternalNoteUpdateHandler.NoteUpdated"/>, for as long as Anki runs.<br/>
   /// Wraps the Python callable as an <see cref="Action{JPNote}"/> using PythonDotNetShim.
   /// </summary>
   // ReSharper disable once UnusedMember.Global used from python
   public static void Register(IExternalNoteUpdateHandler handler, dynamic pythonCallback)
   {
      Action<JPNote> typed = PythonDotNetShim.Action.ToDotNet<JPNote>(pythonCallback);
      handler.NoteUpdated.Subscribe(typed);
   }
}
