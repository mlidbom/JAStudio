using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;

/// <summary>
/// The folder <c>shared-conditions</c> in the corpus checkout, which keeps the shared-conditions (<see cref="SharedConditions"/>), one JSON file each, named<br/>
/// by its full name, such as <c>Word.FollowsAPastTenseStem.json</c>. The corpus repository versions them with the notes that use them.
/// </summary>
/// <remarks>What a file holds, and in what JSON, is <see cref="StoredSharedCondition"/>'s.</remarks>
sealed class SharedConditionsFolder
{
   static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true, Encoder = FullUnicodeJsonEncoder.Instance };

   readonly string _path;

   internal SharedConditionsFolder(IEnvironmentPaths paths) => _path = Path.Combine(paths.DatabaseDir, "shared-conditions");

   /// <summary>The name of the file that keeps <paramref name="sharedCondition"/>: its full name, such as <c>Word.FollowsAPastTenseStem.json</c>.</summary>
   internal static string FileNameOf(ISharedCondition sharedCondition) => $"{sharedCondition.FullName}.json";

   /// <summary>The shared-conditions as every file in the folder keeps one, in the order of their file names: none when there's no folder yet.</summary>
   /// <exception cref="FormatException">When a file isn't a shared-condition's.</exception>
   internal IReadOnlyList<StoredSharedCondition> ReadEveryFile() =>
      Directory.Exists(_path) ? [.. Directory.EnumerateFiles(_path, "*.json").Order(StringComparer.Ordinal).Select(Read)] : [];

   /// <summary>Saves <paramref name="sharedCondition"/> in its file, making the folder when there's none yet.</summary>
   internal void Save(ISharedCondition sharedCondition)
   {
      Directory.CreateDirectory(_path);
      var stored = StoredSharedCondition.Of(sharedCondition);
      File.WriteAllText(Path.Combine(_path, stored.FileName), stored.ToJson().ToJsonString(IndentedJson));
   }

   static StoredSharedCondition Read(string path) => StoredSharedCondition.From(Path.GetFileName(path), JsonNode.Parse(File.ReadAllText(path)));
}
