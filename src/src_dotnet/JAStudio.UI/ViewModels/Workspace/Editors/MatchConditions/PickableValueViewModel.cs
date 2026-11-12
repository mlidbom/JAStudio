using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;

namespace JAStudio.UI.ViewModels.Workspace.Editors.MatchConditions;

/// <summary>
/// One of the tokenizer's values in a condition's editor (<see cref="ConditionTextsEditorViewModel"/>), such as a part of speech, with its English: picked<br/>
/// or not, and shown while the editor's filter finds it by its Japanese or its English.
/// </summary>
partial class PickableValueViewModel : ObservableObject
{
   readonly Action<PickableValueViewModel> _pickedOrUnpicked;

   /// <param name="value">The value and its English.</param>
   /// <param name="isPicked">Whether it's picked to start with: whether the condition being changed has it.</param>
   /// <param name="pickedOrUnpicked">Called after the value is picked or unpicked.</param>
   public PickableValueViewModel(PickableValue value, bool isPicked, Action<PickableValueViewModel> pickedOrUnpicked)
   {
      Value = value.Value;
      English = value.English;
      _isPicked = isPicked;
      _pickedOrUnpicked = pickedOrUnpicked;
   }

   /// <summary>The value, in Japanese, as the tokenizer gives it and a condition's code has it, such as 名詞・接尾.</summary>
   public string Value { get; }

   /// <summary>The value's English, such as noun › suffix.</summary>
   public string English { get; }

   /// <summary>Whether the value is picked for the condition.</summary>
   [ObservableProperty] bool _isPicked;

   /// <summary>Whether the editor's <paramref name="filter"/> finds this value: it's in the value's Japanese or English, whatever the case. Every value when it's empty.</summary>
   public bool IsFoundBy(string filter) =>
      Value.Contains(filter, StringComparison.OrdinalIgnoreCase) || English.Contains(filter, StringComparison.OrdinalIgnoreCase);

   partial void OnIsPickedChanged(bool value) => _pickedOrUnpicked(this);
}
