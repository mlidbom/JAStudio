using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.NoteFields;

/// <summary>
/// A yes-or-no setting of a note, such as whether a vocab is a poison word or is polite: set or not. Setting it saves the note.
/// </summary>
/// <remarks>
/// The corpus doesn't store a flag by itself: a note's flags of one kind, such as a vocab's register, are stored together<br/>
/// as the <see cref="Name"/>s of those that are set (<see cref="FlagFieldList"/>).
/// </remarks>
public class FlagField
{
   readonly NoteGuard _guard;
   bool _isSet;

   /// <param name="name">The flag's <see cref="Name"/>.</param>
   /// <param name="namesOfTheSetFlags">The names of the note's flags of this kind that the corpus has as set.</param>
   /// <param name="guard">Saves the note after a change.</param>
   internal FlagField(string name, IReadOnlyCollection<string> namesOfTheSetFlags, NoteGuard guard)
   {
      Name = name;
      _isSet = namesOfTheSetFlags.Contains(name);
      _guard = guard;
   }

   /// <summary>The flag's name in the corpus, such as <c>isPoisonWord</c>.</summary>
   public string Name { get; }

   public bool IsSet() => _isSet;

   /// <summary>Sets or clears the flag, and saves the note.</summary>
   public void SetTo(bool set) => _guard.Update(() => _isSet = set);

   public override string ToString() => $"{Name}: {IsSet()}";
}
