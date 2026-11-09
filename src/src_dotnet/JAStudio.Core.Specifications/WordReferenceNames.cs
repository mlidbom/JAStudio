using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Specifications;

/// <summary>Word-references (<see cref="WordReference"/>) as the specifications give and compare them: by their names, as a person types them, such as <c>入る:masu</c>.</summary>
static class WordReferenceNames
{
   /// <summary>The word-references <paramref name="names"/> name (<see cref="WordReference.FromDisambiguationName"/>), in their order.</summary>
   public static List<WordReference> Words(params string[] names) => [.. names.Select(WordReference.FromDisambiguationName)];

   /// <summary>The names of <paramref name="words"/> (<see cref="WordReference.DisambiguationName"/>), in their order.</summary>
   public static string[] NamesOf(IEnumerable<WordReference> words) => [.. words.Select(it => it.DisambiguationName)];

   /// <summary>The names of <paramref name="words"/>, a set, in ordinal order, so that two sets compare equal whatever order they hold their words in.</summary>
   public static string[] SortedNamesOf(IEnumerable<WordReference> words) => [.. NamesOf(words).Order(StringComparer.Ordinal)];
}
