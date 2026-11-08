using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteMatchingRulesData
{
   public HashSet<string> PrefixIsNot { get; set; } = [];
   public HashSet<string> SuffixIsNot { get; set; } = [];
   public HashSet<string> SurfaceIsNot { get; set; } = [];
   public HashSet<string> YieldToSurface { get; set; } = [];
   public HashSet<string> RequiredPrefix { get; set; } = [];

   public VocabNoteMatchingRulesData() {}

   public VocabNoteMatchingRulesData(HashSet<string> surfaceIsNot,
                                     HashSet<string> prefixIsNot,
                                     HashSet<string> suffixIsNot,
                                     HashSet<string> requiredPrefix,
                                     HashSet<string> yieldToSurface)
   {
      SurfaceIsNot = surfaceIsNot;
      PrefixIsNot = prefixIsNot;
      SuffixIsNot = suffixIsNot;
      RequiredPrefix = requiredPrefix;
      YieldToSurface = yieldToSurface;
   }
}

public class VocabNoteMatchingRulesSerializer : IObjectSerializer<VocabNoteMatchingRulesData>
{
   static string _emptyObjectJson = string.Empty;

   public VocabNoteMatchingRulesSerializer()
   {
      if(string.IsNullOrEmpty(_emptyObjectJson))
      {
         _emptyObjectJson = Serialize(Deserialize(string.Empty));
      }
   }

   public VocabNoteMatchingRulesData Deserialize(string serialized)
   {
      if(string.IsNullOrEmpty(serialized))
      {
         return new VocabNoteMatchingRulesData();
      }

      using var doc = JsonDocument.Parse(serialized);
      var reader = new JsonReader(doc.RootElement);

      return new VocabNoteMatchingRulesData(
         reader.GetStringSet("surface_is_not", []),
         reader.GetStringSet("prefix_is_not", []),
         reader.GetStringSet("suffix_is_not", []),
         reader.GetStringSet("required_prefix", []),
         reader.GetStringSet("yield_to_surface", [])
      );
   }

   public string Serialize(VocabNoteMatchingRulesData instance)
   {
      var jsonDict = new Dictionary<string, object>();

      if(instance.SurfaceIsNot.Any())
         jsonDict["surface_is_not"] = instance.SurfaceIsNot.OrderBy(s => s).ToList();

      if(instance.PrefixIsNot.Any())
         jsonDict["prefix_is_not"] = instance.PrefixIsNot.OrderBy(s => s).ToList();

      if(instance.SuffixIsNot.Any())
         jsonDict["suffix_is_not"] = instance.SuffixIsNot.OrderBy(s => s).ToList();

      if(instance.RequiredPrefix.Any())
         jsonDict["required_prefix"] = instance.RequiredPrefix.OrderBy(s => s).ToList();

      if(instance.YieldToSurface.Any())
         jsonDict["yield_to_surface"] = instance.YieldToSurface.OrderBy(s => s).ToList();

      var json = JsonHelper.DictToJson(jsonDict);
      return json != _emptyObjectJson ? json : string.Empty;
   }
}

public class VocabNoteMatchingRules
{
   readonly NoteGuard _guard;
   readonly VocabNoteMatchingRulesData _data;

   public HashSet<string> SurfaceIsNot => _data.SurfaceIsNot;
   public HashSet<string> YieldToSurface => _data.YieldToSurface;
   public HashSet<string> PrefixIsNot => _data.PrefixIsNot;
   public HashSet<string> SuffixIsNot => _data.SuffixIsNot;
   public HashSet<string> RequiredPrefix => _data.RequiredPrefix;

   internal VocabNoteMatchingRules(VocabMatchingRulesDto? subData, NoteGuard guard)
   {
      _guard = guard;
      _data = subData != null
                 ? new VocabNoteMatchingRulesData(
                    subData.SurfaceIsNot.ToHashSet(),
                    subData.PrefixIsNot.ToHashSet(),
                    subData.SuffixIsNot.ToHashSet(),
                    subData.RequiredPrefix.ToHashSet(),
                    subData.YieldToSurface.ToHashSet())
                 : new VocabNoteMatchingRulesData();
   }

   public void Save() => _guard.Update(() => {});

   public int MatchWeight
   {
      get
      {
         var weight = 0;
         if(RequiredPrefix.Any())
            weight += 10;

         if(SurfaceIsNot.Any()) weight += 2;
         if(YieldToSurface.Any()) weight += 2;
         if(PrefixIsNot.Any()) weight += 2;
         if(SuffixIsNot.Any()) weight += 2;

         return weight;
      }
   }

   public void OverwriteWith(VocabNoteMatchingRules other)
   {
      SurfaceIsNot.Clear();
      foreach(var item in other.SurfaceIsNot) SurfaceIsNot.Add(item);

      YieldToSurface.Clear();
      foreach(var item in other.YieldToSurface) YieldToSurface.Add(item);

      PrefixIsNot.Clear();
      foreach(var item in other.PrefixIsNot) PrefixIsNot.Add(item);

      SuffixIsNot.Clear();
      foreach(var item in other.SuffixIsNot) SuffixIsNot.Add(item);

      RequiredPrefix.Clear();
      foreach(var item in other.RequiredPrefix) RequiredPrefix.Add(item);

      Save();
   }
}
