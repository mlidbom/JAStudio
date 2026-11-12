using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.UI.Views.Workspace.Editors.Matching;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// The small modal dialog that renames a shared-condition (<see cref="SharedConditionRenameDialog"/>): its new name, typed after its subject's, and why it<br/>
/// can't be used, if it can't. Every vocab and definition using it then uses it by the new name.
/// </summary>
sealed partial class SharedConditionRenameDialogViewModel : ObservableObject
{
   readonly ISharedCondition _sharedCondition;
   readonly SharedConditions _sharedConditions;

   /// <param name="sharedCondition">The shared-condition to rename: one of <paramref name="sharedConditions"/>.</param>
   /// <param name="sharedConditions">The corpus's shared-conditions, whose names it can't have.</param>
   public SharedConditionRenameDialogViewModel(ISharedCondition sharedCondition, SharedConditions sharedConditions)
   {
      _sharedCondition = sharedCondition;
      _sharedConditions = sharedConditions;
      _typedName = sharedCondition.Name;
   }

   public string Title => $"Rename {_sharedCondition.FullName}";

   /// <summary>Its subject's name as code names it, shown in front of the typed name, such as <c>Word.</c>.</summary>
   public string SubjectInFront => $"{_sharedCondition.Level.SubjectNameInCode}.";

   /// <summary>Its new name, as typed, without its subject's in front: its name to start with.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(WhyItCannotBeRenamed), nameof(CanRename))]
   string _typedName;

   /// <summary>What renaming it does besides.</summary>
   public string WhatItDoes => "Every vocab and shared-condition using it then uses it by the new name, and is saved with it. None of them means anything else.";

   /// <summary>Why it can't be renamed the typed name, or null when it can.</summary>
   public string? WhyItCannotBeRenamed => TypedName.Length == 0            ? "Type its new name."
                                          : TypedName == _sharedCondition.Name ? "That's its name now."
                                                                              : _sharedConditions.WhyItCannotNameANewOne(_sharedCondition.Level, TypedName);

   public bool CanRename => WhyItCannotBeRenamed is null;

   /// <summary>Renames it. Set by the <see cref="SharedConditionRenameDialog"/>, which closes with the <see cref="TypedName"/>.</summary>
   public IRelayCommand? AcceptCommand { get; set; }

   /// <summary>Gives up on the rename. Set by the <see cref="SharedConditionRenameDialog"/>, which closes without a name.</summary>
   public IRelayCommand? CancelCommand { get; set; }
}
