using System.IO;
using JAStudio.Core;

namespace JAStudio.HeadlessHost;

/// <summary>
/// Where the headless JAStudio finds things: the add-on's files in a source checkout, as JAStudio inside Anki does, but the notes and their media<br/>
/// in the corpus checkout it was started with, which needn't be the source checkout's own corpus repository, and the user files in a copy of<br/>
/// its own (<see cref="HeadlessUserFiles"/>).
/// </summary>
class HeadlessEnvironmentPaths : IEnvironmentPaths
{
   readonly HeadlessJAStudioSetup _setup;

   public HeadlessEnvironmentPaths(HeadlessJAStudioSetup setup) => _setup = setup;

   public string AddonRootDir => _setup.AddonFolder.FullName;

   /// <summary>The media folder of the Anki profile whose collection the headless JAStudio reads, in the profile's folder, as Anki keeps it.</summary>
   public string AnkiMediaDir => Path.Combine(_setup.AnkiProfileFolder.FullName, "collection.media");

   public string UserFilesDir => HeadlessUserFiles.Folder.FullName;
   public string DatabaseDir => _setup.CorpusFolder.FullName;
   public string MediaDir => Path.Combine(DatabaseDir, "media");
}
