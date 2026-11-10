using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Search.TagProperties;

/// <summary>
/// A condition that a note has a tag in the <see cref="TagBranch"/> at <see cref="TagPath"/>: that tag, or one below it, ignoring case.<br/>
/// Made by <see cref="TagsProperty.Include"/>.<br/>
/// Only whole levels count: <c>source::anime</c> matches <c>source::anime::natsume</c>, but <c>source::ani</c> doesn't.
/// </summary>
public sealed record TagCondition : NoteCondition
{
   internal TagCondition(TagsProperty property, string tagPath)
   {
      Property = property;
      TagPath = tagPath;
   }

   /// <summary>The property the condition is on, the note's tags.</summary>
   public TagsProperty Property { get; }

   /// <summary>The full path of the tag whose branch the note must have a tag in, such as <c>source::anime</c>.</summary>
   public string TagPath { get; }

   public override bool Matches(JPNote note) => note.AllTags.Any(new TagBranch(TagPath).Contains);

   public override IEnumerable<string> DescriptionLines() => [$"{Property.Name} include {TagPath}"];
}
