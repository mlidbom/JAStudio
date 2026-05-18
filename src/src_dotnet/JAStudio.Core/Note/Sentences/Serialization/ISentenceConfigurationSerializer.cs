using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Note.Sentences.Serialization;

public interface ISentenceConfigurationSerializer
{
   public static ISentenceConfigurationSerializer Instance { get; } = new SentenceConfigurationSerializer();
   SentenceConfiguration Deserialize(string json, Action saveCallback);
   string Serialize(SentenceConfiguration config);

   class SentenceConfigurationSerializer : ISentenceConfigurationSerializer
   {
      static string _emptyObjectJson = "";

      public SentenceConfigurationSerializer()
      {
         if(string.IsNullOrEmpty(_emptyObjectJson))
         {
            _emptyObjectJson = Serialize(Deserialize("", () => {}));
         }
      }

      public SentenceConfiguration Deserialize(string json, Action saveCallback)
      {
         if(string.IsNullOrEmpty(json))
         {
            return new SentenceConfiguration(
               [],
               WordExclusionSet.Empty(saveCallback),
               WordExclusionSet.Empty(saveCallback)
            );
         }

         using var doc = JsonDocument.Parse(json);
         var reader = new JsonReader(doc.RootElement);
         return new SentenceConfiguration(
            reader.GetStringList("highlighted_words", []),
            new WordExclusionSet(saveCallback, reader.GetObjectList("incorrect_matches", r => WordExclusion.FromReader(r), [])),
            new WordExclusionSet(saveCallback, reader.GetObjectList("hidden_matches", r => WordExclusion.FromReader(r), []))
         );
      }

      public string Serialize(SentenceConfiguration config)
      {
         var jsonDict = new Dictionary<string, object>();

         if(config.HighlightedWords.Any())
         {
            jsonDict["highlighted_words"] = config.HighlightedWords.ToList();
         }

         if(config.IncorrectMatches.Words().Any())
         {
            jsonDict["incorrect_matches"] = config.IncorrectMatches.Get().Select(e => e.ToDict()).ToList();
         }

         if(config.HiddenMatches.Get().Any())
         {
            jsonDict["hidden_matches"] = config.HiddenMatches.Get().Select(e => e.ToDict()).ToList();
         }

         var json = JsonHelper.DictToJson(jsonDict);
         return json != _emptyObjectJson ? json : "";
      }
   }
}
