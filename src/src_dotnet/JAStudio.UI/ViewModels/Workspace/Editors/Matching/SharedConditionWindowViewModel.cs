using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// A shared-condition's window (<see cref="SharedConditionWindow"/>): its definition, edited in the tree editor, and the vocab whose match-conditions<br/>
/// depend on it (<see cref="Condition.DependsOnTheDefinitionOf"/>). Every change to the definition is saved in its file, and those vocab's sentences are<br/>
/// then reparsed, as a vocab's own are after a change on its Matching tab.
/// </summary>
partial class SharedConditionWindowViewModel : ObservableObject
{
   readonly TemporaryServiceCollection _services;
   readonly IUIThreadDispatcher _uiThread;

   /// <param name="services">The application's services.</param>
   /// <param name="sharedCondition">The shared-condition the window is of: one of the corpus's.</param>
   /// <param name="openTheDefinitionOf">Opens another shared-condition's window, such as one the definition uses.</param>
   public SharedConditionWindowViewModel(TemporaryServiceCollection services, ISharedCondition sharedCondition, Action<ISharedCondition> openTheDefinitionOf)
   {
      _services = services;
      _uiThread = services.Resolve<IUIThreadDispatcher>();
      SharedCondition = sharedCondition;
      Definition = new ConditionTreeEditorViewModel(services.Resolve<IFatalErrorHandler>(),
                                                    new EditedDefinition(sharedCondition, services.NoteServices.SharedConditions),
                                                    services.NoteServices,
                                                    ReparseTheSentencesOfTheVocabDependingOnIt,
                                                    openTheDefinitionOf);
      _vocabDependingOnIt = TheVocabDependingOnIt();
   }

   /// <summary>The shared-condition the window is of.</summary>
   public ISharedCondition SharedCondition { get; }

   /// <summary>The window's title: the shared-condition's full name, such as <c>Word.FollowsAPastTenseStem</c>.</summary>
   public string Title => $"{SharedCondition.FullName} - shared-condition";

   /// <summary>Opens the dialog that renames the shared-condition, and renames it the name it closes with. Set by the <see cref="SharedConditionWindow"/>.</summary>
   public IRelayCommand? RenameCommand { get; set; }

   /// <summary>Asks whether to delete the shared-condition, and deletes it and closes the window if so. Set by the <see cref="SharedConditionWindow"/>.</summary>
   public IRelayCommand? DeleteCommand { get; set; }

   /// <summary>What deleting the shared-condition does, as the dialog confirming it says: its definition is written out wherever it's used.</summary>
   internal string WhatDeletingItDoes()
   {
      var vocabUsingIt = Vocab.All().Count(it => it.MatchingConfiguration.MatchCondition.Uses(SharedCondition));
      var definitionsUsingIt = SharedConditions.All.Count(it => it.Definition.Uses(SharedCondition));
      return $"Delete {SharedCondition.FullName}? Its definition is written out in its place wherever it's used: in the match-conditions of {vocabUsingIt} vocab, "
           + $"and the definitions of {definitionsUsingIt} other shared-conditions. None of them then means anything else.";
   }

   /// <summary>Deletes the shared-condition, with its definition written out wherever it's used (<see cref="SharedConditions.Delete"/>).</summary>
   internal void Delete() => SharedConditions.Delete(SharedCondition, Vocab.All());

   /// <summary>
   /// Shows the shared-condition as it is now, when something else may have changed it or what uses it, such as a rename through the MCP server, or a<br/>
   /// shared-condition its definition uses made or deleted in another window: its title, the vocab depending on it, and its definition.
   /// </summary>
   internal void ShowItAsItIsNow()
   {
      OnPropertyChanged(nameof(Title));
      VocabDependingOnIt = TheVocabDependingOnIt();
      Definition.Refresh();
   }

   /// <summary>The dialog that renames the shared-condition.</summary>
   internal SharedConditionRenameDialogViewModel RenameDialog() => new(SharedCondition, SharedConditions);

   /// <summary>
   /// Renames the shared-condition <paramref name="newName"/> (<see cref="SharedConditions.Rename"/>): every vocab and definition using it then uses it by<br/>
   /// that name. The window shows it.
   /// </summary>
   internal void RenameTo(string newName)
   {
      SharedConditions.Rename(SharedCondition, newName, Vocab.All());
      OnPropertyChanged(nameof(Title));
      Definition.Refresh();
   }

   /// <summary>The shared-condition's definition, edited in the tree editor.</summary>
   public ConditionTreeEditorViewModel Definition { get; }

   /// <summary>The vocab whose match-conditions depend on the definition: those whose sentences a change to it reparses.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(WhoUsesIt))]
   IReadOnlyList<VocabNote> _vocabDependingOnIt;

   /// <summary>Whether the sentences of the vocab depending on it are being reparsed after a change. The definition can't be changed meanwhile.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(WhoUsesIt))]
   bool _isReparsingSentences;

   /// <summary>How many vocab's match-conditions depend on the definition, which a change to it affects, or that their sentences are being reparsed.</summary>
   public string WhoUsesIt => IsReparsingSentences
                                 ? $"Reparsing the sentences of {VocabDependingOnIt.Count} vocab…"
                                 : $"The match-conditions of {VocabDependingOnIt.Count} vocab depend on it: a change to its definition reparses their sentences.";

   /// <summary>The vocab whose match-conditions depend on the definition, as the collection holds them now.</summary>
   IReadOnlyList<VocabNote> TheVocabDependingOnIt() => Vocab.WhoseMatchConditionsDependOnTheDefinitionOf(SharedCondition);

   SharedConditions SharedConditions => _services.NoteServices.SharedConditions;

   VocabCollection Vocab => _services.CoreApp.Collection.Vocab;

   /// <summary>Reparses the sentences of the vocab depending on the definition, in the background, after it changed.</summary>
   void ReparseTheSentencesOfTheVocabDependingOnIt()
   {
      VocabDependingOnIt = TheVocabDependingOnIt();
      IsReparsingSentences = true;
      var vocab = VocabDependingOnIt;
      _services.BackgroundTaskManager.Run(() =>
      {
         try
         {
            _services.LocalNoteUpdater.ReparseSentencesForVocabWithoutProgressDialog(vocab);
         }
         finally
         {
            _uiThread.PostToUIThread(() => IsReparsingSentences = false);
         }
      });
   }
}
