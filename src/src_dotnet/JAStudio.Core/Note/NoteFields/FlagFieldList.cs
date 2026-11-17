using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.NoteFields;

/// <summary>
/// The <see cref="FlagField"/>s of one kind that a note has, such as a vocab's register, and which of them are set: the corpus stores them as the<br/>
/// names of those that are set (<see cref="NamesOfTheSetFlags"/>), which is also what a change to one of them publishes.
/// </summary>
class FlagFieldList
{
   readonly JPNote _note;
   readonly Func<IReadOnlyList<string>, NoteTevent> _teventOf;

   /// <param name="note">The note whose flags they are, which publishes their tevent.</param>
   /// <param name="namesOfTheSetFlags">The names of the flags that the corpus has as set: see <see cref="AssertEveryNameIsOneOf"/>.</param>
   /// <param name="teventOf">The tevent that the flags of the kind are now those that the names it's given name.</param>
   public FlagFieldList(JPNote note, IReadOnlyCollection<string> namesOfTheSetFlags, Func<IReadOnlyList<string>, NoteTevent> teventOf)
   {
      _note = note;
      _teventOf = teventOf;
      NamesOfTheSetFlags = InTheOrderTheCorpusStoresThem(namesOfTheSetFlags);
   }

   /// <summary>The names of the flags that are set, as the corpus stores them: in ordinal order, so that the same flags always write the same file.</summary>
   public IReadOnlyList<string> NamesOfTheSetFlags { get; private set; }

   /// <summary>
   /// Asserts that each name the corpus has as set is one of <paramref name="flags"/>, every flag of the kind: a name none of them has would be lost<br/>
   /// when the note is next saved.
   /// </summary>
   public void AssertEveryNameIsOneOf(params FlagField[] flags)
   {
      var unknownNames = NamesOfTheSetFlags.Where(name => flags.All(flag => flag.Name != name)).ToList();
      Contract.Argument.Assert(unknownNames.Count == 0, () => $"The stored flags {string.Join(", ", unknownNames)} are unknown, and would be lost when the note is saved.");
   }

   public bool IsSet(FlagField flag) => NamesOfTheSetFlags.Contains(flag.Name);

   /// <summary>Sets or clears <paramref name="flag"/>: a change publishes the flags' tevent.</summary>
   public void SetTo(FlagField flag, bool set) => SetTheFlagsNamed(set ? [.. NamesOfTheSetFlags, flag.Name] : NamesOfTheSetFlags.Where(it => it != flag.Name));

   /// <summary>Sets each flag as <paramref name="other"/>, a list of the same kind, has it.</summary>
   public void OverwriteWith(FlagFieldList other) => SetTheFlagsNamed(other.NamesOfTheSetFlags);

   /// <summary>Makes the flags named <paramref name="namesOfTheSetFlags"/> those that are set: what the flags' tevent's applier does.</summary>
   public void Apply(IReadOnlyList<string> namesOfTheSetFlags) => NamesOfTheSetFlags = namesOfTheSetFlags;

   void SetTheFlagsNamed(IEnumerable<string> names) => _note.PublishIfTheListChanged(NamesOfTheSetFlags, InTheOrderTheCorpusStoresThem(names), _teventOf);

   static List<string> InTheOrderTheCorpusStoresThem(IEnumerable<string> names) => [.. names.Distinct().Order(StringComparer.Ordinal)];
}
