using System.ComponentModel;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.TagProperties;
using JAStudio.UI.ViewModels.Workspace.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>
/// The editor for a tag condition: parts of a tag's path are typed, and the tag is picked from the tag paths in use that contain them all,<br/>
/// in a <see cref="TagPickerViewModel"/>. So typing <c>nat s3 05</c> offers <c>source::anime::natsume::s3::05</c>.
/// </summary>
class TagConditionEditorViewModel : ConditionEditorViewModel
{
   readonly TagsProperty _property;

   /// <param name="property">The note's tags, the property the condition is on.</param>
   /// <param name="tagPaths">The tag paths in use, to pick from.</param>
   /// <param name="typedParts">What's typed when the editor opens. When it's a tag path in use, that tag is picked.</param>
   public TagConditionEditorViewModel(TagsProperty property, TagPathsInUse tagPaths, string typedParts) : base(property)
   {
      _property = property;
      Picker = new TagPickerViewModel(tagPaths.All, typedParts, offersTheTypedTextAsANewTag: false);
      Picker.PropertyChanged += OnPickerPropertyChanged;
   }

   /// <summary>Picks the tag path the condition is on.</summary>
   public TagPickerViewModel Picker { get; }

   public override NoteCondition? Condition => Picker.Picked is {} picked ? _property.Include(picked.TagPath) : null;

   void OnPickerPropertyChanged(object? sender, PropertyChangedEventArgs e)
   {
      if(e.PropertyName == nameof(TagPickerViewModel.Picked))
         OnConditionChanged();
   }
}
