using System.Linq;
using System.Reflection;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Matching;

/// <summary>
/// A condition written in C# as its code reads (<see cref="TokenConditionsInCode"/>, <see cref="TextConditionsInCode"/>), such as<br/>
/// <c>!PrecedingToken.Surface.EndsWithOneOf("ん")</c>, is the condition the model's own parts build, and its code is what the C# says (Magnus,<br/>
/// 2026-11-12: "If that is not hard, by all means do it").
/// </summary>
public class When_writing_a_condition_in_CSharp_as_its_code_reads : IAIGeneratedSpec
{
   [XF] public void a_fact_about_a_token_is_the_condition_that_the_subject_meets_the_fact() =>
      PrecedingToken.IsAGodanVerb.Must().Be(PrecedingToken.Meets(TokenConditions.IsAGodanVerb));

   [XF] public void a_test_of_a_tokens_surface_is_the_condition_that_the_token_meets_it_on_its_surface() =>
      PrecedingToken.Surface.EndsWithOneOf("ん").Must().Be(PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん"))));

   [XF] public void the_not_operator_is_the_negation() => (!PrecedingToken.IsAGodanVerb).Must().Be(ConditionGroup.Not(PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));

   [XF] public void a_group_on_a_token_is_the_condition_that_the_token_meets_the_group() =>
      FirstToken.AnyOf(TokenConditions.IsAGodanPotentialStem, TokenConditions.IsAGodanPotentialInflection)
                .Must().Be(FirstToken.Meets(ConditionGroup.AnyOf(TokenConditions.IsAGodanPotentialStem, TokenConditions.IsAGodanPotentialInflection)));

   [XF] public void a_tokens_part_of_speech_is_the_part_of_speech_condition_on_the_token() =>
      FollowingToken.PartOfSpeech.Is("名詞", "数詞").Must().Be(FollowingToken.Meets(TokenConditions.PartOfSpeech.Is("名詞", "数詞")));

   [XF] public void a_tokens_inflected_form_is_the_inflected_form_condition_on_the_token() =>
      LastToken.InflectedForm.In("連用形-一般").Must().Be(LastToken.Meets(TokenConditions.InflectedForm.In("連用形-一般")));

   [XF] public void a_part_of_speech_inside_a_group_on_a_token_is_written_on_the_token_itself() =>
      LastToken.AllOf(TokenConditions.PartOfSpeech.Is("動詞")).Must().Be(LastToken.Meets(ConditionGroup.AllOf(TokenConditions.PartOfSpeech.Is("動詞"))));

   [XF] public void a_test_of_a_words_text_is_the_condition_that_the_text_meets_it() => Surface.In("本の").Must().Be(Surface.Meets(TextConditions.In("本の")));

   [XF] public void the_code_of_a_condition_written_so_reads_as_the_CSharp_does() =>
      (!FollowingToken.Surface.StartsWithOneOf("湧")).ToString().Must().Be("!FollowingToken.Surface.StartsWithOneOf(\"湧\")");

   [XF] public void every_fact_about_a_token_can_be_written_on_a_subject_by_its_name() =>
      TokenConditions.Level.Facts.Select(it => it.Name).Must().Satisfy(names => names.All(name => MethodsOf(typeof(TokenConditionsInCode)).Contains($"get_{name}")));

   [XF] public void every_test_of_a_text_can_be_written_on_a_subject_by_its_name() =>
      TextConditions.Level.TestsWithTexts.Select(it => it.Name).Must().Satisfy(names => names.All(name => MethodsOf(typeof(TextConditionsInCode)).Contains(name)));

   /// <summary>The names of <paramref name="type"/>'s public static methods: an extension property's getter is one, named <c>get_</c> and the property's name.</summary>
   static string[] MethodsOf(System.Type type) => [.. type.GetMethods(BindingFlags.Public | BindingFlags.Static).Select(it => it.Name)];
}
