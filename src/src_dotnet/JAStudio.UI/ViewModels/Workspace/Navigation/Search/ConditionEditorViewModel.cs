using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.NoteTypeProperties;
using JAStudio.Core.Note.Search.TagProperties;
using JAStudio.Core.Note.Search.TextProperties;
using JAStudio.Core.Note.Search.TrueOrFalseProperties;
using JAStudio.UI.Views.Workspace.Navigation.Search;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>
/// The editor for one condition on a property, shown in a <see cref="SearchConditionDialog"/>: a new condition, or one being changed.<br/>
/// Each kind of property has its own, such as <see cref="TextConditionEditorViewModel"/>.
/// </summary>
/// <remarks>
/// A new condition on a property with only a few values, such as the note type or studying reading, needs no editor:<br/>
/// the add menu's submenu for the property adds it at once. Changing one opens its editor.
/// </remarks>
abstract class ConditionEditorViewModel : ObservableObject
{
   protected ConditionEditorViewModel(NoteProperty property) => Title = property.Name;

   /// <summary>The dialog's title: the name of the property the condition is on.</summary>
   public string Title { get; }

   /// <summary>The condition the editor's inputs describe, or null while they don't describe one yet, such as while a text is empty.</summary>
   public abstract NoteCondition? Condition { get; }

   /// <summary>Whether the dialog can be accepted: the inputs describe a <see cref="Condition"/>.</summary>
   public bool CanAccept => Condition is not null;

   /// <summary>Accepts the editor's <see cref="Condition"/>. Set by the <see cref="SearchConditionDialog"/>, which closes with it.</summary>
   public IRelayCommand? AcceptCommand { get; set; }

   /// <summary>Gives up on the condition. Set by the <see cref="SearchConditionDialog"/>, which closes without one.</summary>
   public IRelayCommand? CancelCommand { get; set; }

   /// <summary>Called by an editor when an input changes, since <see cref="Condition"/> may change with it.</summary>
   protected void OnConditionChanged()
   {
      OnPropertyChanged(nameof(Condition));
      OnPropertyChanged(nameof(CanAccept));
   }

   /// <summary>
   /// An editor for a new condition on <paramref name="property"/>, the text property, comparing its text in the way <paramref name="matchChoice"/> says.<br/>
   /// The add menu's submenu for the property picks the way.
   /// </summary>
   public static ConditionEditorViewModel ForANewTextCondition(TextProperty property, TextMatchChoice matchChoice) =>
      new TextConditionEditorViewModel(property, matchChoice, "");

   /// <summary>An editor for a new tag condition on <paramref name="property"/>, offering the tag paths in use in <paramref name="collection"/>.</summary>
   public static ConditionEditorViewModel ForANewTagCondition(TagsProperty property, JPCollection collection) =>
      new TagConditionEditorViewModel(property, collection.TagPathsInUse(), "");

   /// <summary>An editor filled in with <paramref name="condition"/>, to change it.</summary>
   public static ConditionEditorViewModel ForChanging(NoteCondition condition, JPCollection collection) => condition switch
   {
      TextCondition text => new TextConditionEditorViewModel(text.Property, TextMatchChoice.For(text.Match), TextMatchChoice.TextOf(text.Match)),
      TagCondition tag => new TagConditionEditorViewModel(tag.Property, collection.TagPathsInUse(), tag.TagPath),
      TrueOrFalseCondition trueOrFalse => new TrueOrFalseConditionEditorViewModel(trueOrFalse.Property, trueOrFalse.Value),
      NoteTypeCondition noteType => new NoteTypeConditionEditorViewModel(noteType.Property, noteType.NoteType),
      _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "Only a condition on a property can be changed in an editor.")
   };
}
