using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.Grouping;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.Navigation;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The workspace is left with two sentences in tabs, the second one shown, a search for <c>source::anime</c>, a list of both sentences<br/>
/// on the List tab, which is shown, the editor section open, and both panes widened. Opening it again after a restart is a new workspace,<br/>
/// created before the notes are loaded, as at Anki's start.
/// </summary>
public class When_the_workspace_is_left : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string SentenceAboutHome = "家に帰る";
   const string SentenceAboutSchool = "学校に行く";

   readonly SentenceNote _sentenceAboutHome;

   public When_the_workspace_is_left()
   {
      _sentenceAboutHome = CreateTaggedSentence(SentenceAboutHome);
      var sentenceAboutSchool = CreateTaggedSentence(SentenceAboutSchool);
      Workspace.OpenNote(_sentenceAboutHome);
      Workspace.OpenNote(sentenceAboutSchool);
      Workspace.Navigation.Search.AddCondition(NoteProperties.Tags.Include("source::anime"));
      Workspace.Navigation.ShowNoteList(new NoteList("Sentences about going", [_sentenceAboutHome, sentenceAboutSchool]));
      Workspace.IsEditorSectionOpen = true;
      Workspace.EditorSectionWidth = 600;
      Workspace.Navigation.Width = 350;
      Workspace.SaveTheState();
   }

   SentenceNote CreateTaggedSentence(string question)
   {
      var sentence = CreateTestSentence(question, "an answer");
      sentence.Tags.Set(Tag.FromName("source::anime"));
      return sentence;
   }

   /// <summary>A new workspace, as after a restart, and then the notes loaded, as when Anki opens its profile.</summary>
   private protected WorkspaceViewModel OpenedAgainAfterARestart()
   {
      var openedAgain = new WorkspaceViewModel(GetService<TemporaryServiceCollection>());
      GetService<JPCollection>().ReloadFromBackend();
      return openedAgain;
   }

   public class and_opened_again_after_a_restart : When_the_workspace_is_left
   {
      readonly WorkspaceViewModel _openedAgain;

      public and_opened_again_after_a_restart() => _openedAgain = OpenedAgainAfterARestart();

      [XF] public void the_tabs_show_the_same_notes() => _openedAgain.Tabs.Select(it => it.Header).Must().SequenceEqual([SentenceAboutHome, SentenceAboutSchool]);
      [XF] public void the_tab_that_was_shown_is_shown() => (_openedAgain.SelectedTab?.Header ?? "No tab is shown").Must().Be(SentenceAboutSchool);
      [XF] public void the_search_is_the_same() => _openedAgain.Navigation.Search.CurrentSearch.Must().Be(ConditionGroup.AllOf(NoteProperties.Tags.Include("source::anime")));
      [XF] public void the_List_tab_lists_the_same_notes() => _openedAgain.Navigation.NoteList.Title.Must().Be("Sentences about going (2)");
      [XF] public void the_List_tab_is_shown() => _openedAgain.Navigation.ShownTab.Must().Be(NavigationPaneTab.NoteList);
      [XF] public void the_editor_section_is_open() => _openedAgain.IsEditorSectionOpen.Must().BeTrue();
      [XF] public void the_editor_section_is_as_wide() => _openedAgain.EditorSectionWidth.Must().Be(600.0);
      [XF] public void the_navigation_pane_is_as_wide() => _openedAgain.Navigation.Width.Must().Be(350.0);
   }

   public class and_a_note_in_a_tab_is_deleted_before_it_is_opened_again_after_a_restart : When_the_workspace_is_left
   {
      readonly WorkspaceViewModel _openedAgain;

      public and_a_note_in_a_tab_is_deleted_before_it_is_opened_again_after_a_restart()
      {
         GetService<JPCollection>().Delete(_sentenceAboutHome, ItsAnkiNote.IsDeleted);
         _openedAgain = OpenedAgainAfterARestart();
      }

      [XF] public void only_the_tab_of_the_note_that_is_left_comes_back() => _openedAgain.Tabs.Select(it => it.Header).Must().SequenceEqual([SentenceAboutSchool]);
      [XF] public void the_List_tab_lists_only_the_note_that_is_left() => _openedAgain.Navigation.NoteList.Title.Must().Be("Sentences about going (1)");
   }
}
