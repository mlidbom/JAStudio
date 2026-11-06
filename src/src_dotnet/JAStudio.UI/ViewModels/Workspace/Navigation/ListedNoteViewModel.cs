using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.SysUtils;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// A note as the navigation pane lists it, in a <see cref="ListedNotesViewModel"/>: its type, question and answer, without markup.
/// </summary>
class ListedNoteViewModel
{
   public JPNote Note { get; }
   public string NoteType { get; }
   public string Question { get; }
   public string Answer { get; }

   public ListedNoteViewModel(JPNote note)
   {
      Note = note;
      NoteType = GetNoteTypeDisplay(note);
      Question = note.GetQuestion().StripHtmlAndBracketMarkupAndNoiseCharacters();
      Answer = note.GetAnswer().StripHtmlAndBracketMarkupAndNoiseCharacters();
   }

   static string GetNoteTypeDisplay(JPNote note)
   {
      return note switch
      {
         VocabNote    => "Vocab",
         KanjiNote    => "Kanji",
         SentenceNote => "Sentence",
         _            => "Note"
      };
   }
}
