using System;
using System.Collections.Generic;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction;

public sealed class WordExclusion
{
   const int NoIndex = -1;

   public string Word { get; }
   public int Index { get; }

   WordExclusion(string word, int index)
   {
      Word = word;
      Index = index;
   }

   /// <summary>Whether the exclusion is of its <see cref="Word"/> everywhere in the sentence (<see cref="Global"/>), rather than at one position (<see cref="AtIndex"/>).</summary>
   public bool IsGlobal => Index == NoIndex;

   public bool ExcludesFormAtIndex(string form, int index) => form == Word && (IsGlobal || Index == index);

   public static WordExclusion Global(string exclusion) => new(exclusion.Trim(), NoIndex);

   public static WordExclusion AtIndex(string exclusion, int index) => new(exclusion.Trim(), index);

   /// <summary>
   /// Creates a WordExclusion from a string in the format "word:index" or "word" for global exclusions.
   /// </summary>
   public static WordExclusion FromString(string value)
   {
      var parts = value.Split(':');
      if(parts.Length == 1)
      {
         return Global(parts[0]);
      } else if(parts.Length == 2)
      {
         return AtIndex(parts[0], int.Parse(parts[1]));
      }

      throw new ArgumentException($"Invalid exclusion format: {value}");
   }

   public override bool Equals(object? obj)
   {
      if(obj is WordExclusion other)
      {
         return Word == other.Word && Index == other.Index;
      }

      return false;
   }

   public override int GetHashCode() => HashCode.Combine(Word, Index);

   public override string ToString() => $"WordExclusion('{Word}', {Index})";

   public bool ExcludesAllWordsExcludedBy(WordExclusion other) => Word == other.Word && (IsGlobal || Index == other.Index);

   public Dictionary<string, object> ToDict() =>
      new()
      {
         { "word", Word },
         { "index", Index }
      };

   public static WordExclusion FromReader(JsonReader reader) =>
      new(
         reader.GetString("word"),
         reader.GetInt("index")
      );
}
