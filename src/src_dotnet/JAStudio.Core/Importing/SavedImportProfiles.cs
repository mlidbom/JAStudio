using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Importing;

/// <summary>
/// The <see cref="ImportProfile"/>s, saved in <c>import-profiles.json</c> in the corpus repository: they name the corpus's sources and media folders,<br/>
/// so they're versioned with it.
/// </summary>
public class SavedImportProfiles(IEnvironmentPaths paths)
{
   static readonly JsonSerializerOptions JsonOptions = new()
                                                       {
                                                          WriteIndented = true,
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                          Encoder = FullUnicodeJsonEncoder.Instance
                                                       };

   readonly string _filePath = Path.Combine(paths.DatabaseDir, "import-profiles.json");

   /// <summary>The saved import profiles, or none before the first is saved.</summary>
   public List<ImportProfile> Load() =>
      File.Exists(_filePath)
         ? JsonSerializer.Deserialize<List<ImportProfile>>(File.ReadAllText(_filePath), JsonOptions) ?? throw new InvalidDataException($"{_filePath} holds no list of import profiles.")
         : [];

   public void Save(IReadOnlyList<ImportProfile> profiles) => File.WriteAllText(_filePath, JsonSerializer.Serialize(profiles, JsonOptions));
}
