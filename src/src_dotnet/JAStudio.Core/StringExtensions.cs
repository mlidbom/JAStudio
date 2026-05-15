using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JAStudio.Core;

public static class StringExtensions
{
   static readonly Regex HtmlTagRegex = new(@"<[^>]*>", RegexOptions.Compiled);

   public static string StripHtmlMarkup(string input)
   {
      if(string.IsNullOrEmpty(input))
      {
         return string.Empty;
      }

      return HtmlTagRegex.Replace(input.Replace("&nbsp;", " "), string.Empty);
   }

   public static List<string> ExtractCommaSeparatedValues(string input)
   {
      if(string.IsNullOrWhiteSpace(input))
      {
         return [];
      }

      // Support both comma and Japanese comma
      var separators = new[] { ',', '、' };
      return input.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                  .Select(s => s.Trim())
                  .Where(s => !string.IsNullOrEmpty(s))
                  .ToList();
   }

   public static string ReplaceWord(string word, string replacement, string text)
   {
      if(string.IsNullOrEmpty(text))
      {
         return text;
      }

      // Use word boundary detection to match whole words only (matching Python's implementation)
      var pattern = $@"\b{Regex.Escape(word)}\b";
      return Regex.Replace(text, pattern, replacement);
   }

   public static string PadToLength(string value, int targetLength, double spaceScaling = 1.0)
   {
      if(string.IsNullOrEmpty(value))
      {
         value = string.Empty;
      }

      var padding = Math.Max(0, targetLength - value.Length);
      return value + new string(' ', (int)(padding * spaceScaling));
   }

   public static string FirstNumber(string input)
   {
      if(string.IsNullOrEmpty(input))
      {
         return string.Empty;
      }

      var match = Regex.Match(input, @"\d+");
      return match.Success ? match.Value : string.Empty;
   }

   public const char InvisibleSpace = '\u200B'; // Zero-width space
}
