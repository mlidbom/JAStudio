using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// The window listing every one of the corpus's shared-conditions (<see cref="SharedConditionsWindow"/>), as the workspace's main menu opens it: each as<br/>
/// it's defined, with how many vocab's match-conditions depend on it, which each can open in the workspace, those on a word first, then a token's, then<br/>
/// a text's. Opening one opens its own window (<see cref="SharedConditionWindow"/>), where it's edited, renamed or deleted.
/// </summary>
partial class SharedConditionsWindowViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;
   readonly Action<NoteList> _showInTheWorkspace;

   /// <param name="services">The application's services.</param>
   /// <param name="showInTheWorkspace">Lists notes in the workspace's "List" tab, as the note menus' "Open" items do: the vocab depending on one.</param>
   public SharedConditionsWindowViewModel(TemporaryServiceCollection services, Action<NoteList> showInTheWorkspace)
   {
      _services = services;
      _showInTheWorkspace = showInTheWorkspace;
      ShowThemAsTheyAreNow();
   }

   /// <summary>The shared-conditions, as they're listed (<see cref="Core.Note.Vocabulary.Matching.Conditions.Sharing.SharedConditions.All"/>).</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(HowManyThereAre))]
   IReadOnlyList<ListedSharedConditionViewModel> _sharedConditions = [];

   /// <summary>The selected shared-condition, which Enter opens.</summary>
   [ObservableProperty] ListedSharedConditionViewModel? _selected;

   /// <summary>How many shared-conditions there are, and how to open one.</summary>
   public string HowManyThereAre => $"{SharedConditions.Count} shared-conditions. Enter or a double-click opens one, to edit, rename or delete it.";

   /// <summary>
   /// Lists the shared-conditions as they are now, since any may have been made, changed, renamed or deleted elsewhere, as in their windows: the one<br/>
   /// selected stays selected, if it's still one of them.
   /// </summary>
   internal void ShowThemAsTheyAreNow()
   {
      var selected = Selected?.SharedCondition;
      var vocab = _services.CoreApp.Collection.Vocab;
      SharedConditions = [.. _services.NoteServices.SharedConditions.All.Select(it => new ListedSharedConditionViewModel(it, vocab, _showInTheWorkspace))];
      Selected = SharedConditions.FirstOrDefault(it => ReferenceEquals(it.SharedCondition, selected)) ?? SharedConditions.FirstOrDefault();
   }
}
