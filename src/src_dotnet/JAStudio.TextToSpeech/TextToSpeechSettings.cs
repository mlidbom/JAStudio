using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.TextToSpeech.Azure;
using JAStudio.TextToSpeech.Google;

namespace JAStudio.TextToSpeech;

/// <summary>
/// The text-to-speech settings: the tts-providers' keys and the <see cref="ChosenVoices"/>, from a JSON file in the user's profile,<br/>
/// outside every repository: <see cref="FileInTheUserProfile"/>. A tts-provider without settings isn't offered, and with no file, none is.
/// </summary>
/// <example>
/// <code>
/// {
///   "azure": { "key": "…", "region": "swedencentral" },
///   "google": { "apiKey": "…" },
///   "chosenVoices": [ { "provider": "Azure", "name": "ja-JP-Daichi:DragonHDOmniLatestNeural", "gender": "Male" } ]
/// }
/// </code>
/// </example>
public sealed class TextToSpeechSettings
{
   static readonly JsonSerializerOptions TheFilesJson = new(JsonSerializerDefaults.Web)
                                                        {
                                                           WriteIndented = true,
                                                           Converters = { new JsonStringEnumConverter() },
                                                           DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                                                        };

   /// <summary><c>%APPDATA%\JAStudio\text-to-speech.json</c>.</summary>
   public static string FileInTheUserProfile => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JAStudio", "text-to-speech.json");

   public AzureSpeechSettings? Azure { get; init; }

   public GoogleTextToSpeechSettings? Google { get; init; }

   /// <summary>The voices offered when generating TTS audio for a note, in priority order. The first is the default. See <see cref="ChosenVoice"/>.</summary>
   public IReadOnlyList<ChosenVoice> ChosenVoices { get; init; } = [];

   /// <summary>The settings in <see cref="FileInTheUserProfile"/>.</summary>
   public static TextToSpeechSettings ReadFromTheUserProfile() => ReadFrom(FileInTheUserProfile);

   /// <summary>The settings in the file at <paramref name="filePath"/>, or none, when there is no file there.</summary>
   public static TextToSpeechSettings ReadFrom(string filePath) =>
      File.Exists(filePath)
         ? JsonSerializer.Deserialize<TextToSpeechSettings>(File.ReadAllText(filePath), TheFilesJson) ?? throw new InvalidDataException($"{filePath} holds no settings.")
         : new TextToSpeechSettings();

   /// <summary>Writes these settings to <see cref="FileInTheUserProfile"/>, keys included, replacing what's there.</summary>
   public void WriteToTheUserProfile() => WriteTo(FileInTheUserProfile);

   /// <summary>Writes these settings to the file at <paramref name="filePath"/>, keys included, replacing what's there.</summary>
   public void WriteTo(string filePath)
   {
      Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? throw new ArgumentException($"{filePath} is in no folder.", nameof(filePath)));
      File.WriteAllText(filePath, JsonSerializer.Serialize(this, TheFilesJson));
   }

   /// <summary>These settings, with <paramref name="chosenVoices"/> as the <see cref="ChosenVoices"/>.</summary>
   public TextToSpeechSettings WithChosenVoices(IReadOnlyList<ChosenVoice> chosenVoices) => new() { Azure = Azure, Google = Google, ChosenVoices = chosenVoices };

   /// <summary>The <see cref="HttpClient"/> JAStudio calls the tts-providers through. One for the whole process, as <see cref="HttpClient"/> is meant to be used.</summary>
   static readonly HttpClient TheProvidersHttp = new();

   /// <summary>A tts-provider for each of the services these settings have a key for, calling it through the process's one client for them.</summary>
   public IReadOnlyList<ITextToSpeechProvider> ProvidersWithKeys() => ProvidersWithKeys(TheProvidersHttp);

   /// <summary>A tts-provider for each of the services these settings have a key for, calling it through <paramref name="http"/>.</summary>
   public IReadOnlyList<ITextToSpeechProvider> ProvidersWithKeys(HttpClient http) =>
   [
      ..Azure is {} azure ? [new AzureSpeechProvider(azure, http)] : Array.Empty<ITextToSpeechProvider>(),
      ..Google is {} google ? [new GoogleTextToSpeechProvider(google, http)] : Array.Empty<ITextToSpeechProvider>()
   ];

   /// <summary>The <see cref="ChosenVoices"/> as voices of <paramref name="providers"/>, in order, leaving out those whose tts-provider has no key.</summary>
   public IReadOnlyList<TextToSpeechVoice> ChosenVoicesOf(IReadOnlyList<ITextToSpeechProvider> providers) => [..ChosenVoices.Select(it => it.VoiceOf(providers)).OfType<TextToSpeechVoice>()];
}
