using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Storage.Media;

public class MediaImportRulePersistence
{
   static readonly JsonSerializerOptions JsonOptions = new()
                                                       {
                                                          WriteIndented = true,
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                          DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                                                          Encoder = FullUnicodeJsonEncoder.Instance,
                                                          Converters = { new SourceTagJsonConverter() }
                                                       };

   static string FilePath(IEnvironmentPaths paths) => Path.Combine(paths.MetadataDir, "media-import-rules.json");

   public static PersistedImportRules Load(IEnvironmentPaths paths)
   {
      var path = FilePath(paths);
      if(!File.Exists(path)) return new PersistedImportRules();
      var json = File.ReadAllText(path);
      return JsonSerializer.Deserialize<PersistedImportRules>(json, JsonOptions) ?? new PersistedImportRules();
   }

   public static void Save(PersistedImportRules rules, IEnvironmentPaths paths)
   {
      var path = FilePath(paths);
      Directory.CreateDirectory(Path.GetDirectoryName(path)!);
      var json = JsonSerializer.Serialize(rules, JsonOptions);
      File.WriteAllText(path, json);
   }
}

public class PersistedImportRules
{
   public List<ImportRule> VocabRules { get; set; } = [];
   public List<ImportRule> SentenceRules { get; set; } = [];
}
