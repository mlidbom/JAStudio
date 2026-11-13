using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Views.Workspace.Tagging;

namespace JAStudio.UI.ViewModels.Workspace.Tagging;

/// <summary>
/// Picks a tag path by typing parts of it (<see cref="TagPickerView"/>): the offered paths that contain every typed part are listed,<br/>
/// and one is picked from them, the first until another is. So typing <c>nat s3 05</c> offers <c>source::anime::natsume::s3::05</c>.<br/>
/// It can also offer the typed text itself as a new tag, for adding a tag no note has yet.
/// </summary>
/// <remarks>Used by the tag condition's editor, to pick the tag a condition is on, and by the <see cref="TagPickerDialogViewModel"/>, to add or remove a tag.</remarks>
partial class TagPickerViewModel : ObservableObject
{
   readonly IReadOnlyList<string> _offeredTagPaths;
   readonly bool _offersTheTypedTextAsANewTag;

   /// <param name="offeredTagPaths">The tag paths to pick from.</param>
   /// <param name="typedParts">What's typed when the picker opens. When it's one of the offered paths, ignoring case, that path is picked.</param>
   /// <param name="offersTheTypedTextAsANewTag">
   /// Whether the typed text is also listed, last, as a new tag, when it can be the name of a tag and isn't offered already.<br/>
   /// A jastudio-defined-tag is never offered as a new tag, since such tags aren't added by hand.
   /// </param>
   public TagPickerViewModel(IReadOnlyList<string> offeredTagPaths, string typedParts, bool offersTheTypedTextAsANewTag)
   {
      _offeredTagPaths = offeredTagPaths;
      _offersTheTypedTextAsANewTag = offersTheTypedTextAsANewTag;
      TypedParts = typedParts;
      PickTheOfferedPathTyped();
   }

   /// <summary>The parts of a tag's path typed so far, separated by spaces.</summary>
   [ObservableProperty] public partial string TypedParts { get; set; }

   /// <summary>The offered tag paths that contain every typed part, and the typed text as a new tag when that's offered.</summary>
   [ObservableProperty] public partial IReadOnlyList<TagPathChoice> Choices { get; set; } = [];

   /// <summary>The tag path picked from <see cref="Choices"/>. The first one, until another is picked, and null while there are none.</summary>
   [ObservableProperty] public partial TagPathChoice? Picked { get; set; }

   partial void OnTypedPartsChanged(string value) => ShowTheChoicesFor(value);

   /// <summary>Picks the offered path that the <see cref="TypedParts"/> are, ignoring case, rather than the first of the <see cref="Choices"/>, if one is.</summary>
   void PickTheOfferedPathTyped()
   {
      if(Choices.FirstOrDefault(it => !it.IsNew && string.Equals(it.TagPath, TypedParts, StringComparison.OrdinalIgnoreCase)) is {} typedPath)
         Picked = typedPath;
   }

   void ShowTheChoicesFor(string typedParts)
   {
      Choices = [..new TypedTagPathParts(typedParts).Matching(_offeredTagPaths).Select(TagPathChoice.Offered), ..TheNewTagFor(typedParts.Trim())];
      Picked = Choices.FirstOrDefault();
   }

   IEnumerable<TagPathChoice> TheNewTagFor(string typedName)
   {
      if(_offersTheTypedTextAsANewTag && CanBeANewTag(typedName))
         yield return TagPathChoice.NewTag(typedName);
   }

   bool CanBeANewTag(string typedName) =>
      Tags.WhyItCannotBeATagAPersonManages(typedName) is null
      && !_offeredTagPaths.Contains(typedName, StringComparer.OrdinalIgnoreCase);
}
