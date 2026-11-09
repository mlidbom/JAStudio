using System;
using System.Collections.Generic;
using System.IO;
using JAStudio.Core.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JAStudio.HeadlessHost;

/// <summary>
/// JAStudio's configuration as Anki keeps it for the add-on: the <c>config</c> object in the add-on folder's <c>meta.json</c>, read as Anki's<br/>
/// <c>getConfig</c> gives it to JAStudio inside Anki. The headless JAStudio only reads it: the file is Anki's, and Anki may be running.
/// </summary>
class AddonConfigFile : IConfigDictSource
{
   readonly string _metaJsonPath;

   public AddonConfigFile(DirectoryInfo addonFolder) => _metaJsonPath = Path.Combine(addonFolder.FullName, "meta.json");

   /// <summary>The configuration, or none, as Anki has none before the add-on's configuration is first changed, and JAStudio then uses its defaults.</summary>
   public Dictionary<string, object> Load()
   {
      if(!File.Exists(_metaJsonPath)) return [];

      var config = JObject.Parse(File.ReadAllText(_metaJsonPath))["config"];
      return config == null ? [] : JsonConvert.DeserializeObject<Dictionary<string, object>>(config.ToString()) ?? [];
   }

   public void Persist(string json) =>
      throw new InvalidOperationException("The headless JAStudio doesn't change JAStudio's configuration: Anki keeps it, and Anki may be running.");
}
