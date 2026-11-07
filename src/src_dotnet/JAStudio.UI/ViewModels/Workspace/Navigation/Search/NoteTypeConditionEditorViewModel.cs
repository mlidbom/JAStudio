using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.NoteTypeProperties;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>The editor for a note type condition: whether the note must be a vocab, a kanji or a sentence.</summary>
partial class NoteTypeConditionEditorViewModel : ConditionEditorViewModel
{
   /// <param name="property">The note type, the property the condition is on.</param>
   /// <param name="noteType">The note class picked when the editor opens, or null to pick the first.</param>
   public NoteTypeConditionEditorViewModel(NoteTypeProperty property, Type? noteType) : base(property)
   {
      Choices = [new NoteTypeChoice("Vocab", property.Is<VocabNote>()), new NoteTypeChoice("Kanji", property.Is<KanjiNote>()), new NoteTypeChoice("Sentence", property.Is<SentenceNote>())];
      PickedChoice = Choices.FirstOrDefault(it => it.Condition.NoteType == noteType) ?? Choices[0];
   }

   /// <summary>The note types to pick from.</summary>
   public IReadOnlyList<NoteTypeChoice> Choices { get; }

   /// <summary>The note type the note must be.</summary>
   [ObservableProperty, NotifyPropertyChangedFor(nameof(Condition)), NotifyPropertyChangedFor(nameof(CanAccept))] NoteTypeChoice? _pickedChoice;

   public override NoteCondition? Condition => PickedChoice?.Condition;
}

