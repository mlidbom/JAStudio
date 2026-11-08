namespace JAStudio.Core;

/// <summary>
/// Provides environment-specific path resolution.
/// Implemented by the host integration layer (e.g. Anki addon) for production,
/// and by test infrastructure for testing.
/// Each test instance should get its own implementation with isolated temporary directories.
/// </summary>
public interface IEnvironmentPaths
{
   string AddonRootDir { get; }
   string AnkiMediaDir { get; }
   string UserFilesDir { get; }
   string DatabaseDir { get; }
   string MediaDir { get; }
}
