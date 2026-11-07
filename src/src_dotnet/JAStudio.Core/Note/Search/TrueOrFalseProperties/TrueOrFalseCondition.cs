using System.Collections.Generic;

namespace JAStudio.Core.Note.Search.TrueOrFalseProperties;

/// <summary>A condition that a <see cref="TrueOrFalseProperty"/> is <see cref="Value"/> for the note. Made by <see cref="TrueOrFalseProperty.Is"/>.</summary>
public sealed record TrueOrFalseCondition : NoteCondition
{
   internal TrueOrFalseCondition(TrueOrFalseProperty property, bool value)
   {
      Property = property;
      Value = value;
   }

   /// <summary>The property the condition is on.</summary>
   public TrueOrFalseProperty Property { get; }

   /// <summary>The value the property must have.</summary>
   public bool Value { get; }

   public override bool Matches(JPNote note) => Property.ValueOf(note) == Value;

   public override IEnumerable<string> DescriptionLines() => [$"{Property.Name} is {(Value ? "true" : "false")}"];
}
