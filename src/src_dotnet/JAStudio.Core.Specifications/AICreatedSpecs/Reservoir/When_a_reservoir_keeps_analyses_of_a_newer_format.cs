using System;
using System.IO;
using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir;
using JAStudio.Core.Reservoir.Analysis.Numbering;
using Microsoft.Data.Sqlite;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A reservoir file whose analyses are of a newer format than the one <see cref="ReservoirFile"/> reads, as when a checkout of an older commit<br/>
/// opens a reservoir that newer code analyzed: its analyses are neither read nor written to, nor forgotten as if they were of an older format.
/// </summary>
public class When_a_reservoir_keeps_analyses_of_a_newer_format : IAIGeneratedSpec, IDisposable
{
   const int AFormatNewerThanAnyYet = 1000;

   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-reservoir-format-spec-");
   readonly ReservoirFile _reservoir;

   public When_a_reservoir_keeps_analyses_of_a_newer_format()
   {
      var file = new FileInfo(Path.Combine(_folder.FullName, "reservoir.sqlite"));
      ReservoirFile.OpenOrCreate(file).Dispose();
      MarkItsAnalysesAsOfANewerFormat(file);
      _reservoir = ReservoirFile.OpenOrCreate(file);
   }

   [XF] public void it_is_not_taken_to_keep_analyses_of_an_older_format() => _reservoir.KeepsAnalysesOfAnOlderFormat.Must().BeFalse();

   [XF] public void reading_its_analyses_fails() => Invoking(() => _reservoir.AnalyzedSentences().ToList()).Must().Throw<StateAssertionFailedException>();

   [XF] public void saving_analyses_to_it_fails() => Invoking(() => _reservoir.SaveAnalyses([], new ReservoirAnalysisNumbers([], []))).Must().Throw<StateAssertionFailedException>();

   public void Dispose()
   {
      _reservoir.Dispose();
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   /// <summary>Marks the analyses the reservoir file <paramref name="file"/> keeps as of a newer format than any yet: the format is the file's <c>user_version</c>.</summary>
   static void MarkItsAnalysesAsOfANewerFormat(FileInfo file)
   {
      using var connection = new SqliteConnection($"Data Source={file.FullName};Pooling=False");
      connection.Open();
      using var command = connection.CreateCommand();
      command.CommandText = $"pragma user_version = {AFormatNewerThanAnyYet}";
      command.ExecuteNonQuery();
   }
}
