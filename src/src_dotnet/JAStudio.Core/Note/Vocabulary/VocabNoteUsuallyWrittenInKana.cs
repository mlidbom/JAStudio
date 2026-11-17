using System;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// Whether a vocab is usually written in kana only, such as 為る, usually written する: yes, no, or not decided yet (<see cref="Decision"/>).<br/>
/// A yes is shown as "uk" on the vocab page and in the sentence breakdown.
/// </summary>
/// <remarks>
/// While it's undecided, JAStudio may decide it as yes: when it generates the vocab's data (<see cref="VocabNoteGeneratedData"/>) and the vocab's question<br/>
/// is all kana and it has no readings yet, or when it creates a vocab without parts of speech (<see cref="VocabNoteFactory"/>) and the dictionary says<br/>
/// the word is usually written in kana.<br/>
/// Once decided, by hand or by JAStudio, it's never changed automatically, so a no keeps JAStudio from marking the vocab when it gets it wrong.
/// </remarks>
/// <remarks>A component of its vocab, whose decision publishes a tevent of its own, <see cref="IVocabTevent.UsuallyWrittenInKana"/>, which its applier applies.</remarks>
public class VocabNoteUsuallyWrittenInKana : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, VocabNoteUsuallyWrittenInKana, IVocabTevent.UsuallyWrittenInKana, VocabTevent.UsuallyWrittenInKana>, ITeventRouted
{
   static readonly TeventRouting<VocabNoteUsuallyWrittenInKana, IVocabTevent.UsuallyWrittenInKana> Appliers = DeclareAppliers(it => it
     .For<IVocabTevent.UsuallyWrittenInKana.PropertyUpdated.Decision>(static (usuallyWrittenInKana, tevent) => usuallyWrittenInKana.Decision = tevent.Decision));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly VocabNote _vocab;

   /// <param name="vocab">The vocab it's of.</param>
   /// <param name="stored">The <see cref="Decision"/> as the corpus stores it: see <see cref="ToDto"/>.</param>
   internal VocabNoteUsuallyWrittenInKana(VocabNote vocab, bool? stored) : base(vocab)
   {
      _vocab = vocab;
      Decision = DecisionStoredAs(stored);
   }

   public UsuallyWrittenInKanaDecision Decision { get; private set; }

   /// <summary>Whether the vocab is usually written in kana: only once it's decided as <see cref="UsuallyWrittenInKanaDecision.Yes"/>.</summary>
   public bool IsYes => Decision == UsuallyWrittenInKanaDecision.Yes;

   /// <summary>Decides it as <paramref name="decision"/> says, or with <see cref="UsuallyWrittenInKanaDecision.Undecided"/> makes it undecided again.</summary>
   public void Decide(UsuallyWrittenInKanaDecision decision) =>
      _vocab.PublishIfChanged(Decision, decision, static it => new VocabTevent.UsuallyWrittenInKana.DecisionUpdated(it));

   /// <summary>Decides it as yes, unless it's decided already: how JAStudio decides it automatically.</summary>
   internal void DecideAsYesUnlessDecided()
   {
      if(Decision == UsuallyWrittenInKanaDecision.Undecided) Decide(UsuallyWrittenInKanaDecision.Yes);
   }

   /// <summary>The <see cref="Decision"/> as the corpus stores it: true for yes, false for no, and null while it's undecided.</summary>
   internal bool? ToDto() => Decision switch
   {
      UsuallyWrittenInKanaDecision.Yes => true,
      UsuallyWrittenInKanaDecision.No => false,
      UsuallyWrittenInKanaDecision.Undecided => null,
      _ => throw new ArgumentOutOfRangeException(nameof(Decision), Decision, null)
   };

   static UsuallyWrittenInKanaDecision DecisionStoredAs(bool? stored) => stored switch
   {
      true => UsuallyWrittenInKanaDecision.Yes,
      false => UsuallyWrittenInKanaDecision.No,
      null => UsuallyWrittenInKanaDecision.Undecided
   };
}
