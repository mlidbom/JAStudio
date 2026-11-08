using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A vocab's register: the kind of speech it belongs to, such as polite, slang or archaic, each a flag of its own, since several may apply.<br/>
/// Shown on the vocab's card. It doesn't affect matching.
/// </summary>
public class VocabNoteRegister
{
   readonly FlagFieldList _all;

   /// <param name="namesOfTheSetFlags">The registers the corpus has the vocab in, by name.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabNoteRegister(IReadOnlyCollection<string> namesOfTheSetFlags, NoteGuard guard)
   {
      Polite = Flag("polite");
      Formal = Flag("formal");
      Informal = Flag("informal");
      Archaic = Flag("archaic");
      Sensitive = Flag("sensitive");
      Vulgar = Flag("vulgar");
      Humble = Flag("humble");
      Literary = Flag("literary");
      Honorific = Flag("honorific");
      RoughMasculine = Flag("roughMasculine");
      SoftFeminine = Flag("softFeminine");
      Slang = Flag("slang");
      Derogatory = Flag("derogatory");
      Childish = Flag("childish");
      _all = new FlagFieldList(namesOfTheSetFlags,
                               Polite, Formal, Informal, Archaic, Sensitive, Vulgar, Humble,
                               Literary, Honorific, RoughMasculine, SoftFeminine, Slang, Derogatory, Childish);
      return;

      FlagField Flag(string name) => new(name, namesOfTheSetFlags, guard);
   }

   public FlagField Polite { get; }
   public FlagField Formal { get; }
   public FlagField Informal { get; }
   public FlagField Archaic { get; }
   public FlagField Sensitive { get; }
   public FlagField Vulgar { get; }
   public FlagField Humble { get; }
   public FlagField Literary { get; }
   public FlagField Honorific { get; }
   public FlagField RoughMasculine { get; }
   public FlagField SoftFeminine { get; }
   public FlagField Slang { get; }
   public FlagField Derogatory { get; }
   public FlagField Childish { get; }

   /// <summary>The register as the corpus stores it: see <see cref="FlagFieldList.NamesOfTheSetFlags"/>.</summary>
   internal List<string> ToDto() => _all.NamesOfTheSetFlags();

   /// <summary>Puts the vocab in the registers <paramref name="other"/> is in, and only those.</summary>
   internal void OverwriteWith(VocabNoteRegister other) => _all.OverwriteWith(other._all);
}
