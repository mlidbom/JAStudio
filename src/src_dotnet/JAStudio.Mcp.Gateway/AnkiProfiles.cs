using System;
using System.IO;
using System.Linq;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// Anki's profiles on this computer, each a folder in Anki's data folder with its collection file, <c>collection.anki2</c>, which the headless<br/>
/// JAStudio reads the notes' studying statuses from.
/// </summary>
static class AnkiProfiles
{
   /// <summary>Anki's data folder, where Anki keeps it by default on each platform.</summary>
   static DirectoryInfo AnkiDataFolder => new(Path.Combine(
      Environment.GetFolderPath(OperatingSystem.IsWindows() ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.LocalApplicationData),
      "Anki2"));

   /// <summary>
   /// The collection file of the profile named <paramref name="profileName"/>, or, with none named, of the profile used last: the one whose<br/>
   /// collection file was written last.
   /// </summary>
   public static FileInfo CollectionFileOf(string? profileName) =>
      profileName == null ? TheLastUsedProfilesCollectionFile() : TheCollectionFileOfTheProfileNamed(profileName);

   static FileInfo TheCollectionFileOfTheProfileNamed(string profileName)
   {
      var collectionFile = CollectionFileIn(new DirectoryInfo(Path.Combine(AnkiDataFolder.FullName, profileName)));
      return collectionFile.Exists ? collectionFile : throw new ArgumentException($"Anki has no profile {profileName}: no file {collectionFile.FullName}");
   }

   static FileInfo TheLastUsedProfilesCollectionFile() =>
      AnkiDataFolder.EnumerateDirectories()
                    .Select(CollectionFileIn)
                    .Where(it => it.Exists)
                    .MaxBy(it => it.LastWriteTimeUtc)
   ?? throw new InvalidOperationException($"Anki has no profile with a collection in {AnkiDataFolder.FullName}");

   static FileInfo CollectionFileIn(DirectoryInfo profileFolder) => new(Path.Combine(profileFolder.FullName, "collection.anki2"));
}
