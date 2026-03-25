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

         var parserVersion = rows[0].Split('|')[0];

         var parsedWords = rows.Skip(2)
                               .Select(row => ParsedWordSerializer.FromRow(row))
                               .ToList();

         return new ParsingResult(
            parsedWords,
            RestoreNewline(rows[1]),
            parserVersion
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
      var lines = new List<string>
                  {
                     instance.ParserVersion,
                     ReplaceNewline(instance.Sentence)
                  };

      lines.AddRange(instance.ParsedWords.Select(word => ParsedWordSerializer.ToRow(word)));

      return string.Join("\n", lines);
   }
}
