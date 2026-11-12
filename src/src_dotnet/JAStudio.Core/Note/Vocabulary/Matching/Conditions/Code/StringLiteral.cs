using System;
using System.Globalization;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>A text written as C# writes it in code, as a condition's code has its texts: <c>"ん"</c>, or a regular expression's <c>@"^よ.$"</c>.</summary>
static class StringLiteral
{
   /// <summary>
   /// <paramref name="text"/> as a C# string literal, such as <c>"ん"</c>: its quotes and backslashes escaped, and the characters that show as nothing written as<br/>
   /// C# escapes them, such as a tab as <c>\t</c> and a zero-width space by its code point, so that the code shows every character a text has.
   /// </summary>
   internal static string Of(string text) => $"\"{string.Concat(text.Select(Escaped))}\"";

   static string Escaped(char character) => character switch
   {
      '\\' => @"\\",
      '"' => "\\\"",
      '\t' => @"\t",
      '\n' => @"\n",
      '\r' => @"\r",
      '\0' => @"\0",
      _ when ShowsAsNothing(character) => $@"\u{(int)character:X4}",
      _ => character.ToString()
   };

   /// <summary>Whether <paramref name="character"/> shows as nothing: a control character, or a format character such as a zero-width space.</summary>
   static bool ShowsAsNothing(char character) =>
      char.GetUnicodeCategory(character) is UnicodeCategory.Control or UnicodeCategory.Format or UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator;

   /// <summary><paramref name="text"/> as a C# verbatim string literal, such as <c>@"^よ.$"</c>, as a regular expression is written: only its quotes are doubled.</summary>
   internal static string VerbatimOf(string text) => $"@\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
