using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// A condition's code shown read-only (<see cref="ConditionCodeView"/>), laid out as it's read (<see cref="CodeLayout.ForReading"/>), such as a vocab's<br/>
/// match-condition on its Matching tab: in pieces each shown as its kind should be, a shared-condition's name showing its definition on hover, and<br/>
/// opening its window on Ctrl+click.
/// </summary>
partial class ConditionCodeViewModel : ObservableObject
{
   readonly Action<ISharedCondition> _openTheDefinitionOf;

   /// <param name="lines">The lines of the condition's code.</param>
   /// <param name="openTheDefinitionOf">Opens a shared-condition's window, where its definition is edited.</param>
   public ConditionCodeViewModel(IReadOnlyList<CodeLine> lines, Action<ISharedCondition> openTheDefinitionOf)
   {
      _openTheDefinitionOf = openTheDefinitionOf;
      Show(lines);
   }

   /// <summary>The code's lines as they're shown.</summary>
   [ObservableProperty] IReadOnlyList<CodeLineViewModel> _lines = [];

   /// <summary>Shows <paramref name="lines"/>, the lines of the condition's code as it is now, in place of those shown.</summary>
   public void Show(IReadOnlyList<CodeLine> lines) => Lines = CodeLineViewModel.LinesOf(lines, CodeLayout.ForReading);

   /// <summary>Opens <paramref name="sharedCondition"/>'s window, where its definition is edited, as Ctrl+click on its name does.</summary>
   public void OpenTheDefinitionOf(ISharedCondition sharedCondition) => _openTheDefinitionOf(sharedCondition);
}
