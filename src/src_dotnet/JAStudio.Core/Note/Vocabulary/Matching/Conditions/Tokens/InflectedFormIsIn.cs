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
/// A condition that a token's inflected form, as the tokenizer gives it, is one of <see cref="Names"/>, such as 連用形-促音便, the continuative form<br/>
/// whose ending sound becomes っ before た, as in 待った. Written <c>InflectedForm.In(...)</c>: made by <see cref="InflectedFormTest{TFrom}.In"/>, on <see cref="TokenConditions.InflectedForm"/> or on a way to<br/>
/// a token, such as <c>LastToken.InflectedForm</c>.
/// </summary>
/// <remarks>
/// A piece JAStudio split off a token, such as a dictionary-form verb's ending, has the inflected form of the token it was split from<br/>
/// (<see cref="IAnalysisToken.SourceToken"/>).
/// </remarks>
public sealed record InflectedFormIsIn : Condition<IAnalysisToken>
{
   //review:matching: "inflected form", after JNToken.InflectedForm, whose type is InflectionForm: two names for one concept. The value,
   //   such as 連用形-促音便, is a kind of form, not the inflected word itself, so "inflection form" tells the truth better. Not renamed yet,
   //   since the question beside InflectionTypeIsIn, inflection or conjugation, may rename it again: one rename for both, across the
   //   tokenizer, the MCP's AnalyzedToken and this model.

   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When a name isn't one of the tokenizer's inflected forms.</exception>
   internal InflectedFormIsIn(TextSet names)
   {
      Names = names;
      var whyTheyAreNoInflectedForms = WhyTheyAreNoInflectedForms(names);
      Contract.Argument.Assert(whyTheyAreNoInflectedForms is null, () => $"{whyTheyAreNoInflectedForms}");
   }

   /// <summary>The condition typed into the tree editor: the names separated by spaces.</summary>
   /// <exception cref="Compze.Contracts.Exceptions.ArgumentAssertionFailedException">When <see cref="WhyTypedNamesMakeNoCondition"/> says why they make none.</exception>
   internal static InflectedFormIsIn FromTyped(string typedNames) => new(TextSet.FromTyped(typedNames));

   /// <summary>Why the names typed into the tree editor make no condition, such as a name the tokenizer has no form of, or null when they make one.</summary>
   internal static string? WhyTypedNamesMakeNoCondition(string typedNames) =>
      TextSet.WhyTypedTextsMakeNoSet(typedNames) ?? WhyTheyAreNoInflectedForms(TextSet.FromTyped(typedNames));

   /// <summary>Every inflected form the tokenizer gives, with its English, in ordinal order: what the tree editor offers to pick from.</summary>
   internal static IReadOnlyList<TokenizerValue> PickableValues() =>
      [.. InflectionForms.All.OrderBy(it => it.Name, StringComparer.Ordinal).Select(ValueOf)];

   /// <summary>The inflected form <paramref name="form"/> with its English, such as 連用形-促音便 with Continuative with っ sound change.</summary>
   static TokenizerValue ValueOf(InflectionForm form) => new(form.Name, form.Description);

   /// <summary>Why <paramref name="names"/> aren't all the tokenizer's inflected forms, or null when they are.</summary>
   static string? WhyTheyAreNoInflectedForms(TextSet names)
   {
      var unknownNames = names.Texts.Where(it => !InflectionForms.IsKnown(it)).ToList();
      return unknownNames.Count == 0 ? null : $"The tokenizer has no inflected forms named {string.Join(", ", unknownNames)}.";
   }

   /// <summary>The inflected forms' names, in Japanese, as the tokenizer gives them, such as 連用形-促音便.</summary>
   public TextSet Names { get; }

   public override bool IsMetBy(IAnalysisToken subject) => Names.Texts.Contains(subject.SourceToken.InflectedForm.Name);

   /// <summary>
   /// The condition in code, each form with its English as a comment, one to a line when there are several:<br/>
   /// <c>InflectedForm.In(連用形-促音便)  // Continuative with っ sound change</c>.
   /// </summary>
   internal override IEnumerable<CodeLine> CodeLines(CodeWriting writing) =>
      writing.LinesListing("InflectedForm.In", [.. Names.Texts.Select(InflectionForms.GetByName).Select(ValueOf)]);

   /// <summary>
   /// The key this condition is stored under in the corpus. It keeps the name the condition had before its code was written <c>In</c>: a new key would change<br/>
   /// every note that stores one, and the stored notes are migrated only when there's more to gain.
   /// </summary>
   internal const string JsonKey = "inflectedFormIsOneOf";

   internal override JsonObject ToJson() => new() { [JsonKey] = Names.ToJson() };
}
