using System;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.UI.ViewModels.Workspace.Navigation;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// A shared-condition as the <see cref="SharedConditionsWindow"/> lists it: as it's defined, as code, such as <c>Word.FollowsAPastTenseStem =</c> then its<br/>
/// definition under it, and how many vocab's match-conditions depend on it, which it can open in the workspace.
/// </summary>
sealed partial class ListedSharedConditionViewModel
{
   readonly VocabCollection _vocab;
   readonly Action<NoteList> _showInTheWorkspace;
   readonly int _howManyVocabDependOnIt;

   /// <param name="sharedCondition">The shared-condition listed.</param>
   /// <param name="vocab">The collection's vocab, among which are those whose match-conditions depend on it.</param>
   /// <param name="showInTheWorkspace">Lists notes in the workspace's "List" tab, as the note menus' "Open" items do: the vocab depending on it.</param>
   public ListedSharedConditionViewModel(ISharedCondition sharedCondition, VocabCollection vocab, Action<NoteList> showInTheWorkspace)
   {
      _vocab = vocab;
      _showInTheWorkspace = showInTheWorkspace;
      SharedCondition = sharedCondition;
      Code = ConditionCode.AsText(ConditionCode.LinesOfASharedCondition(sharedCondition));
      _howManyVocabDependOnIt = vocab.WhoseMatchConditionsDependOnTheDefinitionOf(sharedCondition).Count;
   }

   /// <summary>The shared-condition listed.</summary>
   public ISharedCondition SharedCondition { get; }

   /// <summary>The shared-condition as it's defined, as code: its full name, then its definition under it.</summary>
   public string Code { get; }

   /// <summary>How many vocab's match-conditions depend on it, directly or through other shared-conditions.</summary>
   public string WhoDependsOnIt => $"The match-conditions of {_howManyVocabDependOnIt} vocab depend on it.";

   /// <summary>
   /// Lists the vocab whose match-conditions depend on it, directly or through other shared-conditions, as the collection holds them now, in the<br/>
   /// workspace's "List" tab (<see cref="NoteList.OfTheVocabDependingOn"/>). It can run when any vocab did as it was listed.
   /// </summary>
   [RelayCommand(CanExecute = nameof(AnyVocabDependsOnIt))]
   void OpenTheVocabDependingOnIt() => _showInTheWorkspace(NoteList.OfTheVocabDependingOn(SharedCondition, _vocab));

   bool AnyVocabDependsOnIt() => _howManyVocabDependOnIt > 0;
}
