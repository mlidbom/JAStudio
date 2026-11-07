using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.TrueOrFalseProperties;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>The editor for a condition on a <see cref="TrueOrFalseProperty"/>: whether the property must be true or false.</summary>
partial class TrueOrFalseConditionEditorViewModel : ConditionEditorViewModel
{
   readonly TrueOrFalseProperty _property;

   public TrueOrFalseConditionEditorViewModel(TrueOrFalseProperty property, bool value) : base(property)
   {
      _property = property;
      Value = value;
   }

   /// <summary>The values to pick from, true first.</summary>
   public IReadOnlyList<bool> Values { get; } = [true, false];

   /// <summary>The value the property must have.</summary>
   [ObservableProperty, NotifyPropertyChangedFor(nameof(Condition)), NotifyPropertyChangedFor(nameof(CanAccept))] bool _value;

   public override NoteCondition Condition => _property.Is(Value);
}
