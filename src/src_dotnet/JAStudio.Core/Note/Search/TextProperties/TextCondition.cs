using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>
/// A condition on a <see cref="TextProperty"/>: one of its texts must match <see cref="Match"/>.<br/>
/// Made by the property, such as <see cref="TextProperty.Contains"/>.
/// </summary>
public sealed record TextCondition : NoteCondition
{
   internal TextCondition(TextProperty property, TextMatch match)
   {
      Property = property;
      Match = match;
   }

   /// <summary>The property whose texts are compared.</summary>
   public TextProperty Property { get; }

   /// <summary>How each text is compared.</summary>
   public TextMatch Match { get; }

   public override bool Matches(JPNote note) => Property.TextsOf(note).Any(Match.IsMatchedBy);

   public override IEnumerable<string> DescriptionLines() => [$"{Property.Name} {Match.Description}"];
}
