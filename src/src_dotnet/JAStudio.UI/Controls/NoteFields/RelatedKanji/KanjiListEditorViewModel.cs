using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.UI.Controls.NoteFields.RelatedKanji;

/// <summary>
/// One of a kanji's lists of other kanji, such as its radicals or the kanji it's confused with (<see cref="KanjiEditableFields"/>), edited live in a<br/>
/// <see cref="KanjiListEditor"/>: each kanji as a chip with a button that removes it, and a box for adding one, which offers the corpus's kanji, filtered<br/>
/// by what's typed. Adding or removing a kanji writes to the kanji at once, through the kanji's own operations, so a relationship that goes both ways<br/>
/// changes on both sides.
/// </summary>
/// <remarks>
/// A kanji is one character, so only one character can be added; a kanji the corpus has no note for can be, by typing it. It reads and writes the list<br/>
/// through <c>currentKanji</c>, which finds the kanji as the collection holds it now, since the collection replaces a note's object when it reloads the note.
/// </remarks>
partial class KanjiListEditorViewModel : ObservableObject
{
   readonly IFieldEditedOneItemAtATime<KanjiNote, string> _field;
   readonly Func<KanjiNote> _currentKanji;
   readonly Lazy<IReadOnlyList<PickableKanji>> _pickable;

   /// <param name="field">The list edited, as one of the kanji's editable fields.</param>
   /// <param name="currentKanji">Finds the kanji being edited, as the collection holds it now.</param>
   /// <param name="pickable">The kanji the box offers, shared by the editors of all the kanji's lists, and listed only once a box needs them.</param>
   public KanjiListEditorViewModel(IFieldEditedOneItemAtATime<KanjiNote, string> field, Func<KanjiNote> currentKanji, Lazy<IReadOnlyList<PickableKanji>> pickable)
   {
      _field = field;
      _currentKanji = currentKanji;
      _pickable = pickable;
      Refresh();
   }

   /// <summary>What the list is, shown above it.</summary>
   public string Title => _field.Label;

   /// <summary>The kanji, in the list's order.</summary>
   public ObservableCollection<KanjiChipViewModel> Items { get; } = [];

   /// <summary>The kanji the box for adding one offers.</summary>
   public IReadOnlyList<PickableKanji> Pickable => _pickable.Value;

   /// <summary>The kanji to add, as typed or picked.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   public partial string NewKanji { get; set; } = string.Empty;

   /// <summary>Shows the list as the kanji has it now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      var kanji = _currentKanji();
      Items.Clear();
      foreach(var other in _field.Read(kanji))
         Items.Add(new KanjiChipViewModel(other, TheCorpusHasANoteFor(other), Remove));
      return;

      bool TheCorpusHasANoteFor(string other) => kanji.Services.Collection.Kanji.WithKanji(other) is not null;
   }

   /// <summary>Adds the kanji typed in <see cref="NewKanji"/>.</summary>
   [RelayCommand(CanExecute = nameof(CanAdd))]
   void Add()
   {
      _field.Add(_currentKanji(), TheKanjiTyped);
      NewKanji = string.Empty;
      Refresh();
   }

   /// <summary>Whether <see cref="NewKanji"/> is one character, other than the kanji's own, that the list may take.</summary>
   bool CanAdd() => IsOneCharacter(TheKanjiTyped) && !IsTheKanjiItself(TheKanjiTyped) && TheListMayTake(TheKanjiTyped);

   string TheKanjiTyped => NewKanji.Trim();

   bool IsTheKanjiItself(string kanji) => _currentKanji().IsNamedBy(kanji);

   /// <summary>
   /// Whether the list may take <paramref name="kanji"/>: any kanji when it may have a kanji more than once, as a kanji's radicals may<br/>
   /// (<see cref="IFieldEditedOneItemAtATime{TNote,TItem}.HoldsEachItemOnce"/>), and otherwise one it doesn't have yet.
   /// </summary>
   bool TheListMayTake(string kanji) => !_field.HoldsEachItemOnce || !IsInTheList(kanji);

   bool IsInTheList(string kanji) => _field.Read(_currentKanji()).Contains(kanji);

   static bool IsOneCharacter(string text) => new StringInfo(text).LengthInTextElements == 1;

   void Remove(string kanji)
   {
      _field.Remove(_currentKanji(), kanji);
      Refresh();
   }
}
