using System;
using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A condition-test that takes texts: a kind of condition the match-condition editor makes from what's typed into it, such as <c>EndsWithOneOf</c> from<br/>
/// the endings, or <c>PartOfSpeech.Is</c> from the part of speech's levels. Each is defined once, with the conditions on its subject, in<br/>
/// <see cref="Tokens.TokenConditions"/> or <see cref="Texts.TextConditions"/>, and offered by the subject's <see cref="IConditionLevel"/>.
/// </summary>
public sealed class ConditionTestWithTexts
{
   //review:matching: "condition-test", from the design document's terms: what a single condition checks about its subject. The facts are
   //   condition-tests too, ones that take no value, but they're conditions in themselves, so only those that take texts have a type.
   readonly Func<string, string?> _whyTypedTextMakesNoCondition;
   readonly Func<string, Condition> _madeFromTypedText;
   readonly Func<Condition, string?> _typedTextOf;
   readonly Func<IReadOnlyList<PickableValue>> _pickableValues;

   ConditionTestWithTexts(string name,
                          string howToTypeTheTexts,
                          Func<string, string?> whyTypedTextMakesNoCondition,
                          Func<string, Condition> madeFromTypedText,
                          Func<Condition, string?> typedTextOf,
                          Func<IReadOnlyList<PickableValue>> pickableValues,
                          bool picksOne)
   {
      Name = name;
      _pickableValues = pickableValues;
      PicksOne = picksOne;
      HowToTypeTheTexts = howToTypeTheTexts;
      _whyTypedTextMakesNoCondition = whyTypedTextMakesNoCondition;
      _madeFromTypedText = madeFromTypedText;
      _typedTextOf = typedTextOf;
   }

   /// <summary>The test's name in code, as the editor shows it, such as <c>EndsWithOneOf</c>.</summary>
   public string Name { get; }

   /// <summary>
   /// The values to pick the texts from, for a test whose texts are the tokenizer's values, such as <c>PartOfSpeech.Is</c>: picking them types them,<br/>
   /// separated by spaces. None for a test whose texts are typed freely, such as <c>EndsWithOneOf</c>.
   /// </summary>
   public IReadOnlyList<PickableValue> PickableValues => _pickableValues();

   /// <summary>Whether only one of the <see cref="PickableValues"/> is picked, as a part of speech is, rather than any number of them, as inflected forms are.</summary>
   public bool PicksOne { get; }

   /// <summary>What to type, as the editor tells it, such as "The texts, separated by spaces".</summary>
   public string HowToTypeTheTexts { get; }

   /// <summary>
   /// Why <paramref name="typedText"/> makes no condition, such as a part of speech the tokenizer never gives, or null when it makes one: what the<br/>
   /// editor shows while what's typed is on its way to a condition.
   /// </summary>
   public string? WhyItMakesNoConditionFrom(string typedText) => _whyTypedTextMakesNoCondition(typedText);

   /// <summary>
   /// The condition <paramref name="typedText"/> makes. Only for text <see cref="WhyItMakesNoConditionFrom"/> has no reason against: the conditions'<br/>
   /// constructors assert the same rules, so making one from any other is a bug, and fails loudly.
   /// </summary>
   public Condition MadeFrom(string typedText) => _madeFromTypedText(typedText);

   /// <summary>Whether <paramref name="condition"/> is of this test, so the editor can change its texts.</summary>
   public bool Made(Condition condition) => _typedTextOf(condition) is not null;

   /// <summary>The text that, typed into the editor, makes <paramref name="condition"/>, a condition of this test: what the editor starts from to change it.</summary>
   public string TypedTextOf(Condition condition) =>
      _typedTextOf(condition) ?? throw new ArgumentException($"Not a condition that {Name}: {condition}", nameof(condition));

   /// <summary>
   /// The test whose conditions are the <typeparamref name="TCondition"/>s: <paramref name="whyTypedTextMakesNoCondition"/> says why what's typed<br/>
   /// makes none, <paramref name="madeFromTypedText"/> makes one from what's typed, and <paramref name="typedTextOf"/> gives back what typing makes it.<br/>
   /// <paramref name="pickableValues"/> lists the values to pick from, if they're the tokenizer's, and <paramref name="picksOne"/> says whether only one is picked.
   /// </summary>
   internal static ConditionTestWithTexts Of<TCondition>(string name,
                                                         string howToTypeTheTexts,
                                                         Func<string, string?> whyTypedTextMakesNoCondition,
                                                         Func<string, TCondition> madeFromTypedText,
                                                         Func<TCondition, string> typedTextOf,
                                                         Func<IReadOnlyList<PickableValue>>? pickableValues = null,
                                                         bool picksOne = false)
      where TCondition : Condition =>
      new(name,
          howToTypeTheTexts,
          whyTypedTextMakesNoCondition,
          madeFromTypedText,
          condition => condition is TCondition ofThisTest ? typedTextOf(ofThisTest) : null,
          pickableValues ?? (() => []),
          picksOne);
}
