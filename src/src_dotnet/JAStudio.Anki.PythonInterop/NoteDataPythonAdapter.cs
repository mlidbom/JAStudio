using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.PythonInterop;

namespace JAStudio.Anki.PythonInterop;

/// <summary>
/// Adapts Python data types to C# domain types at the Python interop boundary.
/// </summary>
// ReSharper disable once UnusedType.Global Used from python
public static class NoteDataPythonAdapter
{
   /// <summary>
   /// Creates NoteData from a Python object with .fields (dict) and .tags (list) attributes.
   /// The domain NoteId is NOT set here — it must be assigned by the caller
   /// (e.g. the sync handler or bulk loader) since Python only knows external IDs.
   /// </summary>
   // ReSharper disable once UnusedMember.Global used from python
   public static NoteData FromPython(dynamic item)
   {
      var fields = PythonDotNetShim.StringStringDict.ToDotNet(item.fields);
      var tags = PythonDotNetShim.StringList.ToDotNet(item.tags);
      return new NoteData(null, fields, tags);
   }
}
