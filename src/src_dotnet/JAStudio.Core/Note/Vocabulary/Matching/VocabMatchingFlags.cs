using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>The yes-or-no settings of a vocab's matching configuration, such as whether it's a poison word.</summary>
public class VocabMatchingFlags
{
   readonly FlagFieldList _all;

   /// <param name="partsOfSpeech">The vocab's parts of speech, some of which make it an inflecting word without the flag.</param>
   /// <param name="namesOfTheSetFlags">The names of the flags that the corpus has as set.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabMatchingFlags(VocabNotePartsOfSpeech partsOfSpeech, IReadOnlyCollection<string> namesOfTheSetFlags, NoteGuard guard)
   {
      IsInflectingWord = new IsInflectingWord(partsOfSpeech, namesOfTheSetFlags, guard);
      IsPoisonWord = Flag("isPoisonWord");
      MatchWithPrecedingVowel = Flag("matchWithPrecedingVowel");
      QuestionOverridesForm = Flag("questionOverridesForm");
      IsCompositionallyTransparentCompound = Flag("isCompositionallyTransparentCompound");
      _all = new FlagFieldList(namesOfTheSetFlags, IsInflectingWord, IsPoisonWord, MatchWithPrecedingVowel, QuestionOverridesForm, IsCompositionallyTransparentCompound);
      return;

      FlagField Flag(string name) => new(name, namesOfTheSetFlags, guard);
   }

   public IsInflectingWord IsInflectingWord { get; }

   /// <summary>Every match to the vocab fails, so a word is never parsed as it.</summary>
   public FlagField IsPoisonWord { get; }

   /// <summary>A to-do: the vocab should match together with the vowel before it, which matching doesn't do yet. Nothing reads the flag.</summary>
   public FlagField MatchWithPrecedingVowel { get; }

   /// <summary>A word that matches another of the vocab's forms is shown, and placed in the sentence, as the vocab's question.</summary>
   public FlagField QuestionOverridesForm { get; }

   /// <summary>
   /// The vocab is a compound whose meaning follows from its parts, such as 外出中. While the setting to hide such compounds is on,<br/>
   /// the sentence breakdown hides a match to it whose parts each match on their own.
   /// </summary>
   public FlagField IsCompositionallyTransparentCompound { get; }

   /// <summary>Every flag, set or not, in the order above.</summary>
   public IReadOnlyList<FlagField> All => _all.All;

   /// <summary>The flags as the corpus stores them: see <see cref="FlagFieldList.NamesOfTheSetFlags"/>.</summary>
   internal List<string> ToDto() => _all.NamesOfTheSetFlags();

   /// <summary>Sets each flag as <paramref name="other"/> has it.</summary>
   internal void OverwriteWith(VocabMatchingFlags other) => _all.OverwriteWith(other._all);
}
