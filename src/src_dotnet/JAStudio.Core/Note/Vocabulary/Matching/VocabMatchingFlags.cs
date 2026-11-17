using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>The yes-or-no settings of a vocab's matching configuration, such as whether it's a poison word.</summary>
/// <remarks>
/// A change publishes <see cref="IVocabTevent.MatchingConfiguration.PropertyUpdated.NamesOfTheSetFlags"/>, which the matching configuration's applier<br/>
/// applies (<see cref="Apply"/>).
/// </remarks>
public class VocabMatchingFlags
{
   readonly FlagFieldList _all;

   /// <param name="vocab">The vocab it's of.</param>
   /// <param name="partsOfSpeech">The vocab's parts of speech, some of which make it an inflecting word without the flag.</param>
   /// <param name="namesOfTheSetFlags">The names of the flags that the corpus has as set.</param>
   internal VocabMatchingFlags(VocabNote vocab, VocabNotePartsOfSpeech partsOfSpeech, IReadOnlyCollection<string> namesOfTheSetFlags)
   {
      _all = new FlagFieldList(vocab, namesOfTheSetFlags, static it => new VocabTevent.MatchingConfiguration.NamesOfTheSetFlagsUpdated(it));
      IsInflectingWord = new IsInflectingWord(partsOfSpeech, _all);
      IsPoisonWord = Flag("isPoisonWord");
      MatchWithPrecedingVowel = Flag("matchWithPrecedingVowel");
      QuestionOverridesForm = Flag("questionOverridesForm");
      IsCompositionallyTransparentCompound = Flag("isCompositionallyTransparentCompound");
      _all.AssertEveryNameIsOneOf(IsInflectingWord, IsPoisonWord, MatchWithPrecedingVowel, QuestionOverridesForm, IsCompositionallyTransparentCompound);
      return;

      FlagField Flag(string name) => new(name, _all);
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

   /// <summary>The flags as the corpus stores them: see <see cref="FlagFieldList.NamesOfTheSetFlags"/>.</summary>
   internal List<string> ToDto() => [.. _all.NamesOfTheSetFlags];

   /// <summary>Sets each flag as <paramref name="other"/> has it.</summary>
   internal void OverwriteWith(VocabMatchingFlags other) => _all.OverwriteWith(other._all);

   /// <summary>Makes the flags named <paramref name="namesOfTheSetFlags"/> those that are set: what the flags' tevent's applier does.</summary>
   internal void Apply(IReadOnlyList<string> namesOfTheSetFlags) => _all.Apply(namesOfTheSetFlags);
}
