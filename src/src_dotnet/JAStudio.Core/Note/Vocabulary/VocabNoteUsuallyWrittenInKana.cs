using System;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// Whether a vocab is usually written in kana only, such as 為る, usually written する: yes, no, or not decided yet (<see cref="Decision"/>).<br/>
/// A yes is shown as "uk" on the vocab page and in the sentence breakdown.
/// </summary>
/// <remarks>
/// While it's undecided, generating the vocab's data (<see cref="VocabNoteGeneratedData"/>) may decide it as yes: when the vocab's question is all kana<br/>
/// and it has no readings yet, or when the dictionary says the word is usually written in kana and the vocab has no parts of speech yet.<br/>
/// The dictionary's half never runs today, a known bug: an empty parts of speech parses as one empty part of speech, never as none.<br/>
/// Once decided, by hand or by JAStudio, it's never changed automatically, so a no keeps JAStudio from marking the vocab when it gets it wrong.
/// </remarks>
public class VocabNoteUsuallyWrittenInKana
{
   readonly NoteGuard _guard;

   /// <param name="stored">The <see cref="Decision"/> as the corpus stores it: see <see cref="ToDto"/>.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabNoteUsuallyWrittenInKana(bool? stored, NoteGuard guard)
   {
      Decision = DecisionStoredAs(stored);
      _guard = guard;
   }

   public UsuallyWrittenInKanaDecision Decision { get; private set; }

   /// <summary>Whether the vocab is usually written in kana: only once it's decided as <see cref="UsuallyWrittenInKanaDecision.Yes"/>.</summary>
   public bool IsYes => Decision == UsuallyWrittenInKanaDecision.Yes;

   /// <summary>Decides it as <paramref name="decision"/> says, or with <see cref="UsuallyWrittenInKanaDecision.Undecided"/> makes it undecided again, and saves the vocab.</summary>
   public void Decide(UsuallyWrittenInKanaDecision decision) => _guard.Update(() => Decision = decision);

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
