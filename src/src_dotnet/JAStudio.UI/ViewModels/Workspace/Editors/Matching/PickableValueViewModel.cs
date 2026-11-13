using System;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching;

/// <summary>
/// One of the tokenizer's values in a condition's editor (<see cref="ConditionTextsEditorViewModel"/>), such as a part of speech, with its English: picked<br/>
/// or not, and shown while the editor's filter finds it by its Japanese or its English.
/// </summary>
/// <param name="value">The value and its English.</param>
/// <param name="isPicked">Whether it's picked to start with: whether the condition being changed has it.</param>
/// <param name="pickedOrUnpicked">Called after the value is picked or unpicked, with whether it's picked now.</param>
partial class PickableValueViewModel(TokenizerValue value, bool isPicked, Action<PickableValueViewModel, bool> pickedOrUnpicked) : ObservableObject
{
   readonly Action<PickableValueViewModel, bool> _pickedOrUnpicked = pickedOrUnpicked;

   /// <summary>The value with its English, as the condition's test offers it.</summary>
   public TokenizerValue TokenizerValue { get; } = value;

   /// <summary>The value, in Japanese, as the tokenizer gives it and a condition's code has it, such as 名詞・固有名詞.</summary>
   public string Value => TokenizerValue.Value;

   /// <summary>The value's English, such as noun › suffix.</summary>
   public string English => TokenizerValue.English;

   /// <summary>Whether the value is picked for the condition.</summary>
   [ObservableProperty] public partial bool IsPicked { get; set; } = isPicked;

   /// <summary>Whether the editor's <paramref name="filter"/> finds this value: it's in the value's Japanese or English, whatever the case. Every value when it's empty.</summary>
   public bool IsFoundBy(string filter) =>
      Value.Contains(filter, StringComparison.OrdinalIgnoreCase) || English.Contains(filter, StringComparison.OrdinalIgnoreCase);

   partial void OnIsPickedChanged(bool value) => _pickedOrUnpicked(this, value);
}
