using System;
using JAStudio.PythonInterop.Utilities;

namespace JAStudio.Anki.AnkisFunctions.ThroughPythonnet;

/// <summary>
/// The functions JAStudio calls in Anki, as JAStudio running inside Anki reaches them: through pythonnet, in the Anki add-on's Python module<br/>
/// <c>jastudio.dotnet.anki_facade_backend</c>. Each group of them is one of its properties.
/// </summary>
/// <remarks>
/// Every function is safe to call from any .NET thread, the Avalonia UI thread included: those that drive Anki's windows run their work on Qt's<br/>
/// main thread themselves, and the others call Anki's collection on the calling thread, which Anki's backend allows from any thread.
/// </remarks>
/// <remarks>
/// It goes when the Anki add-on stops loading .NET: JAStudio then calls the same functions over HTTP (<c>dev_docs/moving-jastudio-out-of-anki.md</c>).
/// </remarks>
public sealed partial class AnkiThroughPythonnet
{
   readonly Lazy<PythonObjectWrapper> _backend = new(() => PythonEnvironment.Import("jastudio.dotnet.anki_facade_backend"));

   public AnkiThroughPythonnet()
   {
      Notes = new ItsNotes(this);
      Tags = new ItsTags(this);
      Media = new ItsMedia(this);
      Database = new ItsDatabase(this);
      Windows = new ItsWindows(this);
   }

   public IAnkisNotes Notes { get; }
   public IAnkisTags Tags { get; }
   public IAnkisMedia Media { get; }
   public IAnkisDatabase Database { get; }
   public IAnkisWindows Windows { get; }

   /// <summary>
   /// The folder of the Anki add-on, which holds JAStudio's user files and its corpus checkout while JAStudio runs inside Anki. It isn't one of<br/>
   /// Anki's functions: the add-on answers for itself, and a JAStudio that is a program of its own has a folder of its own.
   /// </summary>
   public string AddOnRootFolder => Backend.Use(it => (string)it.addon_root_dir());

   /// <summary>The add-on's Python module, imported when the first function is called.</summary>
   PythonObjectWrapper Backend => _backend.Value;
}
