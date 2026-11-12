using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.Controls.NoteFields.RelatedWords;

/// <summary>
/// One of a vocab's relationships to other words, such as its synonyms (<see cref="Core.Note.Vocabulary.RelatedVocab.WordRelationship"/>), edited live in a<br/>
/// <see cref="WordSetEditor"/>: each word as a chip with a button that removes it, and a box for adding one, which offers the corpus's vocab, filtered by<br/>
/// what's typed. Adding or removing a word writes to the vocab at once, through the vocab's own operations, so a relationship that goes both ways changes<br/>
/// on both sides.
/// </summary>
/// <remarks>
/// A word that no vocab has can be added too, by typing its name, to a set that accepts it (<see cref="EditableSetField{TNote,TItem}.Accepts"/>): a<br/>
/// word-reference names a word, which may not have a vocab yet. For such a word the editor also offers to create its vocab and add that<br/>
/// (<see cref="OffersToCreateTheVocab"/>), the only way to add it to a set that takes only vocab, such as the perfect synonyms.<br/>
/// It reads and writes the relationship through <c>currentVocab</c>, which finds the vocab as the collection holds it now, since the collection replaces<br/>
/// a note's object when it reloads the note.
/// </remarks>
partial class WordSetEditorViewModel : ObservableObject
{
   readonly EditableSetField<VocabNote, WordReference> _field;
   readonly Func<VocabNote> _currentVocab;
   readonly Lazy<IReadOnlyList<PickableVocab>> _pickable;
   readonly Action<WordReference> _createTheVocabAndAddIt;

   /// <param name="field">The relationship edited, as one of the vocab's editable fields.</param>
   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   /// <param name="pickable">The vocab the box offers, shared by the editors of all the vocab's relationships, and listed only once a box needs them.</param>
   /// <param name="createTheVocabAndAddIt">Creates the vocab of a word no vocab has, adds it to the set, and opens the new vocab for editing.</param>
   public WordSetEditorViewModel(EditableSetField<VocabNote, WordReference> field, Func<VocabNote> currentVocab, Lazy<IReadOnlyList<PickableVocab>> pickable,
                                 Action<WordReference> createTheVocabAndAddIt)
   {
      _field = field;
      _currentVocab = currentVocab;
      _pickable = pickable;
      _createTheVocabAndAddIt = createTheVocabAndAddIt;
      Refresh();
   }

   /// <summary>What the relationship is, shown above it.</summary>
   public string Title => _field.Label;

   /// <summary>The words, in the order of their names.</summary>
   public ObservableCollection<WordChipViewModel> Items { get; } = [];

   /// <summary>The vocab the box for adding a word offers.</summary>
   public IReadOnlyList<PickableVocab> Pickable => _pickable.Value;

   /// <summary>The name of the word to add, as typed or picked, such as <c>入る:masu</c>.</summary>
   [ObservableProperty]
   [NotifyCanExecuteChangedFor(nameof(AddCommand))]
   [NotifyCanExecuteChangedFor(nameof(CreateAndAddCommand))]
   [NotifyPropertyChangedFor(nameof(OffersToCreateTheVocab))]
   string _newWord = string.Empty;

   /// <summary>Shows the words as the vocab has them now, after a change made elsewhere.</summary>
   public void Refresh()
   {
      var vocab = _currentVocab();
      Items.Clear();
      foreach(var word in _field.Read(vocab))
         Items.Add(new WordChipViewModel(word, RefersToAVocab(word), Remove));
   }

   /// <summary>Adds the word named in <see cref="NewWord"/>.</summary>
   [RelayCommand(CanExecute = nameof(CanAdd))]
   void Add()
   {
      _field.Add(_currentVocab(), TheWordTyped ?? throw new InvalidOperationException("Adding runs only while the text typed names a word."));
      NewWord = string.Empty;
      Refresh();
   }

   /// <summary>Whether <see cref="NewWord"/> names a word that isn't in the set yet, isn't the vocab itself, and the set accepts.</summary>
   bool CanAdd() => TheWordTyped is {} word && !IsInTheSet(word) && !IsTheVocabItself(word) && _field.Accepts(_currentVocab(), word);

   /// <summary>Creates the vocab of the word named in <see cref="NewWord"/>, adds it, and opens the new vocab for editing.</summary>
   [RelayCommand(CanExecute = nameof(OffersToCreateTheVocab))]
   void CreateAndAdd()
   {
      var word = TheWordTyped ?? throw new InvalidOperationException("Creating runs only while the text typed names a word.");
      NewWord = string.Empty;
      _createTheVocabAndAddIt(word);
   }

   /// <summary>
   /// Whether the editor offers to create the vocab of the word named in <see cref="NewWord"/> and add it: when no vocab has that word, the word has no<br/>
   /// disambiguation, since a vocab that needs one is made by cloning another, and it isn't in the set yet.
   /// </summary>
   public bool OffersToCreateTheVocab => TheWordTyped is { IsDisambiguated: false } word && !IsInTheSet(word) && !RefersToAVocab(word);

   bool RefersToAVocab(WordReference word) => _currentVocab().Services.Collection.Vocab.ReferredToBy(word).Count > 0;

   bool IsInTheSet(WordReference word) => _field.Read(_currentVocab()).Contains(word);

   bool IsTheVocabItself(WordReference word) => _currentVocab().Question.IsNamedBy(word);

   /// <summary>The word <see cref="NewWord"/> names, or null while it names none, such as while nothing is typed.</summary>
   WordReference? TheWordTyped => WordReference.FromDisambiguationNameOrNone(NewWord.Trim());

   void Remove(WordReference word)
   {
      _field.Remove(_currentVocab(), word);
      Refresh();
   }
}
