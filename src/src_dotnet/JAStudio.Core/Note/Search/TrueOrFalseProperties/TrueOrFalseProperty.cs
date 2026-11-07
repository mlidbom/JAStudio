using System;

namespace JAStudio.Core.Note.Search.TrueOrFalseProperties;

/// <summary>A property that is either true or false for a note, such as whether its reading is being studied.</summary>
public sealed class TrueOrFalseProperty : NoteProperty
{
   readonly Func<JPNote, bool> _valueOf;

   internal TrueOrFalseProperty(string name, Func<JPNote, bool> valueOf) : base(name) => _valueOf = valueOf;

   /// <summary>This property's value for <paramref name="note"/>.</summary>
   internal bool ValueOf(JPNote note) => _valueOf(note);

   /// <summary>A condition that this property is <paramref name="value"/> for the note.</summary>
   public TrueOrFalseCondition Is(bool value) => new(this, value);
}
