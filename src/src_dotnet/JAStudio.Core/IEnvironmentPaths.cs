using System;
using System.IO;

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

   /// <summary>
   /// The text-to-speech settings file: the tts-providers' keys and the chosen voices. Unless the environment says otherwise, in the user's profile,<br/>
   /// outside every repository, so that the keys are never committed: <c>%APPDATA%\JAStudio\text-to-speech.json</c>. See <c>dev_docs/text-to-speech.md</c>.
   /// </summary>
   /// <remarks>The specifications' environment has it in their own folder, where there is none, so they never read the user's keys, nor write over them.</remarks>
   string TextToSpeechSettingsFile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JAStudio", "text-to-speech.json");
}
