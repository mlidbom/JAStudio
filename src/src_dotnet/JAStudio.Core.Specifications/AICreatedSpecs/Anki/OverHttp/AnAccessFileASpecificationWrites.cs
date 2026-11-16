using System;
using System.IO;
using System.Text.Json;
using JAStudio.Anki.AnkisFunctions.OverHttp;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// An access file (<see cref="AnkisHttpServerAccessFile"/>) in a folder of its own, which a specification writes as the Anki add-on would: to have<br/>
/// one file name first one Anki and then another, as the user's own access file does when Anki restarts or opens another profile, or to have it<br/>
/// name what no add-on would. It isn't there until it's first written. Disposing it removes the folder.
/// </summary>
sealed class AnAccessFileASpecificationWrites : IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-anki-http-access-of-a-specification-");

   public AnAccessFileASpecificationWrites() => AccessFile = new AnkisHttpServerAccessFile(new FileInfo(Path.Combine(_folder.FullName, "anki-http-server-access.json")));

   public AnkisHttpServerAccessFile AccessFile { get; }

   /// <summary>The file now names <paramref name="anki"/>'s server, as when that Anki has started serving JAStudio.</summary>
   public void Names(AnkiServingACopyOfTheTestCollection anki) => Holds(anki.Access);

   /// <summary>The file now holds <paramref name="access"/>, whether or not a server answers to it.</summary>
   public void Holds(AnkisHttpServerAccess access) => File.WriteAllText(AccessFile.File.FullName, JsonSerializer.Serialize(new { port = access.Port, token = access.Token }));

   /// <summary>The file is gone, as when the Anki it named has stopped serving JAStudio.</summary>
   public void IsRemoved() => AccessFile.File.Delete();

   public void Dispose() => _folder.Delete(recursive: true);
}
