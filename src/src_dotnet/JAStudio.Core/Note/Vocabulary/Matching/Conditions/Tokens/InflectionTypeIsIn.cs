using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>
/// A condition that a token's inflection type, as the tokenizer gives it, is one of <see cref="Names"/>, or in the family of one of them: 五段 is met<br/>
/// by every godan type, such as 五段・ラ行, and 特殊 by every special one, such as 特殊・タ. Written <c>InflectionType.In(...)</c>: made by<br/>
/// <see cref="InflectionTypeTest{TFrom}.In"/>, on <see cref="TokenConditions.InflectionType"/> or on a way to a token, such as <c>LastToken.InflectionType</c>.
/// </summary>
/// <remarks>
/// A piece JAStudio split off a token, such as a dictionary-form verb's ending, has the inflection type of the token it was split from<br/>
/// (<see cref="IAnalysisToken.SourceToken"/>).
/// </remarks>
public sealed record InflectionTypeIsIn : Condition<IAnalysisToken>
{
   //review:matching: "inflection type" and "inflected form" for 活用型 and 活用形, after Janome's infl_type and infl_form. English grammars
   //   of Japanese mostly say conjugation type and conjugation form, as UniDic's documentation does. Linguistics uses both words.

   //review:matching: InflectionTypeIsIn, InflectedFormIsIn and TextIsIn, for the conditions whose code is InflectionType.In, InflectedForm.In
   //   and Surface.In: they were ...IsOneOf, a second word for the code's In. "IsIn" rather than the code's bare "In", since TextIn or
   //   InflectionTypeIn reads as no sentence, as PartOfSpeechIs and TextStartsWithOneOf do. Their keys in the corpus keep the old names.

   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When a name is neither one of the tokenizer's inflection types nor one of their families.</exception>
   internal InflectionTypeIsIn(TextSet names)
   {
      Names = names;
      var whyTheyAreNoInflectionTypes = WhyTheyAreNoInflectionTypes(names);
      Contract.Argument.Assert(whyTheyAreNoInflectionTypes is null, () => $"{whyTheyAreNoInflectionTypes}");
   }

   /// <summary>The condition typed into the match-condition editor: the names separated by spaces.</summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When <see cref="WhyTypedNamesMakeNoCondition"/> says why they make none.</exception>
   internal static InflectionTypeIsIn FromTyped(string typedNames) => new(TextSet.FromTyped(typedNames));

   /// <summary>Why the names typed into the match-condition editor make no condition, such as a name the tokenizer has no type of, or null when they make one.</summary>
   internal static string? WhyTypedNamesMakeNoCondition(string typedNames) =>
      TextSet.WhyTypedTextsMakeNoSet(typedNames) ?? WhyTheyAreNoInflectionTypes(TextSet.FromTyped(typedNames));

   /// <summary>Every inflection type the tokenizer gives, and every family of them, each with its English, in ordinal order: what the match-condition editor offers to pick from.</summary>
   internal static IReadOnlyList<TokenizerValue> PickableValues() =>
      [.. InflectionTypes.NamesOfTheTypesAndTheirFamilies.Order(StringComparer.Ordinal).Select(ValueOf)];

   /// <summary>
   /// The inflection type, or family of them, named <paramref name="name"/>, with its English: such as 五段・ラ行 with Godan verb with 'ru' ending, or 五段<br/>
   /// with every godan type.
   /// </summary>
   static TokenizerValue ValueOf(string name) => new(name, InflectionTypes.EnglishOf(name));

   /// <summary>Why <paramref name="names"/> aren't all the tokenizer's inflection types, or families of them, or null when they are.</summary>
   static string? WhyTheyAreNoInflectionTypes(TextSet names)
   {
      var unknownNames = names.Texts.Where(it => !InflectionTypes.IsKnown(it) && !InflectionTypes.IsKnownFamily(it)).ToList();
      return unknownNames.Count == 0 ? null : $"The tokenizer has no inflection types, nor families of them, named {string.Join(", ", unknownNames)}.";
   }

   /// <summary>The inflection types' names, in Japanese, as the tokenizer gives them, such as 五段・ラ行, or the family's, such as 五段.</summary>
   public TextSet Names { get; }

   public override bool IsMetBy(IAnalysisToken subject)
   {
      var inflectionType = subject.SourceToken.InflectionType;
      return Names.Texts.Contains(inflectionType.Name) || Names.Texts.Contains(inflectionType.Family);
   }

   /// <summary>
   /// The condition in code, each type with its English as a comment, one to a line when there are several: <c>InflectionType.In(五段・ラ行)  // Godan<br/>
   /// verb with 'ru' ending</c>, and a family's as <c>五段  // every godan type</c>.
   /// </summary>
   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) => writing.LinesListing("InflectionType.In", [.. Names.Texts.Select(ValueOf)]);

   /// <summary>
   /// The key this condition is stored under in the corpus. It keeps the name the condition had before its code was written <c>In</c>: a new key would change<br/>
   /// every note that stores one, and the stored notes are migrated only when there's more to gain.
   /// </summary>
   internal const string JsonKey = "inflectionTypeIsOneOf";

   internal override JsonObject ToJson() => new() { [JsonKey] = Names.ToJson() };
}
