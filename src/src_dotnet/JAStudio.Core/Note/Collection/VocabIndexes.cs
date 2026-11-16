using System.Collections.Generic;
using System.Linq;
using Compze.Teventive.StateStore;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// What the state store files each vocab under, for <see cref="VocabCollection"/> to look vocab up by: its question, its forms and their starts,<br/>
/// its compound parts, the word it is derived from, its kanji, its readings and its stems. Each index has a key type of its own, such as<br/>
/// <see cref="Form"/>, since the store has one index for each class and key type.
/// </summary>
/// <remarks>
/// review: the key types are nested here, in one file, against the standard of a file for each type: each is a line that means nothing apart<br/>
/// from its index, and what a vocab is filed under reads best in one place.
/// </remarks>
static class VocabIndexes
{
   /// <summary>Declares the vocab's indexes for the state store being composed.</summary>
   public static void DeclareIn(StateStoreBuilder store) => store
     .Index<VocabNote, Question>(vocab => [new Question(vocab.GetQuestion())])
     .Index<VocabNote, ReferenceNamingIt>(vocab => vocab.Question.HasText ? [new ReferenceNamingIt(vocab.Question.Reference)] : [])
     .Index<VocabNote, Form>(vocab => vocab.Forms.MatchedFormsList().Distinct().Select(form => new Form(form)))
     .Index<VocabNote, FormStart>(vocab => vocab.Forms.MatchedFormsList().SelectMany(StartsOf).Distinct().Select(start => new FormStart(start)))
     .Index<VocabNote, CompoundPart>(vocab => vocab.CompoundParts.All().Distinct().Select(part => new CompoundPart(part)))
     .Index<VocabNote, DerivedFrom>(vocab => vocab.RelatedNotes.DerivedFrom.Word is {} word ? [new DerivedFrom(word)] : [])
     .Index<VocabNote, KanjiInTheMainForm>(vocab => vocab.Kanji.ExtractMainFormKanji().Distinct().Select(kanji => new KanjiInTheMainForm(kanji)))
     .Index<VocabNote, KanjiInAnyForm>(vocab => vocab.Kanji.ExtractAllKanji().Distinct().Select(kanji => new KanjiInAnyForm(kanji)))
     .Index<VocabNote, Reading>(vocab => vocab.GetReadings().Distinct().Select(reading => new Reading(reading)))
     .Index<VocabNote, Stem>(vocab => vocab.Conjugator.GetStemsForPrimaryForm().Distinct().Select(stem => new Stem(stem)));

   /// <summary>Every start of <paramref name="form"/>: its first character, its first two, and so on to the whole form. 行く gives 行 and 行く.</summary>
   static IEnumerable<string> StartsOf(string form) => Enumerable.Range(1, form.Length).Select(length => form[..length]);

   /// <summary>The vocab's question (<see cref="JPNote.GetQuestion"/>).</summary>
   public readonly record struct Question(string Text);

   /// <summary>The word-reference naming the vocab exactly (<see cref="VocabNoteQuestion.Reference"/>): none while it's being created, without a question yet.</summary>
   public readonly record struct ReferenceNamingIt(WordReference Reference);

   /// <summary>One of the vocab's forms (<see cref="VocabNoteForms.MatchedFormsList"/>).</summary>
   public readonly record struct Form(string Text);

   /// <summary>A start of one of the vocab's forms (<see cref="StartsOf"/>).</summary>
   public readonly record struct FormStart(string Text);

   /// <summary>One of the vocab's compound parts.</summary>
   public readonly record struct CompoundPart(WordReference Part);

   /// <summary>The word the vocab is derived from, if one is given.</summary>
   public readonly record struct DerivedFrom(WordReference Word);

   /// <summary>A kanji of the vocab's main form.</summary>
   public readonly record struct KanjiInTheMainForm(string Kanji);

   /// <summary>A kanji of any of the vocab's forms.</summary>
   public readonly record struct KanjiInAnyForm(string Kanji);

   /// <summary>One of the vocab's readings.</summary>
   public readonly record struct Reading(string Kana);

   /// <summary>A stem of the vocab's primary form.</summary>
   public readonly record struct Stem(string Text);
}
