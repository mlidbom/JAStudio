using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Views.Workspace.Navigation;

namespace JAStudio.UI.ViewModels.Workspace.Navigation;

/// <summary>
/// A list of notes in the navigation pane that open in the workspace's tabs, such as the notes a search found (<see cref="ListedNotesView"/>).<br/>
/// The notes are split by note type, and when the list holds more than one type, each type gets a tab of its own.
/// </summary>
/// <remarks>
/// The type picked last stays shown when other notes are listed, as long as there are notes of that type:<br/>
/// one usually knows which type of note one is after.
/// </remarks>
partial class ListedNotesViewModel : ObservableObject
{
   readonly ListedNotesOfOneTypeViewModel _vocab;
   readonly ListedNotesOfOneTypeViewModel _kanji;
   readonly ListedNotesOfOneTypeViewModel _sentences;

   ListedNotesOfOneTypeViewModel? _shown;

   /// <summary>The type picked last in its tab. Null until one is picked.</summary>
   ListedNotesOfOneTypeViewModel? _pickedLast;

   public ListedNotesViewModel(WorkspaceViewModel workspace)
   {
      _vocab = new ListedNotesOfOneTypeViewModel(workspace, "Vocab");
      _kanji = new ListedNotesOfOneTypeViewModel(workspace, "Kanji");
      _sentences = new ListedNotesOfOneTypeViewModel(workspace, "Sentences");
      OfEachType = [_vocab, _kanji, _sentences];
   }

   /// <summary>The notes of each type, in the order their tabs come in. A type without notes has no tab.</summary>
   public IReadOnlyList<ListedNotesOfOneTypeViewModel> OfEachType { get; }

   /// <summary>Whether notes of more than one type are listed, so that each type gets a tab.</summary>
   public bool ShowsSeveralTypes => OfEachType.Count(it => it.HasNotes) > 1;

   /// <summary>The type whose notes are shown. Set by its tab when the tab is clicked.</summary>
   public ListedNotesOfOneTypeViewModel? Shown
   {
      get => _shown;
      set
      {
         SetProperty(ref _shown, value);
         _pickedLast = value;
      }
   }

   /// <summary>Lists <paramref name="notes"/>, split by type, instead of the notes listed so far.</summary>
   public void Show(IEnumerable<JPNote> notes)
   {
      var notesToList = notes.ToList();
      _vocab.Show(notesToList.OfType<VocabNote>());
      _kanji.Show(notesToList.OfType<KanjiNote>());
      _sentences.Show(notesToList.OfType<SentenceNote>());
      OnPropertyChanged(nameof(ShowsSeveralTypes));

      _shown = TypeToShow();
      OnPropertyChanged(nameof(Shown));
   }

   /// <summary>The type picked last if any of its notes are listed, or else the first type with notes listed.</summary>
   ListedNotesOfOneTypeViewModel? TypeToShow() => _pickedLast is { HasNotes: true } ? _pickedLast : OfEachType.FirstOrDefault(it => it.HasNotes);
}
