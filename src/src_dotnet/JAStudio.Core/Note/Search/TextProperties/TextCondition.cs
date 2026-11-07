using System.Collections.Generic;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>A condition on a <see cref="TextProperty"/>: its text must match <see cref="Match"/>. Made by the property, such as <see cref="TextProperty.Contains"/>.</summary>
public sealed record TextCondition : NoteCondition
{
   internal TextCondition(TextProperty property, TextMatch match)
   {
      Property = property;
      Match = match;
   }

   /// <summary>The property whose text is compared.</summary>
   public TextProperty Property { get; }

   /// <summary>How the text is compared.</summary>
   public TextMatch Match { get; }

   public override bool Matches(JPNote note) => Match.IsMatchedBy(Property.TextOf(note));

   public override IEnumerable<string> DescriptionLines() => [$"{Property.Name} {Match.Description}"];
}
