using System;
using System.IO;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// Creates an isolated temporary directory tree for each test instance.
/// Disposed automatically when CoreApp is disposed, cleaning up all temp files.
/// </summary>
public class TestEnvironmentPaths : IEnvironmentPaths, IDisposable
{
   public TestEnvironmentPaths()
   {
      AddonRootDir = Path.Combine(Path.GetTempPath(), $"JAStudio_test_{Guid.NewGuid():N}");
      Directory.CreateDirectory(UserFilesDir);
      Directory.CreateDirectory(DatabaseDir);
      Directory.CreateDirectory(MediaDir);
   }

   public string AddonRootDir { get; }
   public string AnkiMediaDir => Path.Combine(AddonRootDir, "anki_media");
   public string UserFilesDir => Path.Combine(AddonRootDir, "user_files");
   public string DatabaseDir => Path.Combine(AddonRootDir, "jas_database");
   public string MediaDir => Path.Combine(DatabaseDir, "media");

   public void Dispose()
   {
      try { Directory.Delete(AddonRootDir, recursive: true); }
      catch { /* cleanup is best-effort */ }
   }
}
