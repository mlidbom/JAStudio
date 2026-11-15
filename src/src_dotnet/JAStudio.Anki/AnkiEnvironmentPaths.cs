using System.IO;
using JAStudio.Anki.AnkisFunctions.ThroughPythonnet;
using JAStudio.Core;

namespace JAStudio.Anki;

/// <summary>
/// The paths of JAStudio running inside Anki: the Anki add-on's folder, which holds JAStudio's user files and its corpus checkout, and Anki's<br/>
/// media folder. Both are asked of Python each time (<see cref="AnkiThroughPythonnet"/>).
/// </summary>
public class AnkiEnvironmentPaths(AnkiThroughPythonnet anki) : IEnvironmentPaths
{
   readonly AnkiThroughPythonnet _anki = anki;

   public string AddonRootDir => _anki.AddOnRootFolder;
   public string AnkiMediaDir => _anki.Media.Folder;
   public string UserFilesDir => Path.Combine(AddonRootDir, "user_files");
   public string DatabaseDir => Path.Combine(AddonRootDir, "jas_database");
   public string MediaDir => Path.Combine(DatabaseDir, "media");
}
