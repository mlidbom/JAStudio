using System;
using System.Collections.Generic;
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

   /// <param name="services">The application's services.</param>
   /// <param name="sharedCondition">The shared-condition the window is of: one of the corpus's.</param>
   /// <param name="openTheDefinitionOf">Opens another shared-condition's window, such as one the definition uses.</param>
   public SharedConditionWindowViewModel(TemporaryServiceCollection services, ISharedCondition sharedCondition, Action<ISharedCondition> openTheDefinitionOf)
   {
      _services = services;
      SharedCondition = sharedCondition;
      _vocabDependingOnIt = TheVocabDependingOnIt();
      SentencesReparser = new VocabSentencesReparserViewModel(services, () => VocabDependingOnIt, sentencesReparsed: () => {});
      SentencesReparser.PropertyChanged += (_, changed) =>
      {
         if(changed.PropertyName != nameof(SentencesReparser.IsReparsingSentences)) return;

         OnPropertyChanged(nameof(WhoDependsOnIt));
         RenameCommand?.NotifyCanExecuteChanged();
         DeleteCommand?.NotifyCanExecuteChanged();
      };
      //review:matching: DefinitionEditor was Definition, but it's the definition's tree editor, as a vocab's Matching tab has its MatchConditionEditor.
      DefinitionEditor = new ConditionTreeEditorViewModel(services.Resolve<IFatalErrorHandler>(),
                                                          new EditedDefinition(sharedCondition, services.NoteServices.SharedConditions),
                                                          services.NoteServices,
                                                          ReparseTheSentencesOfTheVocabDependingOnIt,
                                                          openTheDefinitionOf);
   }

   /// <summary>The shared-condition the window is of.</summary>
   public ISharedCondition SharedCondition { get; }

   /// <summary>The window's title: the shared-condition's full name, such as <c>Word.FollowsAPastTenseStem</c>.</summary>
   public string Title => $"{SharedCondition.FullName} - shared-condition";

   /// <summary>
   /// Opens the dialog that renames the shared-condition, and renames it the name it closes with. Set by the <see cref="SharedConditionWindow"/>, it can run<br/>
   /// only when <see cref="CanBeRenamedOrDeleted"/>.
   /// </summary>
   public IRelayCommand? RenameCommand { get; set; }

   /// <summary>
   /// Asks whether to delete the shared-condition, and deletes it and closes the window if so. Set by the <see cref="SharedConditionWindow"/>, it can run<br/>
   /// only when <see cref="CanBeRenamedOrDeleted"/>.
   /// </summary>
   public IRelayCommand? DeleteCommand { get; set; }

   /// <summary>
   /// Whether the shared-condition can be renamed or deleted now: not while the sentences of the vocab depending on it are reparsed, which reads those<br/>
   /// vocab, as renaming and deleting change and save them.
   /// </summary>
   public bool CanBeRenamedOrDeleted => !SentencesReparser.IsReparsingSentences;

   /// <summary>
   /// Whether the shared-condition is no longer one of the corpus's (<see cref="SharedConditions.Holds"/>): deleted elsewhere, as through the MCP server.<br/>
   /// The window then closes, since every change to it would fail.
   /// </summary>
   public bool IsGone => !SharedConditions.Holds(SharedCondition);

   /// <summary>What deleting the shared-condition does, as the dialog confirming it says: its definition is written out wherever it's used.</summary>
   internal string WhatDeletingItDoes() =>
      $"Delete {SharedCondition.FullName}? Its definition is written out in its place wherever it's used: in the match-conditions of "
    + $"{Vocab.WhoseMatchConditionsUse(SharedCondition).Count} vocab, and the definitions of {SharedConditions.WhoseDefinitionsUse(SharedCondition).Count} other "
    + "shared-conditions. None of them then means anything else.";

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
      DefinitionEditor.Refresh();
   }

   /// <summary>The dialog that renames the shared-condition.</summary>
   internal SharedConditionRenameDialogViewModel RenameDialog() => new(SharedCondition, SharedConditions);

   /// <summary>
   /// Renames the shared-condition <paramref name="newName"/> (<see cref="SharedConditions.Rename"/>): every vocab and definition using it then uses it by<br/>
   /// that name. The window shows it as it is now.
   /// </summary>
   internal void RenameTo(string newName)
   {
      SharedConditions.Rename(SharedCondition, newName, Vocab.All());
      ShowItAsItIsNow();
   }

   /// <summary>The tree editor of the shared-condition's definition.</summary>
   public ConditionTreeEditorViewModel DefinitionEditor { get; }

   /// <summary>Reparses the sentences of the vocab depending on the definition after a change to it. The definition can't be changed while it runs.</summary>
   public VocabSentencesReparserViewModel SentencesReparser { get; }

   /// <summary>The vocab whose match-conditions depend on the definition: those whose sentences a change to it reparses.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(WhoDependsOnIt))]
   IReadOnlyList<VocabNote> _vocabDependingOnIt;

   /// <summary>How many vocab's match-conditions depend on the definition, which a change to it affects, or that their sentences are being reparsed.</summary>
   public string WhoDependsOnIt =>
      //review:matching: WhoDependsOnIt was WhoUsesIt, but it counts the vocab depending on it, those that use it through other shared-conditions too,
      //   as its text says.
      SentencesReparser.IsReparsingSentences
         ? $"Reparsing the sentences of {VocabDependingOnIt.Count} vocab…"
         : $"The match-conditions of {VocabDependingOnIt.Count} vocab depend on it: a change to its definition reparses their sentences.";

   /// <summary>The vocab whose match-conditions depend on the definition, as the collection holds them now.</summary>
   IReadOnlyList<VocabNote> TheVocabDependingOnIt() => Vocab.WhoseMatchConditionsDependOnTheDefinitionOf(SharedCondition);

   SharedConditions SharedConditions => _services.NoteServices.SharedConditions;

   VocabCollection Vocab => _services.CoreApp.Collection.Vocab;

   /// <summary>Reparses the sentences of the vocab depending on the definition, as it is after it changed, in the background.</summary>
   void ReparseTheSentencesOfTheVocabDependingOnIt()
   {
      VocabDependingOnIt = TheVocabDependingOnIt();
      SentencesReparser.ReparseSentences();
   }
}
