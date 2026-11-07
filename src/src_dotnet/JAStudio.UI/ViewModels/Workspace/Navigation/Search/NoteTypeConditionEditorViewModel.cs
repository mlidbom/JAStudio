using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.NoteTypeProperties;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>The editor for a note type condition: whether the note must be a vocab, a kanji or a sentence.</summary>
partial class NoteTypeConditionEditorViewModel : ConditionEditorViewModel
{
   /// <param name="property">The note type, the property the condition is on.</param>
   /// <param name="noteType">The note class picked when the editor opens: the one the condition being changed is on.</param>
   public NoteTypeConditionEditorViewModel(NoteTypeProperty property, Type noteType) : base(property)
   {
      Choices = NoteTypeChoice.AllFor(property);
      PickedChoice = Choices.Single(it => it.Condition.NoteType == noteType);
   }

   /// <summary>The note types to pick from.</summary>
   public IReadOnlyList<NoteTypeChoice> Choices { get; }

   /// <summary>The note type the note must be.</summary>
   [ObservableProperty, NotifyPropertyChangedFor(nameof(Condition)), NotifyPropertyChangedFor(nameof(CanAccept))] NoteTypeChoice? _pickedChoice;

   public override NoteCondition? Condition => PickedChoice?.Condition;
}
