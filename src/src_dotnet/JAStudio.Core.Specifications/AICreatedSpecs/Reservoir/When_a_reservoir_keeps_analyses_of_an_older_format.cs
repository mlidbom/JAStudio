using System;
using System.IO;
using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Reservoir;
using Microsoft.Data.Sqlite;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Reservoir;

/// <summary>
/// A reservoir file whose analyses are of an older format than the one <see cref="ReservoirFile"/> reads now, as one made before each word kept<br/>
/// whether it's a valid match: its analyses are never read as if they were of the current format, and are forgotten to be made anew.
/// </summary>
public class When_a_reservoir_keeps_analyses_of_an_older_format : IAIGeneratedSpec, IDisposable
{
   readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-reservoir-format-spec-");
   readonly ReservoirFile _reservoir;

   public When_a_reservoir_keeps_analyses_of_an_older_format()
   {
      var file = new FileInfo(Path.Combine(_folder.FullName, "reservoir.sqlite"));
      ReservoirFile.OpenOrCreate(file).Dispose();
      MarkItsAnalysesAsOfTheFirstFormat(file);
      _reservoir = ReservoirFile.OpenOrCreate(file);
   }

   [XF] public void it_is_known_to_keep_them() => _reservoir.KeepsAnalysesOfAnOlderFormat.Must().BeTrue();

   [XF] public void reading_its_analyses_fails() => Invoking(() => _reservoir.AnalyzedSentences().ToList()).Must().Throw<StateAssertionFailedException>();

   [XF] public void once_they_are_forgotten_it_keeps_none_of_an_older_format()
   {
      _reservoir.ForgetTheAnalysesOfAnOlderFormat();
      _reservoir.KeepsAnalysesOfAnOlderFormat.Must().BeFalse();
   }

   public void Dispose()
   {
      _reservoir.Dispose();
      SqliteConnection.ClearAllPools();
      _folder.Delete(recursive: true);
   }

   /// <summary>
   /// Marks the analyses the reservoir file <paramref name="file"/> keeps as of the first format, 0, as a reservoir's were before each word kept<br/>
   /// whether it's a valid match: the format is the file's <c>user_version</c>.
   /// </summary>
   static void MarkItsAnalysesAsOfTheFirstFormat(FileInfo file)
   {
      using var connection = new SqliteConnection($"Data Source={file.FullName};Pooling=False");
      connection.Open();
      using var command = connection.CreateCommand();
      command.CommandText = "pragma user_version = 0";
      command.ExecuteNonQuery();
   }

   public class a_reservoir_made_now : IAIGeneratedSpec, IDisposable
   {
      readonly DirectoryInfo _folder = Directory.CreateTempSubdirectory("jastudio-reservoir-format-spec-");
      readonly ReservoirFile _reservoir;

      public a_reservoir_made_now() => _reservoir = ReservoirFile.OpenOrCreate(new FileInfo(Path.Combine(_folder.FullName, "reservoir.sqlite")));

      [XF] public void keeps_none_of_an_older_format() => _reservoir.KeepsAnalysesOfAnOlderFormat.Must().BeFalse();

      public void Dispose()
      {
         _reservoir.Dispose();
         SqliteConnection.ClearAllPools();
         _folder.Delete(recursive: true);
      }
   }
}
