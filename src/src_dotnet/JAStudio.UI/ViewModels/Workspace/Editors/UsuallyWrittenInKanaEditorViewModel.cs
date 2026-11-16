using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.Views.Workspace.Editors;

namespace JAStudio.UI.ViewModels.Workspace.Editors;

/// <summary>
/// Whether a vocab is usually written in kana (<see cref="VocabNoteUsuallyWrittenInKana"/>), edited live in the vocab's Metadata tab<br/>
/// as three radio buttons (<see cref="UsuallyWrittenInKanaEditor"/>): undecided, which leaves it for JAStudio to decide, yes, or no.<br/>
/// A choice is written to the vocab at once.
/// </summary>
/// <remarks>
/// It reads the <see cref="VocabNoteUsuallyWrittenInKana.Decision"/> on the vocab the collection holds now, and decides it in an edit of the vocab,<br/>
/// through <see cref="_usuallyWrittenInKana"/>, since the collection replaces a note's object when it reloads the note (see <see cref="VocabEditorSectionViewModel"/>).
/// </remarks>
class UsuallyWrittenInKanaEditorViewModel : ObservableObject
{
   readonly IFatalErrorHandler _fatalErrorHandler;
   readonly EditedNotePart<VocabNoteUsuallyWrittenInKana> _usuallyWrittenInKana;
   UsuallyWrittenInKanaDecision _decision;

   /// <param name="fatalErrorHandler">Shows the user an exception thrown while writing a choice, which the radio buttons' bindings would swallow.</param>
   /// <param name="usuallyWrittenInKana">The <see cref="VocabNoteUsuallyWrittenInKana"/> of the vocab the editor edits.</param>
   public UsuallyWrittenInKanaEditorViewModel(IFatalErrorHandler fatalErrorHandler, EditedNotePart<VocabNoteUsuallyWrittenInKana> usuallyWrittenInKana)
   {
      _fatalErrorHandler = fatalErrorHandler;
      _usuallyWrittenInKana = usuallyWrittenInKana;
      _decision = DecisionOnTheVocab();
   }

   /// <summary>Whether it's <see cref="UsuallyWrittenInKanaDecision.Undecided"/>, so JAStudio may decide it when it generates the vocab's data.</summary>
   public bool IsUndecided
   {
      get => _decision == UsuallyWrittenInKanaDecision.Undecided;
      set => ChooseIfChecked(value, UsuallyWrittenInKanaDecision.Undecided);
   }

   public bool IsYes
   {
      get => _decision == UsuallyWrittenInKanaDecision.Yes;
      set => ChooseIfChecked(value, UsuallyWrittenInKanaDecision.Yes);
   }

   public bool IsNo
   {
      get => _decision == UsuallyWrittenInKanaDecision.No;
      set => ChooseIfChecked(value, UsuallyWrittenInKanaDecision.No);
   }

   /// <summary>Shows the <see cref="VocabNoteUsuallyWrittenInKana.Decision"/> as the vocab has it now, after a change made elsewhere.</summary>
   public void Refresh() => Show(DecisionOnTheVocab());

   /// <summary>Checking a radio button makes its choice. Unchecking the others, which the radio buttons do by themselves, chooses nothing.</summary>
   void ChooseIfChecked(bool isChecked, UsuallyWrittenInKanaDecision decision)
   {
      if(!isChecked || decision == _decision) return;

      _fatalErrorHandler.HandleExceptionsFrom(() =>
      {
         _usuallyWrittenInKana.Edit(it => it.Decide(decision));
         Show(decision);
      });
   }

   void Show(UsuallyWrittenInKanaDecision decision)
   {
      _decision = decision;
      OnPropertyChanged(nameof(IsUndecided));
      OnPropertyChanged(nameof(IsYes));
      OnPropertyChanged(nameof(IsNo));
   }

   UsuallyWrittenInKanaDecision DecisionOnTheVocab() => _usuallyWrittenInKana.Current.Decision;
}
