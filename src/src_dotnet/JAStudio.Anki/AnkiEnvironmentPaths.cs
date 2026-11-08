using System.IO;
using JAStudio.Core;

namespace JAStudio.Anki;

/// <summary>
/// Provides environment paths by querying the Anki Python environment.
/// </summary>
public class AnkiEnvironmentPaths : IEnvironmentPaths
{
   public string AddonRootDir => AnkiFacade.GetAddonRootDir();
   public string AnkiMediaDir => AnkiFacade.GetAnkiMediaDir();
   public string UserFilesDir => Path.Combine(AddonRootDir, "user_files");
   public string DatabaseDir => Path.Combine(AddonRootDir, "jas_database");
   public string MediaDir => Path.Combine(DatabaseDir, "media");
}
