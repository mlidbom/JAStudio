using System.Collections.Generic;
using System.IO;

namespace JAStudio.Core.Configuration;

/// <summary>
/// The readings mappings, kept in the file <c>readings_mappings.txt</c> among the user files (<see cref="IEnvironmentPaths.UserFilesDir"/>).
/// </summary>
/// <remarks>
/// The file is part of the install, so a missing one is a broken install, and JAStudio fails as it starts (Magnus, 2026-11-15: "Mappings are<br/>
/// part of the install. If they are missing the install is broken.").
/// </remarks>
public class FileReadingsMappingsSource : IReadingsMappingsSource
{
   readonly string _filePath;

   public FileReadingsMappingsSource(IEnvironmentPaths paths) =>
      _filePath = Path.Combine(paths.UserFilesDir, "readings_mappings.txt");

   public Dictionary<string, string> GetMappings() => ReadingsMappingsParser.Parse(ReadRawMappings());
   public string ReadRawMappings() => File.ReadAllText(_filePath);

   public void SaveMappings(string mappings)
   {
      File.WriteAllText(_filePath, mappings);
   }
}
