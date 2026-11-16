using Avalonia.Controls;
using Avalonia.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.Views.Workspace.Navigation;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Workspace;

/// <summary>
/// A workspace's navigation pane (<see cref="NavigationPane"/>) in a window, as the workspace window shows it. The vocab 走る and the sentence 毎日走る,<br/>
/// which is a parsing case.
/// </summary>
/// <remarks>
/// The pane shows one of its tabs at a time, and the views of the others aren't in the window meanwhile, while their view models go on changing.
/// </remarks>
public class When_the_navigation_pane_is_shown_in_a_window : SpecificationOpeningWindows, IAIGeneratedSpec
{
   /// <summary>The CardServer doesn't run in these specifications. The workspace needs its address only to build the URLs of the note pages.</summary>
   const string AddressOfACardServerThatIsNotRunning = "http://localhost";

   readonly NavigationPaneViewModel _navigation;
   readonly Window _window;

   protected When_the_navigation_pane_is_shown_in_a_window()
   {
      CardServerUrl.BaseUrl = AddressOfACardServerThatIsNotRunning;
      CreateVocab("走る", "to-run", "はしる");
      CreateSentence("毎日走る").Edit(it => it.MarkAsAParsingCase());

      _navigation = OnTheUIThread(() => new WorkspaceViewModel(Services).Navigation);
      _window = Open(() =>
      {
         var window = new Window { Width = 800, Height = 900, Content = new NavigationPane { DataContext = _navigation } };
         window.Show();
         return window;
      });
      RunWhatThePaneHasPosted();
   }

   /// <summary>Runs what the pane's views have posted to the UI thread, such as putting the keyboard on a line: what fails there fails the specification.</summary>
   static void RunWhatThePaneHasPosted() => OnTheUIThread(() => Dispatcher.UIThread.RunJobs());

   void Show(NavigationPaneTab tab)
   {
      OnTheUIThread(() => _navigation.ShownTab = tab);
      RunWhatThePaneHasPosted();
   }

   /// <summary>As a workspace does that shows the state it was left in: its search, with another tab than the search shown.</summary>
   public class and_the_search_changes_while_the_tags_are_shown : When_the_navigation_pane_is_shown_in_a_window
   {
      public and_the_search_changes_while_the_tags_are_shown()
      {
         Show(NavigationPaneTab.TagManagement);
         OnTheUIThread(() => _navigation.Search.StartFrom(NoteProperties.Tags.Include("source::anime")));
         RunWhatThePaneHasPosted();
         WaitUntil(() => !_navigation.Search.IsSearching);
      }

      [XF] public void the_tags_stay_shown() => OnTheUIThread(() => _navigation.ShownTab).Must().Be(NavigationPaneTab.TagManagement);
      [XF] public void the_search_is_the_changed_one() => OnTheUIThread(() => _navigation.Search.CurrentSearch.IsEmpty).Must().BeFalse();

      public class and_the_search_is_then_shown : and_the_search_changes_while_the_tags_are_shown
      {
         public and_the_search_is_then_shown() => Show(NavigationPaneTab.Search);

         [XF] public void the_search_is_shown() => OnTheUIThread(() => _navigation.IsSearchShown).Must().BeTrue();
      }
   }

   /// <summary>Rendered, to look at. A view that doesn't load, such as one using a resource it doesn't define, throws when it's shown, so it fails here rather than in Anki.</summary>
   public class showing_the_parsing_cases : When_the_navigation_pane_is_shown_in_a_window
   {
      public showing_the_parsing_cases()
      {
         Show(NavigationPaneTab.ParsingCases);
         WaitUntil(() => !_navigation.ParsingCases.IsComparing);
         Render(_window, "NavigationPaneShowingTheParsingCases");
      }

      [XF] public void the_one_parsing_case_is_listed() => OnTheUIThread(() => _navigation.ParsingCases.StatusText).Must().Be("1 parsing case");
   }
}
