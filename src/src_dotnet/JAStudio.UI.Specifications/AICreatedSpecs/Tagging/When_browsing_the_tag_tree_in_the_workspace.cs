using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// The navigation pane's Tags tab: the tag tree's lines, expanded and filtered, and searching for a tag, which replaces the search.<br/>
/// Two sentences are tagged <c>source::anime::natsume::s3::01</c> and <c>…::02</c>, and a vocab <c>source::wani</c>.<br/>
/// The search is "Note type is VocabNote" when the tab is shown.
/// </summary>
public class When_browsing_the_tag_tree_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string Natsume = "source::anime::natsume";

   readonly NavigationPaneViewModel _navigation;
   readonly NoteSearchViewModel _search;
   readonly TagManagementViewModel _tags;
   readonly VocabNote _vocab;

   public When_browsing_the_tag_tree_in_the_workspace()
   {
      CreateTestSentence("家に帰る", "Go home").Tags.Set(Tag.FromName("source::anime::natsume::s3::01"));
      CreateTestSentence("学校に行く", "Go to school").Tags.Set(Tag.FromName("source::anime::natsume::s3::02"));
      _vocab = CreateVocab("行く", "to go", "いく");
      _vocab.Tags.Set(Tag.FromName("source::wani"));

      _navigation = Workspace.Navigation;
      _search = _navigation.Search;
      _tags = _navigation.TagManagement;
      _search.AddCondition(NoteProperties.NoteType.Is<VocabNote>());
      WaitForTheSearch();
      ShowTheTagsTab();
   }

   /// <summary>As clicking the tab does.</summary>
   void ShowTheTagsTab() => _navigation.IsTagManagementShown = true;

   /// <summary>The specifications have no UI thread, so a search ends on a thread of its own. See <c>When_building_a_search_in_the_workspace</c>.</summary>
   void WaitForTheSearch() => SpinWait.SpinUntil(() => !_search.IsSearching, TimeSpan.FromSeconds(10)).Must().BeTrue();

   IEnumerable<TagTreeNodeViewModel> AllLines() => _tags.TopLevelLines.SelectMany(it => it.ThisAndAllBelow());

   TagTreeNodeViewModel LineOf(string tagPath) => AllLines().SingleOrDefault(it => it.TagPath == tagPath) ?? throw new InvalidOperationException($"No line shows {tagPath}.");

   /// <summary>The paths of the lines a person sees: the top-level lines, and the lines below each expanded line.</summary>
   List<string> PathsOfTheVisibleLines() => [.._tags.TopLevelLines.SelectMany(VisibleFrom).Select(it => it.TagPath)];

   static IEnumerable<TagTreeNodeViewModel> VisibleFrom(TagTreeNodeViewModel line) => [line, ..line.IsExpanded ? line.Children.SelectMany(VisibleFrom) : []];

   string SelectedPath() => _tags.SelectedLine?.TagPath ?? throw new InvalidOperationException("No line is selected.");

   List<string> SearchLines() => [.._search.Nodes[0].ThisAndAllBelow().Select(it => it.Description)];

   [XF] public void only_the_top_level_is_visible() => PathsOfTheVisibleLines().Must().SequenceEqual(["source"]);
   [XF] public void the_top_level_shows_how_many_notes_are_in_its_branch() => LineOf("source").NumberOfNotesInTheBranch.Must().Be(3);

   public class after_expanding_the_top_level_and_typing_a_filter : When_browsing_the_tag_tree_in_the_workspace
   {
      public after_expanding_the_top_level_and_typing_a_filter()
      {
         LineOf("source").IsExpanded = true;
         _tags.Filter = "nat 02";
      }

      [XF] public void only_the_matching_path_and_its_parents_are_left_all_expanded() =>
         PathsOfTheVisibleLines().Must().SequenceEqual(["source", "source::anime", Natsume, $"{Natsume}::s3", $"{Natsume}::s3::02"]);

      public class and_clearing_the_filter : after_expanding_the_top_level_and_typing_a_filter
      {
         public and_clearing_the_filter() => _tags.Filter = "";

         [XF] public void the_lines_are_expanded_as_before_the_filter() => PathsOfTheVisibleLines().Must().SequenceEqual(["source", "source::anime", "source::wani"]);
      }

      public class and_searching_for_a_selected_parent_of_the_match : after_expanding_the_top_level_and_typing_a_filter
      {
         public and_searching_for_a_selected_parent_of_the_match()
         {
            _tags.SelectedLine = LineOf(Natsume);
            _tags.SearchForTheSelectedTag();
            WaitForTheSearch();
         }

         [XF] public void the_search_tab_is_shown() => _navigation.ShownTab.Must().Be(NavigationPaneTab.Search);
         [XF] public void the_whole_search_is_replaced_by_the_tag() => SearchLines().Must().SequenceEqual(["All of:", $"Tags include {Natsume}"]);
         [XF] public void the_notes_in_the_branch_are_found() => _search.Results.Sentences.Results.Select(it => it.Question).Order(StringComparer.Ordinal).Must().SequenceEqual(["学校に行く", "家に帰る"]);

         public class and_undoing_in_the_search : and_searching_for_a_selected_parent_of_the_match
         {
            public and_undoing_in_the_search()
            {
               _search.Undo();
               WaitForTheSearch();
            }

            [XF] public void the_search_before_is_back() => SearchLines().Must().SequenceEqual(["All of:", "Note type is VocabNote"]);
         }
      }
   }

   public class after_selecting_a_line_and_tagging_a_note_while_the_search_tab_is_shown : When_browsing_the_tag_tree_in_the_workspace
   {
      public after_selecting_a_line_and_tagging_a_note_while_the_search_tab_is_shown()
      {
         LineOf("source").IsExpanded = true;
         _tags.SelectedLine = LineOf("source::wani");
         _navigation.ShowSearch();
         _vocab.Tags.Set(Tag.FromName("source::anime::natsume::s4::01"));
      }

      public class and_showing_the_tags_tab_again : after_selecting_a_line_and_tagging_a_note_while_the_search_tab_is_shown
      {
         public and_showing_the_tags_tab_again() => ShowTheTagsTab();

         [XF] public void the_tree_has_the_new_tag() => LineOf($"{Natsume}::s4::01").NumberOfNotesInTheBranch.Must().Be(1);
         [XF] public void the_counts_include_the_note() => LineOf(Natsume).NumberOfNotesInTheBranch.Must().Be(3);
         [XF] public void the_same_path_is_selected() => SelectedPath().Must().Be("source::wani");
         [XF] public void the_lines_are_expanded_as_before() => PathsOfTheVisibleLines().Must().SequenceEqual(["source", "source::anime", "source::wani"]);
      }
   }
}
