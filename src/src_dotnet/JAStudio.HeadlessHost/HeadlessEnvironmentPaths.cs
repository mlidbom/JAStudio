using System.IO;
using JAStudio.Core;

namespace JAStudio.HeadlessHost;

/// <summary>
/// Where the headless JAStudio finds things: the add-on's files in a source checkout, as JAStudio inside Anki does, but the notes and their media<br/>
/// in the corpus checkout it was started with, which needn't be the source checkout's own corpus repository.
/// </summary>
class HeadlessEnvironmentPaths : IEnvironmentPaths
{
   readonly HeadlessHostArguments _arguments;

   public HeadlessEnvironmentPaths(HeadlessHostArguments arguments) => _arguments = arguments;

   public string AddonRootDir => _arguments.AddonFolder.FullName;

   /// <summary>The media folder of the Anki profile whose collection the headless JAStudio reads, in the profile's folder, as Anki keeps it.</summary>
   public string AnkiMediaDir => Path.Combine(_arguments.AnkiProfileFolder.FullName, "collection.media");

   public string UserFilesDir => Path.Combine(AddonRootDir, "user_files");
   public string DatabaseDir => _arguments.CorpusFolder.FullName;
   public string MediaDir => Path.Combine(DatabaseDir, "media");
}
