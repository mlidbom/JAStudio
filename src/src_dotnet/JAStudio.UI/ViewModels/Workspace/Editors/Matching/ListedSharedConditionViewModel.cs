using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// A shared-condition as the <see cref="SharedConditionsWindow"/> lists it: as it's defined, as code, such as <c>Word.FollowsAPastTenseStem =</c> then its<br/>
/// definition under it, and how many vocab's match-conditions depend on it.
/// </summary>
sealed class ListedSharedConditionViewModel
{
   /// <param name="sharedCondition">The shared-condition listed.</param>
   /// <param name="vocab">The collection's vocab, to count those whose match-conditions depend on it.</param>
   public ListedSharedConditionViewModel(ISharedCondition sharedCondition, VocabCollection vocab)
   {
      SharedCondition = sharedCondition;
      Code = ConditionCode.AsText(ConditionCode.LinesOfASharedCondition(sharedCondition));
      WhoDependsOnIt = $"The match-conditions of {vocab.WhoseMatchConditionsDependOnTheDefinitionOf(sharedCondition).Count} vocab depend on it.";
   }

   /// <summary>The shared-condition listed.</summary>
   public ISharedCondition SharedCondition { get; }

   /// <summary>The shared-condition as it's defined, as code: its full name, then its definition under it.</summary>
   public string Code { get; }

   /// <summary>How many vocab's match-conditions depend on it, directly or through other shared-conditions.</summary>
   public string WhoDependsOnIt { get; }
}
