using System.Text.RegularExpressions;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>A condition that could never mean anything can't be made: one naming what the tokenizer never gives, or comparing with no text.</summary>
public class When_creating_a_condition : IAIGeneratedSpec
{
   [XF] public void one_on_a_part_of_speech_the_tokenizer_never_gives_throws() =>
      Invoking(() => TokenConditions.PartOfSpeech.Is("名詞", "存在しない")).Must().Throw<ArgumentAssertionFailedException>();

   [XF] public void one_on_an_inflection_type_the_tokenizer_has_none_of_throws() =>
      Invoking(() => TokenConditions.InflectionType.In("五段", "存在しない")).Must().Throw<ArgumentAssertionFailedException>();

   [XF] public void one_on_an_inflected_form_the_tokenizer_has_none_of_throws() =>
      Invoking(() => TokenConditions.InflectedForm.In("存在しない")).Must().Throw<ArgumentAssertionFailedException>();

   [XF] public void a_text_comparison_with_no_texts_throws() => Invoking(() => TextConditions.StartsWithOneOf()).Must().Throw<ArgumentAssertionFailedException>();

   [XF] public void a_text_comparison_with_an_empty_text_throws() => Invoking(() => TextConditions.EndsWithOneOf("ん", "")).Must().Throw<ArgumentAssertionFailedException>();

   [XF] public void one_with_an_invalid_regular_expression_throws() => Invoking(() => TextConditions.MatchesRegularExpression("(")).Must().Throw<RegexParseException>();
}
