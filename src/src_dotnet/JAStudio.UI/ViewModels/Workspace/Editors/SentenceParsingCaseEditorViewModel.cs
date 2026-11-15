using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.UI.Controls.NoteFields;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// The parsing case group of a sentence's editor (<see cref="SentenceEditorSectionViewModel"/>): the name and the why of the sentence's parsing<br/>
/// case (<see cref="ParsingCase"/>), edited live as its other texts are. Shown only while the sentence is a parsing case.
/// </summary>
sealed partial class SentenceParsingCaseEditorViewModel : ObservableObject
{
   readonly Func<SentenceNote> _currentSentence;

   /// <param name="fields">The editors of the parsing case's name and its why.</param>
   /// <param name="currentSentence">The sentence being edited, as the collection holds it now.</param>
   public SentenceParsingCaseEditorViewModel(IReadOnlyList<TextFieldEditorViewModel> fields, Func<SentenceNote> currentSentence)
   {
      Fields = fields;
      _currentSentence = currentSentence;
      IsAParsingCase = currentSentence().ParsingCase != null;
   }

   /// <summary>The editors of the parsing case's name and its why.</summary>
   public IReadOnlyList<TextFieldEditorViewModel> Fields { get; }

   /// <summary>Whether the sentence is a parsing case, so that the group is shown.</summary>
   [ObservableProperty] public partial bool IsAParsingCase { get; set; }

   /// <summary>Shows the group as the sentence is now: whether it's a parsing case, and its name and why.</summary>
   public void Refresh()
   {
      IsAParsingCase = _currentSentence().ParsingCase != null;
      foreach(var field in Fields) field.Refresh();
   }
}
