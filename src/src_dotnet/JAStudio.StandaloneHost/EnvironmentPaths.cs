using System.IO;
using JAStudio.Anki.AnkisFunctions;
using JAStudio.Core;

namespace JAStudio.StandaloneHost;

/// <summary>
/// Where the standalone JAStudio finds things: its own in the add-on folder it was started on, a source checkout's <c>src</c> folder, as JAStudio<br/>
/// inside Anki finds them in the add-on folder Anki loads it from. The notes are in the checkout's corpus checkout, <c>jas_database</c>, and the user<br/>
/// files, such as the configuration, the window places and the workspace's state, are the add-on folder's own. Anki's media folder is asked of<br/>
/// the Anki that runs.
/// </summary>
sealed class EnvironmentPaths : IEnvironmentPaths
{
   readonly DirectoryInfo _addOnFolder;
   readonly IAnkisMedia _ankisMedia;

   /// <param name="addOnFolder">The add-on folder the standalone JAStudio was started on.</param>
   /// <param name="ankisMedia">Anki's media, which says where its folder is.</param>
   public EnvironmentPaths(DirectoryInfo addOnFolder, IAnkisMedia ankisMedia)
   {
      _addOnFolder = addOnFolder;
      _ankisMedia = ankisMedia;
   }

   public string AddonRootDir => _addOnFolder.FullName;

   /// <summary>The media folder of the collection Anki has open, asked of Anki each time: only importing reads it. It fails, as any call to Anki, while Anki can't be used.</summary>
   public string AnkiMediaDir => _ankisMedia.Folder;

   public string UserFilesDir => Path.Combine(AddonRootDir, "user_files");
   public string DatabaseDir => Path.Combine(AddonRootDir, "jas_database");
   public string MediaDir => Path.Combine(DatabaseDir, "media");
}
