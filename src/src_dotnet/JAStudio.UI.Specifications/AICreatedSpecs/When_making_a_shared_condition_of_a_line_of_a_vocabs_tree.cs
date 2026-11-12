using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using static JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words.WordConditions;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The vocab だの, whose match-condition is <c>AllOf(!PrecedingToken.Surface.EndsWithOneOf("ん"), PrecedingToken.IsAGodanVerb)</c>, edited on its Matching<br/>
/// tab, and んだ, whose match-condition is <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c>. The first line of だの's tree is selected.
/// </summary>
public class When_making_a_shared_condition_of_a_line_of_a_vocabs_tree : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   static readonly Condition<CandidateWordVariant> FollowsASurfaceEndingWithん = PrecedingToken.Meets(TokenConditions.Surface.Meets(TextConditions.EndsWithOneOf("ん")));

   readonly VocabNote _んだ;
   readonly VocabEditorSectionViewModel _editor;
   readonly NewSharedConditionDialogViewModel _dialog;

   public When_making_a_shared_condition_of_a_line_of_a_vocabs_tree()
   {
      var だの = CreateVocab("だの", "and so on", "だの");
      だの.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(!FollowsASurfaceEndingWithん, PrecedingToken.Meets(TokenConditions.IsAGodanVerb)));
      _んだ = CreateVocab("んだ", "it is that", "んだ");
      _んだ.MatchingConfiguration.SetMatchCondition(ConditionGroup.AllOf(FollowsASurfaceEndingWithん));
      _editor = Edit<VocabEditorSectionViewModel>(だの);
      Tree.SelectedLine = Tree.Lines[0].Children[0];
      _dialog = Tree.DialogMakingASharedConditionOfTheSelectedLine() ?? throw new InvalidOperationException("The first line offers no condition to make a shared-condition of.");
   }

   ConditionTreeEditorViewModel Tree => _editor.MatchingConfig.MatchConditionEditor;

   List<string> Lines() => [.. Tree.Lines[0].ThisAndAllBelow().Select(it => it.Text)];

   SpecMenuItem TheMakeItemOfTheSelectedLinesMenu() =>
      SpecMenuItem.ItemPickedBy('m', Tree.SelectedLineMenu(() => {}, () => {}, () => {})) ?? throw new InvalidOperationException("The menu has no item picked by m.");

   [XF] public void the_selected_lines_menu_offers_to_make_a_shared_condition_of_it() => TheMakeItemOfTheSelectedLinesMenu().IsEnabled.Must().BeTrue();

   [XF] public void the_dialog_offers_the_condition_inside_the_negation_on_a_word_then_what_it_holds_on_a_token_and_on_a_text() =>
      _dialog.Definitions.Select(it => it.Text)
             .Must().SequenceEqual(["""On a word: PrecedingToken.Surface.EndsWithOneOf("ん")""", """On a token: Surface.EndsWithOneOf("ん")""", """On a text: EndsWithOneOf("ん")"""]);

   [XF] public void the_dialog_chooses_the_outermost_to_start_with_with_the_subject_in_front_of_the_name() => _dialog.SubjectInFront.Must().Be("Word.");

   [XF] public void the_dialog_asks_for_a_name_before_it_can_make_one() => new { _dialog.CanMake, _dialog.WhyItCannotBeMade }.Must().Satisfy(it => !it.CanMake && it.WhyItCannotBeMade == "Type its name.");

   public class with_the_line_of_the_fact_on_the_preceding_token_selected : When_making_a_shared_condition_of_a_line_of_a_vocabs_tree
   {
      public with_the_line_of_the_fact_on_the_preceding_token_selected() => Tree.SelectedLine = Tree.Lines[0].Children[1];

      [XF] public void the_menu_does_not_offer_to_make_one_since_it_is_a_name_already() => TheMakeItemOfTheSelectedLinesMenu().IsEnabled.Must().BeFalse();
   }

   public class after_making_Token_EndsInN_of_what_it_holds_on_a_token : When_making_a_shared_condition_of_a_line_of_a_vocabs_tree
   {
      public after_making_Token_EndsInN_of_what_it_holds_on_a_token() =>
         Tree.MakeASharedConditionOfTheSelectedLineThroughADialog(dialog =>
         {
            dialog.Chosen = dialog.Definitions[1];
            dialog.TypedName = "EndsInN";
            return Task.FromResult<NewSharedCondition?>(dialog.ToMake);
         });

      [XF] public void the_tree_shows_it_used_inside_the_negation() =>
         Lines().Must().SequenceEqual(["// A word matches the vocab when each of these holds:", "!PrecedingToken.EndsInN", "PrecedingToken.IsAGodanVerb"]);

      [XF] public void the_line_stays_selected() => Tree.SelectedLine?.Text.Must().Be("!PrecedingToken.EndsInN");

      [XF] public void the_other_vocab_holding_its_definition_uses_it_too() => _んだ.MatchingConfiguration.MatchCondition.ToString().Must().Be("AllOf(PrecedingToken.EndsInN)");
   }
}
