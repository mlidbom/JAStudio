namespace JAStudio.Core.Note.NoteFields;

/// <summary>
/// A yes-or-no setting of a note, such as whether a vocab is a poison word or is polite: set or not.
/// </summary>
/// <remarks>
/// The corpus doesn't store a flag by itself: a note's flags of one kind, such as a vocab's register, are stored together<br/>
/// as the <see cref="Name"/>s of those that are set (<see cref="FlagFieldList"/>), which is also what a change to one of them publishes.
/// </remarks>
public class FlagField
{
   readonly FlagFieldList _flagsOfItsKind;

   /// <param name="name">The flag's <see cref="Name"/>.</param>
   /// <param name="flagsOfItsKind">The note's flags of the flag's kind, which hold whether it's set.</param>
   internal FlagField(string name, FlagFieldList flagsOfItsKind)
   {
      Name = name;
      _flagsOfItsKind = flagsOfItsKind;
   }

   /// <summary>The flag's name in the corpus, such as <c>isPoisonWord</c>.</summary>
   public string Name { get; }

   public bool IsSet() => _flagsOfItsKind.IsSet(this);

   /// <summary>Sets or clears the flag: a change publishes the tevent of the note's flags of its kind.</summary>
   public void SetTo(bool set) => _flagsOfItsKind.SetTo(this, set);

   public override string ToString() => $"{Name}: {IsSet()}";
}
