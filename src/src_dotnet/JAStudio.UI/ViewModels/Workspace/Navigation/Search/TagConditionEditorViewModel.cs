using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Search.TagProperties;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.Search;

/// <summary>
/// The editor for a tag condition: parts of a tag's path are typed, and the tag is picked from the tag paths in use that contain them all.<br/>
/// So typing <c>nat s3 05</c> offers <c>source::anime::natsume::s3::05</c>.
/// </summary>
partial class TagConditionEditorViewModel : ConditionEditorViewModel
{
   readonly TagsProperty _property;
   readonly TagPathsInUse _tagPaths;

   /// <param name="property">The note's tags, the property the condition is on.</param>
   /// <param name="tagPaths">The tag paths in use, to pick from.</param>
   /// <param name="typedParts">What's typed when the editor opens. When it's a tag path in use, that tag is picked.</param>
   public TagConditionEditorViewModel(TagsProperty property, TagPathsInUse tagPaths, string typedParts) : base(property)
   {
      _property = property;
      _tagPaths = tagPaths;
      TypedParts = typedParts;
      if(MatchingTagPaths.Contains(typedParts))
         PickedTagPath = typedParts;
   }

   /// <summary>The parts of a tag's path typed so far, separated by spaces.</summary>
   [ObservableProperty] string _typedParts = "";

   /// <summary>The tag paths in use that contain every typed part, to pick from.</summary>
   [ObservableProperty] IReadOnlyList<string> _matchingTagPaths = [];

   /// <summary>The tag path picked from <see cref="MatchingTagPaths"/>. The first one, until another is picked.</summary>
   [ObservableProperty, NotifyPropertyChangedFor(nameof(Condition)), NotifyPropertyChangedFor(nameof(CanAccept))] string? _pickedTagPath;

   public override NoteCondition? Condition => PickedTagPath is {} tagPath ? _property.Include(tagPath) : null;

   partial void OnTypedPartsChanged(string value)
   {
      MatchingTagPaths = _tagPaths.Matching(value);
      PickedTagPath = MatchingTagPaths.FirstOrDefault();
   }
}
