using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Views.Workspace.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>
/// The small modal dialog that picks a tag to add to some notes, or to remove from them (<see cref="TagPickerDialog"/>), with a <see cref="TagPickerViewModel"/> inside.<br/>
/// Its title says what will happen, such as "Add a tag to 312 notes". When the tag picked is a source tag, it warns that the notes' source changes.
/// </summary>
/// <remarks>
/// Both offer the notes' Anki tags too (<see cref="Tags.Anki"/>), which go to their study-notes in Anki. Neither offers a tag JAStudio maintains, since<br/>
/// such tags aren't added or removed by hand (<see cref="Tags.IsMaintainedByJAStudio"/>).
/// </remarks>
sealed class TagPickerDialogViewModel : ObservableObject
{
   readonly Func<Tag, int> _numberOfNotesTheTagChanges;
   readonly string _whatHappensToTheTag;

   TagPickerDialogViewModel(string title, TagPickerViewModel picker, string whatHappensToTheTag, Func<Tag, int> numberOfNotesTheTagChanges)
   {
      Title = title;
      Picker = picker;
      _whatHappensToTheTag = whatHappensToTheTag;
      _numberOfNotesTheTagChanges = numberOfNotesTheTagChanges;
      picker.PropertyChanged += OnPickerPropertyChanged;
   }

   /// <summary>A dialog for picking a tag to add to <paramref name="notes"/>: any tag some note in <paramref name="collection"/> has, or a new one typed.</summary>
   public static TagPickerDialogViewModel ForAddingATagTo(IReadOnlyCollection<JPNote> notes, JPCollection collection) =>
      new($"Add a tag to {NotesCounted(notes.Count)}",
          new TagPickerViewModel(TagPathsThatCanBeAddedOrRemovedByHand(collection.TagsInUse()), "", offersTheTypedTextAsANewTag: true),
          "adding",
          tag => notes.Count(it => !it.AllTags.Contains(tag)));

   /// <summary>A dialog for picking a tag to remove from <paramref name="notes"/>: any tag one of them has, of its own or in Anki.</summary>
   public static TagPickerDialogViewModel ForRemovingATagFrom(IReadOnlyCollection<JPNote> notes) =>
      new($"Remove a tag from {NotesCounted(notes.Count)}",
          new TagPickerViewModel(TagPathsThatCanBeAddedOrRemovedByHand(notes.SelectMany(it => it.AllTags).Distinct()), "", offersTheTypedTextAsANewTag: false),
          "removing",
          tag => notes.Count(it => it.AllTags.Contains(tag)));

   public string Title { get; }

   /// <summary>
   /// The warning shown when the tag picked is a source tag (<see cref="Tags.Source"/>), such as "source::misc is a source tag: adding it changes<br/>
   /// the source of 12 notes.", or null when it isn't. The source is the record of where the corpus's content comes from.
   /// </summary>
   public string? SourceWarning =>
      PickedTagPath is {} picked && new TagBranch(picked).IsInTheSourceBranch
         ? $"{picked} is a source tag: {_whatHappensToTheTag} it changes the source of {NotesCounted(_numberOfNotesTheTagChanges(Tag.FromName(picked)))}."
         : null;

   public TagPickerViewModel Picker { get; }

   /// <summary>The tag path the dialog closes with when it's accepted, or null while none is picked.</summary>
   public string? PickedTagPath => Picker.Picked?.TagPath;

   /// <summary>Whether the dialog can be accepted: a tag is picked.</summary>
   public bool CanAccept => PickedTagPath is not null;

   /// <summary>Accepts the <see cref="PickedTagPath"/>. Set by the <see cref="TagPickerDialog"/>, which closes with it.</summary>
   public IRelayCommand? AcceptCommand { get; set; }

   /// <summary>Gives up on picking. Set by the <see cref="TagPickerDialog"/>, which closes without a tag.</summary>
   public IRelayCommand? CancelCommand { get; set; }

   void OnPickerPropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      if(e.PropertyName != nameof(TagPickerViewModel.Picked)) return;

      OnPropertyChanged(nameof(PickedTagPath));
      OnPropertyChanged(nameof(CanAccept));
      OnPropertyChanged(nameof(SourceWarning));
   }

   static List<string> TagPathsThatCanBeAddedOrRemovedByHand(IEnumerable<Tag> tags) =>
      [..tags.Select(it => it.Name).Where(it => !Tags.IsMaintainedByJAStudio(it)).Order(StringComparer.OrdinalIgnoreCase)];

   static string NotesCounted(int numberOfNotes) => numberOfNotes == 1 ? "1 note" : $"{numberOfNotes} notes";
}
