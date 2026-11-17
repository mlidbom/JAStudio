using System.Collections.Generic;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A vocab's register: the kind of speech it belongs to, such as polite, slang or archaic, each a flag of its own, since several may apply.<br/>
/// Shown on the vocab's card. It doesn't affect matching.
/// </summary>
/// <remarks>A component of its vocab, whose flags publish a tevent of their own, <see cref="IVocabTevent.Register"/>, which its applier applies.</remarks>
public class VocabNoteRegister : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, VocabNoteRegister, IVocabTevent.Register, VocabTevent.Register>, ITeventRouted
{
   static readonly TeventRouting<VocabNoteRegister> Appliers = TeventRouting<VocabNoteRegister>.DeclareAppliers(it => it
     .For<IVocabTevent.Register.PropertyUpdated.NamesOfTheSetFlags>(static (register, tevent) => register._all.Apply(tevent.NamesOfTheSetFlags)));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly FlagFieldList _all;

   /// <param name="vocab">The vocab it's of.</param>
   /// <param name="namesOfTheSetFlags">The registers the corpus has the vocab in, by name.</param>
   internal VocabNoteRegister(VocabNote vocab, IReadOnlyCollection<string> namesOfTheSetFlags) : base(vocab)
   {
      _all = new FlagFieldList(vocab, namesOfTheSetFlags, static it => new VocabTevent.Register.NamesOfTheSetFlagsUpdated(it));
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
      _all.AssertEveryNameIsOneOf(Polite, Formal, Informal, Archaic, Sensitive, Vulgar, Humble,
                                  Literary, Honorific, RoughMasculine, SoftFeminine, Slang, Derogatory, Childish);
      return;

      FlagField Flag(string name) => new(name, _all);
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
   internal List<string> ToDto() => [.. _all.NamesOfTheSetFlags];

   /// <summary>Puts the vocab in the registers <paramref name="other"/> is in, and only those.</summary>
   internal void OverwriteWith(VocabNoteRegister other) => _all.OverwriteWith(other._all);
}
