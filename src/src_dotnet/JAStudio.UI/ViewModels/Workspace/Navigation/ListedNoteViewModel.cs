using JAStudio.Core.Note;
using JAStudio.Core.SysUtils;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>A note as the navigation pane lists it, in a <see cref="ListedNotesOfOneTypeViewModel"/>: its question and answer, without markup.</summary>
class ListedNoteViewModel
{
   public JPNote Note { get; }
   public string Question { get; }
   public string Answer { get; }

   public ListedNoteViewModel(JPNote note)
   {
      Note = note;
      Question = note.GetQuestion().StripHtmlAndBracketMarkupAndNoiseCharacters();
      Answer = note.GetAnswer().StripHtmlAndBracketMarkupAndNoiseCharacters();
   }
}
