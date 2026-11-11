using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A vocab's <see cref="VocabNoteMatchingConfiguration"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class VocabMatchingDto
{
   /// <summary>
   /// The vocab's <see cref="VocabNoteMatchingConfiguration.RequiresForbids"/> rules that are set, each under its <see cref="RequireForbidFlagField.Name"/>.<br/>
   /// A rule that's unset isn't stored.
   /// </summary>
   public Dictionary<string, RequiredOrForbiddenDto> RequiredOrForbidden { get; init; } = new();

   /// <summary>The names of the vocab's <see cref="VocabNoteMatchingConfiguration.Flags"/> that are set.</summary>
   public List<string> Flags { get; init; } = [];

   /// <summary>The vocab's <see cref="VocabNoteMatchingConfiguration.MatchCondition"/>, as <see cref="MatchConditionJson.AsAVocabStoresIt"/> writes it: none for a vocab without one.</summary>
   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
   [JsonObjectAsJsonText]
   public JsonObject? Condition { get; init; }

   /// <summary>The surfaces of the vocab's <see cref="VocabNoteMatchingConfiguration.YieldToSurface"/> rule.</summary>
   public List<string> YieldToSurface { get; init; } = [];
}
