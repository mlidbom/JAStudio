using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using JAStudio.TextToSpeech.Azure;
using JAStudio.TextToSpeech.Google;

namespace JAStudio.TextToSpeech;

/// <summary>
/// The keys of the tts-providers, from a JSON file in the user's profile, outside every repository: <see cref="FileInTheUserProfile"/>.<br/>
/// A tts-provider without settings isn't offered, and with no file, none is.
/// </summary>
/// <example>
/// <code>
/// {
///   "azure": { "key": "…", "region": "swedencentral" },
///   "google": { "apiKey": "…" }
/// }
/// </code>
/// </example>
public sealed class TextToSpeechSettings
{
   static readonly JsonSerializerOptions TheFilesJson = new(JsonSerializerDefaults.Web);

   /// <summary><c>%APPDATA%\JAStudio\text-to-speech.json</c>.</summary>
   public static string FileInTheUserProfile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JAStudio", "text-to-speech.json");

   public AzureSpeechSettings? Azure { get; init; }

   public GoogleTextToSpeechSettings? Google { get; init; }

   /// <summary>The settings in <see cref="FileInTheUserProfile"/>.</summary>
   public static TextToSpeechSettings ReadFromTheUserProfile() => ReadFrom(FileInTheUserProfile);

   /// <summary>The settings in the file at <paramref name="filePath"/>, or none, when there is no file there.</summary>
   public static TextToSpeechSettings ReadFrom(string filePath) =>
      File.Exists(filePath)
         ? JsonSerializer.Deserialize<TextToSpeechSettings>(File.ReadAllText(filePath), TheFilesJson) ?? throw new InvalidDataException($"{filePath} holds no settings.")
         : new TextToSpeechSettings();

   /// <summary>A tts-provider for each of the services these settings have a key for, calling it through <paramref name="http"/>.</summary>
   public IReadOnlyList<ITextToSpeechProvider> ProvidersWithKeys(HttpClient http) =>
   [
      ..Azure is {} azure ? [new AzureSpeechProvider(azure, http)] : Array.Empty<ITextToSpeechProvider>(),
      ..Google is {} google ? [new GoogleTextToSpeechProvider(google, http)] : Array.Empty<ITextToSpeechProvider>()
   ];
}
