using System;
using System.Collections.Generic;
using System.Text.Json;
using JetBrains.Annotations;

namespace JAStudio.Anki.CollectionDatabase;

/// <summary>
/// The SQLite database of an Anki collection, as JAStudio reads it. Each read is a SELECT returning one value: a JSON array that SQLite builds of<br/>
/// one JSON object per row (<c>json_group_array(json_object(…))</c>), read into a row type whose properties have the objects' keys as names.<br/>
/// Inside Anki it's read through Anki's own connection (<see cref="AnkisOwnConnection"/>); in the headless JAStudio, from the collection's file<br/>
/// (<see cref="AnkiCollectionFile"/>). The SELECTs are the same for both.
/// </summary>
/// <remarks>
/// One JSON text per read, rather than one row after another, because inside Anki the rows come from Python, and converting each value from<br/>
/// Python to .NET costs far more than converting one string: 20 to 185 ms a read rather than 4 to 15, measured on Magnus's collection on<br/>
/// 2026-11-10. And because SQLite builds the JSON, it holds whatever text a field holds, tabs and newlines included.
/// </remarks>
public abstract class AnkiCollectionDatabase
{
   /// <summary>
   /// How the JSON SQLite builds is read into rows: a key is the camel-cased name of a row's property, such as <c>noteTypeId</c> for<br/>
   /// <c>NoteTypeId</c>, and a row the SELECT leaves a value out of, or gives a null where the row type has no nullable property, fails the read.
   /// </summary>
   static readonly JsonSerializerOptions RowsAsSqliteBuildsThem = new()
   {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
      RespectNullableAnnotations = true,
      RespectRequiredConstructorParameters = true
   };

   /// <summary>The rows <paramref name="select"/> returns: a SELECT of one <c>json_group_array(json_object(…))</c>, with a key for each of <typeparamref name="TRow"/>'s properties.</summary>
   /// <typeparam name="TRow">The type of a row, which the read creates, through its constructor, as System.Text.Json does.</typeparam>
   public IReadOnlyList<TRow> Read<[MeansImplicitUse(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature, ImplicitUseTargetFlags.WithMembers)] TRow>(string select) =>
      JsonSerializer.Deserialize<List<TRow>>(TheJsonTextOf(select), RowsAsSqliteBuildsThem) ?? throw new InvalidOperationException($"The SELECT returned null rather than a JSON array: {select}");

   /// <summary>A list for an SQL <c>in</c> condition, such as <c>(1, 2, 3)</c>, of <paramref name="ids"/>, such as note ids.</summary>
   public static string SqlListOf(IEnumerable<long> ids) => $"({string.Join(", ", ids)})";

   /// <summary>The one value <paramref name="select"/> returns: a JSON text.</summary>
   protected abstract string TheJsonTextOf(string select);
}
