using System;
using System.IO;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// The functions JAStudio calls in Anki, as a JAStudio that is a program of its own reaches them: over HTTP, in the server the Anki add-on runs<br/>
/// for it (Python's <c>HttpServerForJAStudio</c>, in <c>jastudio.anki_http</c>). Each group of them is one of its properties.
/// </summary>
/// <remarks>
/// A call fails with <see cref="AnkiRefusedTheCallException"/> while Anki can't let JAStudio use its collection, and with<br/>
/// <see cref="CallToAnkiFailedException"/> when the function fails in Anki.
/// </remarks>
/// <remarks>
/// wip: nothing uses it yet but its specifications, and it has no <see cref="IAnkisWindows"/>: the add-on's server doesn't serve the functions<br/>
/// that drive Anki's windows yet. The plan is in <c>dev_docs/moving-jastudio-out-of-anki.md</c>.
/// </remarks>
public sealed partial class AnkiOverHttp : IDisposable
{
   readonly AnkisHttpServer _server;

   /// <param name="access">How to reach the add-on's HTTP server.</param>
   /// <param name="collection">The file of the Anki collection JAStudio works with: Anki refuses every call while it has another open.</param>
   public AnkiOverHttp(AnkisHttpServerAccess access, FileInfo collection)
   {
      _server = new AnkisHttpServer(access, collection);
      Notes = new ItsNotes(_server);
      Tags = new ItsTags(_server);
      Media = new ItsMedia(_server);
      Database = new ItsDatabase(_server);
   }

   public IAnkisNotes Notes { get; }
   public IAnkisTags Tags { get; }
   public IAnkisMedia Media { get; }
   public IAnkisDatabase Database { get; }

   public void Dispose() => _server.Dispose();
}
