using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

/// <summary>
/// The conditions on a text, written in C# as a condition's code reads (<see cref="Code.ConditionCode"/>), on whatever leads to a text: a word's<br/>
/// <see cref="Words.WordConditions.Surface"/>, its <see cref="Words.WordConditions.FollowingText"/>, a token's surface, so that<br/>
/// <c>Surface.In("本の")</c> and <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c> are C# as they stand. Each forwards to its condition in<br/>
/// <see cref="TextConditions"/>, which defines it.
/// </summary>
public static class TextConditionsInCode
{
   extension<TFrom>(IConditionPath<TFrom, string> text) where TFrom : class
   {
      /// <summary>The text is empty (<see cref="TextConditions.IsEmpty"/>).</summary>
      public Condition<TFrom> IsEmpty => text.Placed(TextConditions.IsEmpty);

      /// <summary>The text is one of <paramref name="texts"/> (<see cref="TextConditions.In"/>).</summary>
      public Condition<TFrom> In(params string[] texts) => text.Placed(TextConditions.In(texts));

      /// <summary>The text starts with one of <paramref name="texts"/> (<see cref="TextConditions.StartsWithOneOf"/>).</summary>
      public Condition<TFrom> StartsWithOneOf(params string[] texts) => text.Placed(TextConditions.StartsWithOneOf(texts));

      /// <summary>The text ends with one of <paramref name="texts"/> (<see cref="TextConditions.EndsWithOneOf"/>).</summary>
      public Condition<TFrom> EndsWithOneOf(params string[] texts) => text.Placed(TextConditions.EndsWithOneOf(texts));

      /// <summary>The text contains one of <paramref name="texts"/> (<see cref="TextConditions.ContainsOneOf"/>).</summary>
      public Condition<TFrom> ContainsOneOf(params string[] texts) => text.Placed(TextConditions.ContainsOneOf(texts));

      /// <summary>The regular expression <paramref name="pattern"/> matches the text, or any part of it (<see cref="TextConditions.MatchesRegularExpression"/>).</summary>
      public Condition<TFrom> MatchesRegularExpression(string pattern) => text.Placed(TextConditions.MatchesRegularExpression(pattern));

      /// <summary>The text meets all of <paramref name="conditions"/>, conditions on a text.</summary>
      public Condition<TFrom> AllOf(params Condition<string>[] conditions) => text.Placed(ConditionGroup.AllOf(conditions));

      /// <summary>The text meets any of <paramref name="conditions"/>, conditions on a text.</summary>
      public Condition<TFrom> AnyOf(params Condition<string>[] conditions) => text.Placed(ConditionGroup.AnyOf(conditions));
   }
}
