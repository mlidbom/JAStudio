using System.Collections.Generic;

namespace JAStudio.Core.Configuration;

public class ConfigurationStore
{
   readonly IReadingsMappingsSource _mappingsSource;
   readonly IConfigDictSource _configSource;
   readonly Dictionary<string, object> _configDict;

   internal ConfigurationStore(IReadingsMappingsSource mappingsSource, IConfigDictSource configSource)
   {
      _mappingsSource = mappingsSource;
      _configSource = configSource;
      _configDict = configSource.Load();
   }

   internal Dictionary<string, object> GetConfigDict() => _configDict;

   internal void WriteConfigDict() => _configSource.Persist(_configDict);

   JapaneseConfig? _config;

   public JapaneseConfig Config()
   {
      return _config ??= new JapaneseConfig(this);
   }

   // --- Readings mappings ---

   public Dictionary<string, string> GetReadingsMappings() => _mappingsSource.GetMappings();

   public void SaveMappings(string mappings) => _mappingsSource.SaveMappings(mappings);

   public string ReadReadingsMappingsFile() => _mappingsSource.ReadRawMappings();
}
