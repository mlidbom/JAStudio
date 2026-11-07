using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.UI.Views.Workspace.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>
/// The small modal dialog that picks a tag to add to some notes, or to remove from them (<see cref="TagPickerDialog"/>), with a <see cref="TagPickerViewModel"/> inside.<br/>
/// Its title says what will happen, such as "Add a tag to 312 notes".
/// </summary>
/// <remarks>Neither offers a jastudio-defined-tag, since such tags aren't added or removed by hand (<see cref="Tags.IsDefinedByJAStudio"/>).</remarks>
sealed class TagPickerDialogViewModel : ObservableObject
{
   TagPickerDialogViewModel(string title, TagPickerViewModel picker)
   {
      Title = title;
      Picker = picker;
      picker.PropertyChanged += OnPickerPropertyChanged;
   }

   /// <summary>A dialog for picking a tag to add to <paramref name="notes"/>: any tag some note in <paramref name="collection"/> has, or a new one typed.</summary>
   public static TagPickerDialogViewModel ForAddingATagTo(IReadOnlyCollection<JPNote> notes, JPCollection collection) =>
      new($"Add a tag to {NotesCounted(notes)}",
          new TagPickerViewModel(TagPathsThatCanBeAddedOrRemovedByHand(collection.TagsInUse()), "", offersTheTypedTextAsANewTag: true));

   /// <summary>A dialog for picking a tag to remove from <paramref name="notes"/>: any tag one of them has.</summary>
   public static TagPickerDialogViewModel ForRemovingATagFrom(IReadOnlyCollection<JPNote> notes) =>
      new($"Remove a tag from {NotesCounted(notes)}",
          new TagPickerViewModel(TagPathsThatCanBeAddedOrRemovedByHand(notes.SelectMany(it => it.Tags).Distinct()), "", offersTheTypedTextAsANewTag: false));

   public string Title { get; }

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
   }

   static List<string> TagPathsThatCanBeAddedOrRemovedByHand(IEnumerable<Tag> tags) =>
      [..tags.Select(it => it.Name).Where(it => !Tags.IsDefinedByJAStudio(it)).Order(StringComparer.OrdinalIgnoreCase)];

   static string NotesCounted(IReadOnlyCollection<JPNote> notes) => notes.Count == 1 ? "1 note" : $"{notes.Count} notes";
}
