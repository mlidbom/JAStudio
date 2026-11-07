using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.TextToSpeech.Azure;
using JAStudio.TextToSpeech.Google;

// ReSharper disable InconsistentNaming

namespace JAStudio.TextToSpeech.Specifications.AICreatedSpecs;

/// <summary>The settings file with the tts-providers' keys, read from a folder of its own, which the specification deletes afterwards.</summary>
public class When_reading_the_text_to_speech_settings : IAIGeneratedSpec, IDisposable
{
   readonly string _folder = Directory.CreateTempSubdirectory("JAStudio_text_to_speech_settings_").FullName;

   string SettingsFile => Path.Combine(_folder, "text-to-speech.json");

   string[] NamesOfTheProvidersWithKeys() => [..TextToSpeechSettings.ReadFrom(SettingsFile).ProvidersWithKeys(new HttpClient()).Select(it => it.Name)];

   public void Dispose() => Directory.Delete(_folder, recursive: true);

   public class from_a_file_with_keys_for_both_providers : When_reading_the_text_to_speech_settings
   {
      public from_a_file_with_keys_for_both_providers() =>
         File.WriteAllText(SettingsFile, """{ "azure": { "key": "azure-key", "region": "swedencentral" }, "google": { "apiKey": "google-key" } }""");

      [XF] public void both_providers_are_offered() => NamesOfTheProvidersWithKeys().Must().SequenceEqual(["Azure", "Google"]);
      [XF] public void the_azure_region_is_read() =>
         (TextToSpeechSettings.ReadFrom(SettingsFile).Azure ?? throw new InvalidOperationException("No Azure settings were read.")).Region.Must().Be("swedencentral");
   }

   public class from_a_file_with_a_key_for_google_only : When_reading_the_text_to_speech_settings
   {
      public from_a_file_with_a_key_for_google_only() => File.WriteAllText(SettingsFile, """{ "google": { "apiKey": "google-key" } }""");

      [XF] public void only_google_is_offered() => NamesOfTheProvidersWithKeys().Must().SequenceEqual(["Google"]);
   }

   public class when_there_is_no_file : When_reading_the_text_to_speech_settings
   {
      [XF] public void no_provider_is_offered() => NamesOfTheProvidersWithKeys().Must().BeEmpty();
   }

   [XF] public void the_azure_settings_do_not_print_their_key() => $"{new AzureSpeechSettings { Key = "secret", Region = "swedencentral" }}".Must().NotContain("secret");
   [XF] public void the_google_settings_do_not_print_their_key() => $"{new GoogleTextToSpeechSettings { ApiKey = "secret" }}".Must().NotContain("secret");
}
