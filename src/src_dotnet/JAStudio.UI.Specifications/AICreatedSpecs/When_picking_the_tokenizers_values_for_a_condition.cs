using System;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The editor of a condition on the tokenizer's values, such as <c>InflectedForm.In</c> or <c>PartOfSpeech.Is</c>, in the Matching tab's tree editor: the<br/>
/// values are picked rather than typed, each shown with its English, and found by its Japanese or its English.
/// </summary>
public class When_picking_the_tokenizers_values_for_a_condition : IAIGeneratedSpec
{
   static ConditionTestWithTexts TokenTest(string name) => TokenConditions.Level.TestsWithTexts.Single(it => it.Name == name);

   static ConditionTextsEditorViewModel EditorOfANew(string testName) => ConditionTextsEditorViewModel.ForANewCondition(testName, TokenTest(testName), placed: it => it);

   static PickableValueViewModel TheValue(ConditionTextsEditorViewModel editor, string value) => editor.PickableValues.Single(it => it.Value == value);

   public class of_inflected_forms : When_picking_the_tokenizers_values_for_a_condition
   {
      readonly ConditionTextsEditorViewModel _editor = EditorOfANew("InflectedForm.In");

      [XF] public void it_picks_values_rather_than_having_them_typed() => _editor.PicksValues.Must().BeTrue();
      [XF] public void it_shows_every_value_before_anything_is_typed_to_find_them() => _editor.ShownValues.Count.Must().Be(_editor.PickableValues.Count);
      [XF] public void it_has_no_condition_before_one_is_picked() => _editor.Condition.Must().BeNull();

      public class with_english_typed_to_find_them : of_inflected_forms
      {
         public with_english_typed_to_find_them() => _editor.Filter = "TA-CONNECTION";

         [XF] public void it_shows_the_values_whose_english_has_it_whatever_the_case() => _editor.ShownValues.Select(it => it.Value).Must().Contain("連用タ接続");
         [XF] public void it_shows_no_value_whose_english_lacks_it() =>
            _editor.ShownValues.Select(it => it.English).Must().Satisfy(englishes => englishes.All(it => it.Contains("ta-connection", StringComparison.OrdinalIgnoreCase)));
      }

      public class with_japanese_typed_to_find_them : of_inflected_forms
      {
         public with_japanese_typed_to_find_them() => _editor.Filter = "タ接続";

         [XF] public void it_shows_the_values_whose_japanese_has_it() => _editor.ShownValues.Select(it => it.Value).Must().Contain("連用タ接続");
      }

      public class after_picking_two : of_inflected_forms
      {
         public after_picking_two()
         {
            TheValue(_editor, "連用形").IsPicked = true;
            TheValue(_editor, "連用タ接続").IsPicked = true;
         }

         [XF] public void its_condition_has_both() => _editor.Condition.Must().Be(TokenConditions.InflectedForm.In("連用形", "連用タ接続"));

         public class then_unpicking_one : after_picking_two
         {
            public then_unpicking_one() => TheValue(_editor, "連用形").IsPicked = false;

            [XF] public void its_condition_has_the_other_alone() => _editor.Condition.Must().Be(TokenConditions.InflectedForm.In("連用タ接続"));
         }
      }
   }

   public class of_a_part_of_speech : When_picking_the_tokenizers_values_for_a_condition
   {
      readonly ConditionTextsEditorViewModel _editor = EditorOfANew("PartOfSpeech.Is");

      public class after_picking_one_then_another : of_a_part_of_speech
      {
         public after_picking_one_then_another()
         {
            TheValue(_editor, "名詞").IsPicked = true;
            TheValue(_editor, "名詞・接尾").IsPicked = true;
         }

         [XF] public void the_first_is_unpicked_since_a_condition_has_one_part_of_speech() => TheValue(_editor, "名詞").IsPicked.Must().BeFalse();
         [XF] public void its_condition_is_the_one_picked_last() => _editor.Condition.Must().Be(TokenConditions.PartOfSpeech.Is("名詞", "接尾"));
      }
   }

   public class changing_a_condition_on_two_inflected_forms : When_picking_the_tokenizers_values_for_a_condition
   {
      static readonly Condition TheCondition = TokenConditions.InflectedForm.In("連用形", "連用タ接続");

      readonly ConditionTextsEditorViewModel _editor = ConditionTextsEditorViewModel.ForChanging(TheCondition, TokenTest("InflectedForm.In"), title: "InflectedForm.In");

      [XF] public void both_are_picked_to_start_with() => _editor.PickableValues.Where(it => it.IsPicked).Select(it => it.Value).Must().SequenceEqual(["連用タ接続", "連用形"]);
      [XF] public void its_condition_is_the_one_being_changed() => _editor.Condition.Must().Be(TheCondition);
   }
}
