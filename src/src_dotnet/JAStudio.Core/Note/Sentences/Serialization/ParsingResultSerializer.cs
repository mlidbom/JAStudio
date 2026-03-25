using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Utilities.Logging;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.Sentences.Serialization;

class ParsingResultSerializer : IObjectSerializer<ParsingResult>
{
   static readonly string NewlineReplacement = $"NEWLINE{StringExtensions.InvisibleSpace}";

   public ParsingResult Deserialize(string serialized)
   {
      var rows = serialized.Split('\n');
      if(rows.Length < 2)
      {
         return new ParsingResult([], "", "");
      }

      try
      {
         if(string.IsNullOrEmpty(serialized))
         {
            return new ParsingResult([], "", "");
         }

         var versionParts = rows[0].Split('|');
         var parserVersion = versionParts[0];
         var tokenizerVersion = versionParts.Length > 1 ? versionParts[1] : "";

         var parsedWords = rows.Skip(2)
                               .Select(row => ParsedWordSerializer.FromRow(row))
                               .ToList();

         return new ParsingResult(
            parsedWords,
            RestoreNewline(rows[1]),
            parserVersion,
            tokenizerVersion
         );
      }
      catch(Exception ex)
      {
         this.Log().Warning($"Failed to deserialize ParsingResult:\nmessage:\n{ex.Message}\n{serialized}");
         return new ParsingResult([], "", "");
      }
   }

   static string ReplaceNewline(string value) => value.Replace("\n", NewlineReplacement);

   static string RestoreNewline(string serializedValue) => serializedValue.Replace(NewlineReplacement, "\n");

   public string Serialize(ParsingResult instance)
   {
      var versionLine = string.IsNullOrEmpty(instance.TokenizerVersion)
         ? instance.ParserVersion
         : $"{instance.ParserVersion}|{instance.TokenizerVersion}";

      var lines = new List<string>
                  {
                     versionLine,
                     ReplaceNewline(instance.Sentence)
                  };

      lines.AddRange(instance.ParsedWords.Select(word => ParsedWordSerializer.ToRow(word)));

      return string.Join("\n", lines);
   }
}
