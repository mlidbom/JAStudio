using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Contracts;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>
/// A condition that a token's inflection type, as the tokenizer gives it, is one of <see cref="Names"/>, or in the family of one of them: 五段 is met<br/>
/// by every godan type, such as 五段・ラ行, and 特殊 by every special one, such as 特殊・タ. Made by <see cref="TokenConditions.InflectionTypeIsOneOf"/>.
/// </summary>
/// <remarks>
/// A piece JAStudio split off a token, such as a dictionary-form verb's ending, has the inflection type of the token it was split from<br/>
/// (<see cref="IAnalysisToken.SourceToken"/>).
/// </remarks>
public sealed record InflectionTypeIsOneOf : Condition<IAnalysisToken>
{
   //review:matching: "inflection type" and "inflected form" for 活用型 and 活用形, after Janome's infl_type and infl_form. English grammars
   //   of Japanese mostly say conjugation type and conjugation form, as UniDic's documentation does. Linguistics uses both words.

   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When a name is neither one of the tokenizer's inflection types nor one of their families.</exception>
   internal InflectionTypeIsOneOf(TextSet names)
   {
      Names = names;
      var whyTheyAreNoInflectionTypes = WhyTheyAreNoInflectionTypes(names);
      Contract.Argument.Assert(whyTheyAreNoInflectionTypes is null, () => $"{whyTheyAreNoInflectionTypes}");
   }

   /// <summary>The condition typed into the match-condition editor: the names separated by spaces.</summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When <see cref="WhyTypedNamesMakeNoCondition"/> says why they make none.</exception>
   internal static InflectionTypeIsOneOf FromTyped(string typedNames) => new(TextSet.FromTyped(typedNames));

   /// <summary>Why the names typed into the match-condition editor make no condition, such as a name the tokenizer has no type of, or null when they make one.</summary>
   internal static string? WhyTypedNamesMakeNoCondition(string typedNames) =>
      TextSet.WhyTypedTextsMakeNoSet(typedNames) ?? WhyTheyAreNoInflectionTypes(TextSet.FromTyped(typedNames));

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
   /// The condition with each type's description, such as "inflection type is one of 五段・ラ行 (Godan verb with 'ru' ending)", and a family's as<br/>
   /// "五段 (every 五段 type)".
   /// </summary>
   public override IEnumerable<string> DescriptionLines()
   {
      return [$"inflection type is one of {string.Join(", ", Names.Texts.Select(Described))}"];

      static string Described(string name) => $"{name} ({DescriptionOf(name)})";

      static string DescriptionOf(string name) => InflectionTypes.IsKnown(name) ? InflectionTypes.GetByName(name).Description : $"every {name} type";
   }

   /// <summary>The key this condition is stored under in the corpus.</summary>
   internal const string JsonKey = "inflectionTypeIsOneOf";

   internal override JsonObject ToJson() => new() { [JsonKey] = Names.ToJson() };
}
