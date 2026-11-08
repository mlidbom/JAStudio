using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Compze.Contracts;
using Compze.Threading;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Importing;

/// <summary>
/// The <see cref="ImportProfile"/>s, saved in <c>import-profiles.json</c> in the corpus repository: they name the corpus's sources and media folders,<br/>
/// so they're versioned with it.
/// </summary>
/// <remarks>
/// They're loaded once, and every window works on the same <see cref="ImportProfile"/> objects: an import run window keeps track of its profile<br/>
/// while the import profiles window edits it, through <see cref="ProfileChanged"/>.<br/>
/// The import profiles window adds and deletes them on the UI thread while <see cref="AutomaticImportRuns"/> reads them on the threads Anki signals on.
/// </remarks>
public class SavedImportProfiles
{
   static readonly JsonSerializerOptions JsonOptions = new()
                                                       {
                                                          WriteIndented = true,
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                          Encoder = FullUnicodeJsonEncoder.Instance
                                                       };

   readonly string _filePath;
   readonly List<ImportProfile> _profiles;
   readonly IMonitor _monitor = IMonitor.New();

   public SavedImportProfiles(IEnvironmentPaths paths)
   {
      _filePath = Path.Combine(paths.DatabaseDir, "import-profiles.json");
      _profiles = Load();
   }

   /// <summary>The profiles as they are now: a list that later additions and deletions don't change.</summary>
   public IReadOnlyList<ImportProfile> All => _monitor.Locked(() => _profiles.ToList());

   /// <summary>Raised after a profile was changed or deleted, and the profiles saved.</summary>
   public event Action<ImportProfile>? ProfileChanged;

   public bool Contains(ImportProfile profile) => _monitor.Locked(() => _profiles.Contains(profile));

   /// <summary>Adds a new, empty profile and saves the profiles.</summary>
   public ImportProfile AddNew()
   {
      var profile = new ImportProfile { Name = "New import profile" };
      Add(profile);
      return profile;
   }

   /// <summary>Adds <paramref name="profile"/> and saves the profiles.</summary>
   public void Add(ImportProfile profile)
   {
      Contract.Argument.Assert(!Contains(profile), () => $"The import profile {profile.Name} is saved already.");
      _monitor.Locked(() =>
      {
         _profiles.Add(profile);
         Save();
      });
   }

   public void Delete(ImportProfile profile)
   {
      Contract.Argument.Assert(Contains(profile), () => $"The import profile {profile.Name} isn't saved.");
      _monitor.Locked(() =>
      {
         _profiles.Remove(profile);
         Save();
      });
      ProfileChanged?.Invoke(profile);
   }

   /// <summary>Saves the profiles after <paramref name="profile"/> was changed.</summary>
   public void SaveTheChangeOf(ImportProfile profile)
   {
      Contract.Argument.Assert(Contains(profile), () => $"The import profile {profile.Name} isn't saved.");
      _monitor.Locked(Save);
      ProfileChanged?.Invoke(profile);
   }

   List<ImportProfile> Load() =>
      File.Exists(_filePath)
         ? JsonSerializer.Deserialize<List<ImportProfile>>(File.ReadAllText(_filePath), JsonOptions) ?? throw new InvalidDataException($"{_filePath} holds no list of import profiles.")
         : [];

   void Save() => File.WriteAllText(_filePath, JsonSerializer.Serialize(_profiles, JsonOptions));
}
