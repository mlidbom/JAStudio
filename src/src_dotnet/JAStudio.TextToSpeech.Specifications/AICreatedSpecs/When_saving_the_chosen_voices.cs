using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Compze.Must;
using Compze.xUnitBDD;

// ReSharper disable InconsistentNaming

namespace JAStudio.TextToSpeech.Specifications.AICreatedSpecs;

/// <summary>
/// The chosen voices, in priority order, written into the settings file with the keys already there, and read back.<br/>
/// The file is in a folder of its own, which the specification deletes afterwards.
/// </summary>
public class When_saving_the_chosen_voices : IAIGeneratedSpec, IDisposable
{
   readonly string _folder = Directory.CreateTempSubdirectory("JAStudio_chosen_voices_").FullName;
   readonly TextToSpeechSettings _readBack;

   public When_saving_the_chosen_voices()
   {
      File.WriteAllText(SettingsFile, """{ "azure": { "key": "azure-key", "region": "swedencentral" }, "google": { "apiKey": "google-key" } }""");
      TextToSpeechSettings.ReadFrom(SettingsFile)
                          .WithChosenVoices([new ChosenVoice("Google", "ja-JP-Neural2-B", VoiceGender.Female), new ChosenVoice("Azure", "ja-JP-Daichi:DragonHDOmniLatestNeural", VoiceGender.Male)])
                          .WriteTo(SettingsFile);
      _readBack = TextToSpeechSettings.ReadFrom(SettingsFile);
   }

   string SettingsFile => Path.Combine(_folder, "text-to-speech.json");

   public void Dispose() => Directory.Delete(_folder, recursive: true);

   [XF] public void the_chosen_voices_are_read_back_in_their_order() =>
      _readBack.ChosenVoices.Select(it => $"{it.Provider} {it.Name} {it.Gender}").Must().SequenceEqual(["Google ja-JP-Neural2-B Female", "Azure ja-JP-Daichi:DragonHDOmniLatestNeural Male"]);
   [XF] public void the_keys_stay() => (_readBack.Azure?.Key, _readBack.Azure?.Region, _readBack.Google?.ApiKey).Must().Be(("azure-key", "swedencentral", "google-key"));
   [XF] public void the_chosen_voices_become_voices_of_the_providers_with_keys_in_their_order() =>
      _readBack.ChosenVoicesOf(_readBack.ProvidersWithKeys(new HttpClient())).Select(it => $"{it}").Must().SequenceEqual(["Google Neural2-B", "Azure Daichi:DragonHDOmniLatestNeural"]);

   public class when_one_providers_key_is_gone : When_saving_the_chosen_voices
   {
      [XF] public void its_chosen_voices_are_left_out() =>
         _readBack.ChosenVoicesOf([.._readBack.ProvidersWithKeys(new HttpClient()).Where(it => it.Name == "Azure")]).Select(it => $"{it}").Must().SequenceEqual(["Azure Daichi:DragonHDOmniLatestNeural"]);
   }
}
