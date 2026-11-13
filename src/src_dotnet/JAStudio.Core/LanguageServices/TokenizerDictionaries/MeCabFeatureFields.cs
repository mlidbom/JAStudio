using System.Collections.Generic;
using System.Text;

namespace JAStudio.Core.LanguageServices.TokenizerDictionaries;

/// <summary>
/// The fields of a token's features as MeCab gives them (<see cref="MeCab.MeCabNode.Feature"/>): comma-separated, as in a CSV file, a field<br/>
/// that holds commas in double quotes. UniDic's accent fields do, such as <c>"1,0"</c>, so splitting at every comma misplaces the fields after them.
/// </summary>
static class MeCabFeatureFields
{
   /// <summary>The fields of <paramref name="features"/>, a quoted field without its quotes.</summary>
   public static IReadOnlyList<string> Of(string features)
   {
      var fields = new List<string>();
      var field = new StringBuilder();
      var isInQuotes = false;
      foreach(var character in features)
      {
         if(character == '"') isInQuotes = !isInQuotes;
         else if(character == ',' && !isInQuotes) EndTheField();
         else field.Append(character);
      }

      EndTheField();
      return fields;

      void EndTheField()
      {
         fields.Add(field.ToString());
         field.Clear();
      }
   }
}
