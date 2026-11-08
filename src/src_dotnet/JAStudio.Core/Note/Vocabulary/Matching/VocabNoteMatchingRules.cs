using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's configurable matching rules: the texts that must, or must not, surround or make up a word for it to match the vocab,<br/>
/// such as a prefix the word must not have.
/// </summary>
/// <remarks>Changing a rule's set saves nothing by itself: <see cref="Save"/> saves the note.</remarks>
public class VocabNoteMatchingRules
{
   readonly NoteGuard _guard;

   internal VocabNoteMatchingRules(VocabMatchingRulesDto? data, NoteGuard guard)
   {
      _guard = guard;
      SurfaceIsNot = data?.SurfaceIsNot.ToHashSet() ?? [];
      YieldToSurface = data?.YieldToSurface.ToHashSet() ?? [];
      PrefixIsNot = data?.PrefixIsNot.ToHashSet() ?? [];
      SuffixIsNot = data?.SuffixIsNot.ToHashSet() ?? [];
      RequiredPrefix = data?.RequiredPrefix.ToHashSet() ?? [];
   }

   public HashSet<string> SurfaceIsNot { get; }
   public HashSet<string> YieldToSurface { get; }
   public HashSet<string> PrefixIsNot { get; }
   public HashSet<string> SuffixIsNot { get; }
   public HashSet<string> RequiredPrefix { get; }

   public void Save() => _guard.Update(() => {});

   public int MatchWeight
   {
      get
      {
         var weight = 0;
         if(RequiredPrefix.Count > 0)
            weight += 10;

         if(SurfaceIsNot.Count > 0) weight += 2;
         if(YieldToSurface.Count > 0) weight += 2;
         if(PrefixIsNot.Count > 0) weight += 2;
         if(SuffixIsNot.Count > 0) weight += 2;

         return weight;
      }
   }

   internal void OverwriteWith(VocabNoteMatchingRules other)
   {
      Replace(SurfaceIsNot, other.SurfaceIsNot);
      Replace(YieldToSurface, other.YieldToSurface);
      Replace(PrefixIsNot, other.PrefixIsNot);
      Replace(SuffixIsNot, other.SuffixIsNot);
      Replace(RequiredPrefix, other.RequiredPrefix);
      Save();
   }

   /// <summary>The rules as the corpus stores them, each set in ordinal order so that the same rules always write the same file.</summary>
   internal VocabMatchingRulesDto ToDto() => new()
   {
      PrefixIsNot = InOrdinalOrder(PrefixIsNot),
      SuffixIsNot = InOrdinalOrder(SuffixIsNot),
      SurfaceIsNot = InOrdinalOrder(SurfaceIsNot),
      YieldToSurface = InOrdinalOrder(YieldToSurface),
      RequiredPrefix = InOrdinalOrder(RequiredPrefix)
   };

   static void Replace(HashSet<string> rule, HashSet<string> with)
   {
      rule.Clear();
      rule.UnionWith(with);
   }

   static List<string> InOrdinalOrder(HashSet<string> rule) => [.. rule.Order(StringComparer.Ordinal)];
}
