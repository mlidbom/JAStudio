using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Threading;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.Workspace.Editors.Matching;

/// <summary>
/// A vocab's match-condition window, showing a <see cref="MatchConditionWindowViewModel"/>: its match-condition in the tree editor, with all the room a<br/>
/// window gives. A double-click on the match-condition on the vocab's Matching tab, or Ctrl+M in the workspace, opens it. Each vocab has one window at<br/>
/// most: opening it again brings it to the front. It shows the match-condition as it is now whenever it's activated, since the vocab's Matching tab or<br/>
/// the MCP server may have changed it, and closes then if the vocab was deleted.
/// </summary>
/// <remarks>
/// It closes when the collection is loaded anew, as after a sync, since the shared-conditions are then loaded anew too, and the tree would hold the ones<br/>
/// loaded before.
/// </remarks>
partial class MatchConditionWindow : Window
{
   /// <summary>The open windows, by the id of the vocab each is of.</summary>
   static readonly Dictionary<NoteId, MatchConditionWindow> OpenWindows = [];

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public MatchConditionWindow() => InitializeComponent();
#pragma warning restore CS8618

   MatchConditionWindow(VocabNote vocab, TemporaryServiceCollection services)
   {
      var viewModel = new MatchConditionWindowViewModel(services,
                                                        vocab,
                                                        sharedCondition => SharedConditionWindow.Open(sharedCondition, services),
                                                        WorkspaceWindow.ShowTheSentencesReparsedFor);
      DataContext = viewModel;
      InitializeComponent();
      Activated += (_, _) => ShowItAsItIsNowOrCloseIfItIsGone(viewModel);

      var uiThread = services.Resolve<IUIThreadDispatcher>();
      var closingOnTheNextReload = services.CoreApp.Collection.ReloadedFromBackend.Subscribe(_ => uiThread.PostToUIThread(Close));
      var vocabId = vocab.GetId();
      Closed += (_, _) =>
      {
         closingOnTheNextReload.Dispose();
         OpenWindows.Remove(vocabId);
      };
   }

   void ShowItAsItIsNowOrCloseIfItIsGone(MatchConditionWindowViewModel viewModel)
   {
      if(viewModel.IsGone)
         Dispatcher.UIThread.Post(Close);
      else
         viewModel.ShowItAsItIsNow();
   }

   /// <summary>Opens <paramref name="vocab"/>'s match-condition window where it was last, or brings it to the front when it's open already, and returns it.</summary>
   internal static MatchConditionWindow Open(VocabNote vocab, TemporaryServiceCollection services)
   {
      if(OpenWindows.TryGetValue(vocab.GetId(), out var open))
      {
         open.Activate();
         return open;
      }

      var window = new MatchConditionWindow(vocab, services).ShowWhereItWasLast(services);
      OpenWindows.Add(vocab.GetId(), window);
      return window;
   }
}
