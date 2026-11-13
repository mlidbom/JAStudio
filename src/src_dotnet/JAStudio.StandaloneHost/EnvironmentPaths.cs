using System;
using System.IO;
using JAStudio.Core;

namespace JAStudio.StandaloneHost;

/// <summary>
/// Where the standalone JAStudio finds things: all of it in the add-on folder it was started on, a source checkout's <c>src</c> folder, as JAStudio<br/>
/// inside Anki finds them in the add-on folder Anki loads it from. The notes are in the checkout's corpus checkout, <c>jas_database</c>, and the user<br/>
/// files, such as the configuration, the window places and the workspace's state, are the add-on folder's own.
/// </summary>
sealed class EnvironmentPaths : IEnvironmentPaths
{
   readonly DirectoryInfo _addOnFolder;

   public EnvironmentPaths(DirectoryInfo addOnFolder) => _addOnFolder = addOnFolder;

   public string AddonRootDir => _addOnFolder.FullName;

   /// <summary>Refused: there's no Anki, so no Anki media folder. Only importing reads it, and importing is offered only while there's an Anki.</summary>
   public string AnkiMediaDir => throw new InvalidOperationException("The standalone JAStudio has no Anki, so no Anki media folder to import media from.");

   public string UserFilesDir => Path.Combine(AddonRootDir, "user_files");
   public string DatabaseDir => Path.Combine(AddonRootDir, "jas_database");
   public string MediaDir => Path.Combine(DatabaseDir, "media");
}
