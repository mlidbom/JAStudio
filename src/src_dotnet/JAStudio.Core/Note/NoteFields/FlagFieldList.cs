using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;

namespace JAStudio.Core.Note.NoteFields;

/// <summary>
/// The <see cref="FlagField"/>s of one kind that a note has, such as a vocab's register: the corpus stores them as the names of those that are set.
/// </summary>
class FlagFieldList
{
   readonly IReadOnlyList<FlagField> _flags;

   /// <param name="namesOfTheSetFlags">
   /// The names of the flags that the corpus has as set. Each must name one of <paramref name="flags"/>: a name none of them has<br/>
   /// would be lost when the note is next saved.
   /// </param>
   /// <param name="flags">Every flag of the kind.</param>
   public FlagFieldList(IReadOnlyCollection<string> namesOfTheSetFlags, params FlagField[] flags)
   {
      _flags = flags;
      AssertEveryNameIsOneOfTheFlags();
      return;

      void AssertEveryNameIsOneOfTheFlags()
      {
         var unknownNames = namesOfTheSetFlags.Where(name => flags.All(flag => flag.Name != name)).ToList();
         Contract.Argument.Assert(unknownNames.Count == 0, () => $"The stored flags {string.Join(", ", unknownNames)} are unknown, and would be lost when the note is saved.");
      }
   }

   /// <summary>Every flag of the kind, set or not.</summary>
   public IReadOnlyList<FlagField> All => _flags;

   /// <summary>The flags as the corpus stores them: the names of those that are set, in ordinal order, so that the same flags always write the same file.</summary>
   public List<string> NamesOfTheSetFlags() => [.. _flags.Where(it => it.IsSet()).Select(it => it.Name).Order(StringComparer.Ordinal)];

   /// <summary>Sets each flag as <paramref name="other"/>, a list of the same kind, has it.</summary>
   public void OverwriteWith(FlagFieldList other)
   {
      foreach(var (flag, otherFlag) in _flags.Zip(other._flags)) flag.SetTo(otherFlag.IsSet());
   }
}
