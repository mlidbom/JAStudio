using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
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

   /// <summary>The dialog that renames the shared-condition.</summary>
   internal SharedConditionRenameDialogViewModel RenameDialog() => new(SharedCondition, _services.NoteServices.SharedConditions);

   /// <summary>
   /// Renames the shared-condition <paramref name="newName"/> (<see cref="SharedConditions.Rename"/>): every vocab and definition using it then uses it by<br/>
   /// that name. The window shows it.
   /// </summary>
   internal void RenameTo(string newName)
   {
      _services.NoteServices.SharedConditions.Rename(SharedCondition, newName, _services.CoreApp.Collection.Vocab.All());
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
   IReadOnlyList<VocabNote> TheVocabDependingOnIt() => _services.CoreApp.Collection.Vocab.WhoseMatchConditionsDependOnTheDefinitionOf(SharedCondition);

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
