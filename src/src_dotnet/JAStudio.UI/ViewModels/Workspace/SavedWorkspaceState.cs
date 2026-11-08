using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JAStudio.Core;

namespace JAStudio.UI.ViewModels.Workspace;

/// <summary>
/// The <see cref="WorkspaceState"/> the workspace was last left in, kept in <c>workspace-state.json</c> in the user files folder.<br/>
/// It's per machine, like where the windows were, so it isn't versioned.
/// </summary>
class SavedWorkspaceState(IEnvironmentPaths paths)
{
   static readonly JsonSerializerOptions JsonOptions = new()
                                                       {
                                                          WriteIndented = true,
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                          Converters = { new JsonStringEnumConverter() }
                                                       };

   readonly string _filePath = Path.Combine(paths.UserFilesDir, "workspace-state.json");

   /// <summary>The state the workspace was last left in, or null if it was never saved.</summary>
   public WorkspaceState? Load() =>
      File.Exists(_filePath)
         ? JsonSerializer.Deserialize<WorkspaceState>(File.ReadAllText(_filePath), JsonOptions) ?? throw new InvalidDataException($"{_filePath} holds no workspace state.")
         : null;

   public void Save(WorkspaceState state) => File.WriteAllText(_filePath, JsonSerializer.Serialize(state, JsonOptions));
}
