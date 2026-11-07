using System;
using System.Collections.Generic;

namespace JAStudio.Core.Note.Search.NoteTypeProperties;

/// <summary>A condition that a note is of the type <see cref="NoteType"/>, such as a sentence. Made by <see cref="NoteTypeProperty.Is{TNote}"/>.</summary>
public sealed record NoteTypeCondition : NoteCondition
{
   internal NoteTypeCondition(NoteTypeProperty property, Type noteType)
   {
      Property = property;
      NoteType = noteType;
   }

   /// <summary>The property the condition is on, the note's type.</summary>
   public NoteTypeProperty Property { get; }

   /// <summary>The note class the note must be, such as <see cref="Sentences.SentenceNote"/>.</summary>
   public Type NoteType { get; }

   public override bool Matches(JPNote note) => NoteType.IsInstanceOfType(note);

   public override IEnumerable<string> DescriptionLines() => [$"{Property.Name} is {NoteType.Name}"];
}
