using System;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

/// <summary>A text written as C# writes it in code, as a condition's code has its texts: <c>"ん"</c>, or a regular expression's <c>@"^よ.$"</c>.</summary>
static class StringLiteral
{
   /// <summary><paramref name="text"/> as a C# string literal, such as <c>"ん"</c>, with its quotes and backslashes escaped.</summary>
   internal static string Of(string text) => $"\"{text.Replace(@"\", @"\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

   /// <summary><paramref name="text"/> as a C# verbatim string literal, such as <c>@"^よ.$"</c>, as a regular expression is written: only its quotes are doubled.</summary>
   internal static string VerbatimOf(string text) => $"@\"{text.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
