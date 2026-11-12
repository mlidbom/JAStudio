using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Compze.Internals.SystemCE.ReactiveCE;
using JAStudio.Core;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels.Workspace.Editors.Matching;
using JAStudio.UI.WindowPlaces;

namespace JAStudio.UI.Views.Workspace.Editors.Matching;

/// <summary>
/// A shared-condition's window, showing a <see cref="SharedConditionWindowViewModel"/>: its definition in the tree editor, with Rename and Delete. F12 or<br/>
/// Ctrl+click on a shared-condition in any tree opens it. Each shared-condition has one window at most: opening it again brings it to the front. It<br/>
/// shows the shared-condition as it is now whenever it's activated, since another window or the MCP server may have changed it or what it uses.
/// </summary>
/// <remarks>
/// It closes when the collection is loaded anew, as after a sync, since the shared-conditions are then loaded anew too, and it would edit the one<br/>
/// loaded before.
/// </remarks>
partial class SharedConditionWindow : Window
{
   /// <summary>The open windows, by the shared-condition each is of: the object itself, since a rename changes what it equals.</summary>
   static readonly Dictionary<ISharedCondition, SharedConditionWindow> OpenWindows = new(ReferenceEqualityComparer.Instance);

#pragma warning disable CS8618 // For the XAML designer, which needs no view model.
   [Obsolete("For the XAML designer/previewer only")]
   public SharedConditionWindow() => InitializeComponent();
#pragma warning restore CS8618

   SharedConditionWindow(ISharedCondition sharedCondition, TemporaryServiceCollection services)
   {
      var viewModel = new SharedConditionWindowViewModel(services, sharedCondition, it => Open(it, services));
      //bug: Rename and Delete can be run while the sentences of the vocab depending on it are reparsed after a change to its definition: only the tree
      //   is disabled meanwhile (SentencesReparser.IsReparsingSentences). Delete then sets, and saves, the match-conditions of the vocab using it on the
      //   UI thread while the reparse reads those vocab, which VocabSentencesReparserViewModel says nothing may do. Their commands could be disabled while
      //   it runs, as the tree is.
      viewModel.RenameCommand = new AsyncRelayCommand(() => RenameAsync(viewModel));
      viewModel.DeleteCommand = new AsyncRelayCommand(() => DeleteAsync(viewModel));
      DataContext = viewModel;
      InitializeComponent();
      Activated += (_, _) => viewModel.ShowItAsItIsNow();

      var uiThread = services.Resolve<IUIThreadDispatcher>();
      var closingOnTheNextReload = services.CoreApp.Collection.ReloadedFromBackend.Subscribe(_ => uiThread.PostToUIThread(Close));
      Closed += (_, _) =>
      {
         closingOnTheNextReload.Dispose();
         OpenWindows.Remove(sharedCondition);
      };
   }

   async Task RenameAsync(SharedConditionWindowViewModel viewModel)
   {
      if(await SharedConditionRenameDialog.ShowAsync(this, viewModel.RenameDialog(), Position + new PixelPoint(40, 40)) is {} newName)
         viewModel.RenameTo(newName);
   }

   async Task DeleteAsync(SharedConditionWindowViewModel viewModel)
   {
      if(!await ConfirmDialog.ShowOverAsync(this, $"Delete {viewModel.SharedCondition.FullName}", viewModel.WhatDeletingItDoes())) return;

      viewModel.Delete();
      Close();
   }

   /// <summary>Opens <paramref name="sharedCondition"/>'s window where it was last, or brings it to the front when it's open already, and returns it.</summary>
   internal static SharedConditionWindow Open(ISharedCondition sharedCondition, TemporaryServiceCollection services)
   {
      if(OpenWindows.TryGetValue(sharedCondition, out var open))
      {
         open.Activate();
         return open;
      }

      var window = new SharedConditionWindow(sharedCondition, services).ShowWhereItWasLast(services);
      OpenWindows.Add(sharedCondition, window);
      return window;
   }
}
