using System;
using System.Text.Json.Serialization;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="BreakdownWord"/> as the corpus stores it: with its vocab by its id alone, and its variant as a <see cref="ParsedMatchDto"/> has it.</summary>
[MemoryPackable]
public partial class BreakdownWordDto
{
   public string Form { get; init; } = string.Empty;
   public int StartIndex { get; init; }

   /// <summary><see cref="ParsedMatch.SurfaceVariant"/> or <see cref="ParsedMatch.BaseVariant"/>.</summary>
   public string Variant { get; init; } = string.Empty;

   /// <summary>The <see cref="BreakdownWord.ShownAs"/>. Not stored for a word the breakdown shows no other form beside.</summary>
   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
   public string? ShownAs { get; init; }

   public bool IsEmergencyDisplayed { get; init; }
   public Guid? VocabId { get; init; }

   public static BreakdownWordDto From(BreakdownWord word) =>
      new()
      {
         Form = word.Form,
         StartIndex = word.StartIndex,
         Variant = word.IsMatchedByItsSurface ? ParsedMatch.SurfaceVariant : ParsedMatch.BaseVariant,
         ShownAs = word.ShownAs,
         IsEmergencyDisplayed = word.IsEmergencyDisplayed,
         VocabId = word.VocabId != null ? Guid.Parse(word.VocabId) : null
      };

   public BreakdownWord ToBreakdownWord() =>
      new(Form,
          StartIndex,
          Variant switch
          {
             ParsedMatch.SurfaceVariant => BreakdownWord.SurfaceVariant,
             ParsedMatch.BaseVariant => BreakdownWord.BaseVariant,
             _ => throw new InvalidOperationException($"The breakdown word {Form} at {StartIndex} has the variant \"{Variant}\": it is \"{ParsedMatch.SurfaceVariant}\" or \"{ParsedMatch.BaseVariant}\".")
          },
          ShownAs,
          IsEmergencyDisplayed,
          Vocab: null,
          VocabId?.ToString());
}
