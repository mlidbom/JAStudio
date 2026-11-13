using System.Collections.Generic;
using System.IO;
using Compze.Threading;
using Newtonsoft.Json;

namespace JAStudio.Core.Configuration;

/// <summary>
/// Where JAStudio keeps its configuration: <c>configuration.json</c> in the user files folder (<see cref="IEnvironmentPaths.UserFilesDir"/>), each value<br/>
/// under its <see cref="ConfigurationValue{T}.Name"/>, written whenever a value changes. Each add-on folder, and so each source checkout, has its own.
/// </summary>
/// <remarks>
/// Anki kept the configuration until 2026-11-13, as the add-on's config in the add-on folder's <c>meta.json</c>, and JAStudio got it and saved it<br/>
/// through Python. JAStudio runs without Anki too now, so it keeps the configuration itself. The first time, it starts from what Anki kept<br/>
/// (<see cref="TheConfigurationAnkiKept"/>).
/// </remarks>
public class ConfigurationFile : IConfigDictSource
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly string _filePath;
   readonly string _ankisAddOnMetadataFilePath;

   public ConfigurationFile(IEnvironmentPaths paths)
   {
      _filePath = Path.Combine(paths.UserFilesDir, "configuration.json");
      _ankisAddOnMetadataFilePath = Path.Combine(paths.AddonRootDir, "meta.json");
   }

   /// <summary>The configuration saved. The first time, when nothing is saved yet, what Anki kept, which is saved at once.</summary>
   public Dictionary<string, object> Load() => _monitor.Locked(() =>
   {
      if(File.Exists(_filePath)) return ValuesIn(File.ReadAllText(_filePath));

      var configuration = TheConfigurationAnkiKept();
      Write(configuration);
      return configuration;
   });

   public void Persist(IReadOnlyDictionary<string, object> configuration) => _monitor.Locked(() => Write(configuration));

   void Write(IReadOnlyDictionary<string, object> configuration) => File.WriteAllText(_filePath, JsonConvert.SerializeObject(configuration, Formatting.Indented));

   /// <summary>
   /// The configuration Anki kept for JAStudio in the add-on folder's <c>meta.json</c>, its add-on metadata, under <c>config</c>, or nothing in an add-on<br/>
   /// folder Anki never kept any in, such as a new checkout's.
   /// </summary>
   Dictionary<string, object> TheConfigurationAnkiKept() =>
      File.Exists(_ankisAddOnMetadataFilePath)
         ? JsonConvert.DeserializeObject<AnkisAddOnMetadata>(File.ReadAllText(_ankisAddOnMetadataFilePath))?.Config ?? []
         : [];

   /// <summary>The values in <paramref name="json"/>, as Newtonsoft reads them into objects: whole numbers as <see cref="long"/>, the others as <see cref="double"/>.</summary>
   static Dictionary<string, object> ValuesIn(string json) =>
      JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? throw new InvalidDataException("The configuration file holds no configuration.");

   /// <summary>The part of Anki's metadata of an add-on, its <c>meta.json</c>, that holds the add-on's config.</summary>
   sealed class AnkisAddOnMetadata
   {
      [JsonProperty("config")] public Dictionary<string, object>? Config { get; init; }
   }
}
