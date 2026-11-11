using System;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A condition-test that takes texts: a kind of condition the match-condition editor makes from what's typed into it, such as "ends with one of" from<br/>
/// the endings, or "part of speech is" from the part of speech's levels. Each is defined once, with the conditions on its subject, in<br/>
/// <see cref="Tokens.TokenConditions"/> or <see cref="Texts.TextConditions"/>, and offered by the subject's <see cref="IConditionLevel"/>.
/// </summary>
public sealed class ConditionTestWithTexts
{
   //review:matching: "condition-test", from the design document's terms: what a single condition checks about its subject. The facts are
   //   condition-tests too, ones that take no value, but they're conditions in themselves, so only those that take texts have a type.
   readonly Func<string, Condition> _madeFromTypedText;
   readonly Func<Condition, string?> _typedTextOf;

   ConditionTestWithTexts(string name, string howToTypeTheTexts, Func<string, Condition> madeFromTypedText, Func<Condition, string?> typedTextOf)
   {
      Name = name;
      HowToTypeTheTexts = howToTypeTheTexts;
      _madeFromTypedText = madeFromTypedText;
      _typedTextOf = typedTextOf;
   }

   /// <summary>The test as the editor shows it, such as "ends with one of".</summary>
   public string Name { get; }

   /// <summary>What to type, as the editor tells it, such as "The texts, separated by spaces".</summary>
   public string HowToTypeTheTexts { get; }

   /// <summary>The condition <paramref name="typedText"/> describes.</summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When it describes no condition that can be made, such as an empty text.</exception>
   /// <exception cref="System.Text.RegularExpressions.RegexParseException">When it's meant to be a regular expression, and isn't a valid one.</exception>
   public Condition MadeFrom(string typedText) => _madeFromTypedText(typedText);

   /// <summary>Whether <paramref name="condition"/> is of this test, so the editor can change its texts.</summary>
   public bool Made(Condition condition) => _typedTextOf(condition) is not null;

   /// <summary>The text that, typed into the editor, makes <paramref name="condition"/>, a condition of this test: what the editor starts from to change it.</summary>
   public string TypedTextOf(Condition condition) =>
      _typedTextOf(condition) ?? throw new ArgumentException($"Not a condition that {Name}: {condition}", nameof(condition));

   /// <summary>
   /// The test whose conditions are the <typeparamref name="TCondition"/>s: <paramref name="madeFromTypedText"/> makes one from what's typed, and<br/>
   /// <paramref name="typedTextOf"/> gives back what typing makes it.
   /// </summary>
   internal static ConditionTestWithTexts Of<TCondition>(string name, string howToTypeTheTexts, Func<string, TCondition> madeFromTypedText, Func<TCondition, string> typedTextOf)
      where TCondition : Condition =>
      new(name, howToTypeTheTexts, madeFromTypedText, condition => condition is TCondition ofThisTest ? typedTextOf(ofThisTest) : null);
}
