using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Grouping;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Navigation;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>The search tab's condition tree: which group a condition goes into, which line is selected after a change, and the notes found.</summary>
public class When_building_a_search_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string SentenceAboutHome = "家に帰る";
   readonly NoteSearchViewModel _search;

   protected When_building_a_search_in_the_workspace()
   {
      CreateTestSentence(SentenceAboutHome, "Go home").Tags.Set(Tag.FromName("source::anime"));
      CreateVocab("行く", "to go", "いく");
      _search = Workspace.Navigation.Search;
   }

   List<string> Lines() => [.._search.Nodes[0].ThisAndAllBelow().Select(it => it.Description)];

   string SelectedLine() => _search.SelectedNode?.Description ?? throw new InvalidOperationException("No line is selected.");

   List<string> QuestionsOfTheListedNotes()
   {
      SpinWait.SpinUntil(() => !_search.IsSearching, TimeSpan.FromSeconds(10)).Must().BeTrue();
      var results = _search.Results;
      return [..results.Vocab.Results.Select(it => it.Question), ..results.Kanji.Results.Select(it => it.Question), ..results.Sentences.Results.Select(it => it.Question)];
   }

   public class that_was_just_opened : When_building_a_search_in_the_workspace
   {
      [XF] public void the_tree_is_an_empty_all_of_group() => Lines().Must().SequenceEqual(["All of:"]);
      [XF] public void the_root_is_selected() => SelectedLine().Must().Be("All of:");

      public class when_removing_the_selected_root : that_was_just_opened
      {
         public when_removing_the_selected_root() => _search.RemoveTheSelectedNode();

         [XF] public void nothing_happens() => Lines().Must().SequenceEqual(["All of:"]);
      }
   }

   public class after_adding_a_tag_condition : When_building_a_search_in_the_workspace
   {
      public after_adding_a_tag_condition() => _search.AddCondition(NoteProperties.Tags.Include("source::anime"));

      [XF] public void it_is_in_the_root_group() => Lines().Must().SequenceEqual(["All of:", "Tags include source::anime"]);
      [XF] public void it_is_selected() => SelectedLine().Must().Be("Tags include source::anime");
      [XF] public void the_notes_it_finds_are_listed() => QuestionsOfTheListedNotes().Must().SequenceEqual([SentenceAboutHome]);

      public class and_adding_another_while_the_first_is_selected : after_adding_a_tag_condition
      {
         public and_adding_another_while_the_first_is_selected() => _search.AddCondition(NoteProperties.NoteType.Is<VocabNote>());

         [XF] public void it_goes_into_the_group_the_selected_condition_is_in() =>
            Lines().Must().SequenceEqual(["All of:", "Tags include source::anime", "Note type is VocabNote"]);

         public class and_removing_the_first : and_adding_another_while_the_first_is_selected
         {
            public and_removing_the_first()
            {
               _search.SelectedNode = _search.Nodes[0].Children[0];
               _search.RemoveTheSelectedNode();
            }

            [XF] public void the_condition_after_it_is_selected() => SelectedLine().Must().Be("Note type is VocabNote");
         }

         public class and_undoing : and_adding_another_while_the_first_is_selected
         {
            public and_undoing() => _search.Undo();

            [XF] public void the_second_condition_is_gone() => Lines().Must().SequenceEqual(["All of:", "Tags include source::anime"]);
         }
      }

      public class and_removing_it : after_adding_a_tag_condition
      {
         public and_removing_it() => _search.RemoveTheSelectedNode();

         [XF] public void the_root_is_selected_again() => SelectedLine().Must().Be("All of:");
         [XF] public void no_notes_are_listed_for_the_empty_search() => QuestionsOfTheListedNotes().Must().BeEmpty();
      }
   }

   public class when_typing_a_key_from_the_add_menu : When_building_a_search_in_the_workspace
   {
      SpecMenuItem? ItemFor(char key) => NoteSearchViewModel.ItemPickedBy(key, _search.AddConditionMenu(_ => {}));

      [XF] public void t_picks_the_tags_item() => ItemFor('t')?.Name.Must().Be("_t Tags");
      [XF] public void q_picks_the_questions_submenu() => ItemFor('q')?.Kind.Must().Be(SpecMenuItemKind.Submenu);
      [XF] public void the_case_of_the_key_is_ignored() => ItemFor('Q')?.Name.Must().Be("_q Question");
      [XF] public void a_key_no_item_has_picks_nothing() => (ItemFor('x') is null).Must().BeTrue();
   }

   /// <summary>A property with only a few values has a submenu of them in the add menu, which adds the condition at once, with no editor.</summary>
   public class when_picking_a_value_in_the_add_menu : When_building_a_search_in_the_workspace
   {
      bool _anEditorWasOpened;

      void PickInTheAddMenu(char propertyKey, char valueKey) =>
         ItemPickedBy(valueKey, ItemPickedBy(propertyKey, _search.AddConditionMenu(_ => _anEditorWasOpened = true)).Children).Action();

      static SpecMenuItem ItemPickedBy(char key, IEnumerable<SpecMenuItem> menu) =>
         NoteSearchViewModel.ItemPickedBy(key, menu) ?? throw new InvalidOperationException($"No item in the menu has the key {key}.");

      public class n_then_k : when_picking_a_value_in_the_add_menu
      {
         public n_then_k() => PickInTheAddMenu('n', 'k');

         [XF] public void adds_a_condition_that_the_note_is_a_kanji() => Lines().Must().SequenceEqual(["All of:", "Note type is KanjiNote"]);
         [XF] public void opens_no_editor() => _anEditorWasOpened.Must().BeFalse();
      }

      public class r_then_f : when_picking_a_value_in_the_add_menu
      {
         public r_then_f() => PickInTheAddMenu('r', 'f');

         [XF] public void adds_a_condition_that_studying_reading_is_false() => Lines().Must().SequenceEqual(["All of:", "Studying reading is false"]);
         [XF] public void opens_no_editor() => _anEditorWasOpened.Must().BeFalse();
      }

      public class l_then_t : when_picking_a_value_in_the_add_menu
      {
         public l_then_t() => PickInTheAddMenu('l', 't');

         [XF] public void adds_a_condition_that_studying_listening_is_true() => Lines().Must().SequenceEqual(["All of:", "Studying listening is true"]);
      }
   }

   public class after_adding_a_group_and_negating_it : When_building_a_search_in_the_workspace
   {
      public after_adding_a_group_and_negating_it()
      {
         _search.AddCondition(ConditionGroup.AnyOf());
         _search.ToggleNegationOfTheSelectedNode();
      }

      [XF] public void the_group_shows_the_negation_on_its_own_line() => Lines().Must().SequenceEqual(["All of:", "Not: Any of:"]);
      [XF] public void the_negated_group_stays_selected() => SelectedLine().Must().Be("Not: Any of:");

      public class and_adding_a_condition_while_it_is_selected : after_adding_a_group_and_negating_it
      {
         public and_adding_a_condition_while_it_is_selected() => _search.AddCondition(NoteProperties.Tags.Include("source::anime"));

         [XF] public void the_condition_goes_into_the_negated_group() =>
            _search.SelectedNode?.Position.Must().Be(ConditionPosition.Root.Child(0).Child(0).Child(0));

         [XF] public void the_notes_not_tagged_with_it_are_listed() => QuestionsOfTheListedNotes().Must().SequenceEqual(["行く"]);
      }
   }
}
