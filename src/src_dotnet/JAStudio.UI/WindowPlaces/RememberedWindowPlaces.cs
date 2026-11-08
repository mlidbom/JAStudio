using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using JAStudio.Core;

namespace JAStudio.UI.WindowPlaces;

/// <summary>
/// The <see cref="WindowPlace"/> each of JAStudio's windows was in when it was last left, by the window's name, so that it opens there again.<br/>
/// They're kept in <c>window-places.json</c> in the user files folder: places depend on the machine's screens, so they aren't versioned.
/// </summary>
partial class RememberedWindowPlaces(IEnvironmentPaths paths)
{
   static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

   readonly string _filePath = Path.Combine(paths.UserFilesDir, "window-places.json");

   /// <summary>Where the window named <paramref name="windowName"/> was last, or null if it hasn't been opened before.</summary>
   public WindowPlace? Of(string windowName) => SavedPlaces().GetValueOrDefault(windowName)?.ToWindowPlace();

   public void Remember(string windowName, WindowPlace place)
   {
      var savedPlaces = SavedPlaces();
      savedPlaces[windowName] = SavedWindowPlace.Of(place);
      File.WriteAllText(_filePath, JsonSerializer.Serialize(savedPlaces, JsonOptions));
   }

   Dictionary<string, SavedWindowPlace> SavedPlaces() =>
      File.Exists(_filePath)
         ? JsonSerializer.Deserialize<Dictionary<string, SavedWindowPlace>>(File.ReadAllText(_filePath), JsonOptions) ?? throw new InvalidDataException($"{_filePath} holds no window places.")
         : [];
}
