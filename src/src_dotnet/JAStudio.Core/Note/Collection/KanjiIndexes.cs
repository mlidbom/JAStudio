using System.Linq;
using Compze.Teventive.StateStore;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// What the state store files each kanji under, for <see cref="KanjiCollection"/> to look kanji up by: the kanji itself, its radicals and its<br/>
/// readings. Each index has a key type of its own, as <see cref="VocabIndexes"/> says.
/// </summary>
/// <remarks>review: the key types are nested here, as in <see cref="VocabIndexes"/>.</remarks>
static class KanjiIndexes
{
   /// <summary>Declares the kanji's indexes for the state store being composed.</summary>
   public static void DeclareIn(StateStoreBuilder store) => store
     .Index<KanjiNote, Question>(kanji => [new Question(kanji.GetQuestion())])
     .Index<KanjiNote, Radical>(kanji => kanji.Radicals.All.Distinct().Select(radical => new Radical(radical)))
     .Index<KanjiNote, Reading>(kanji => kanji.Readings.AllKana().Distinct().Select(reading => new Reading(reading)));

   /// <summary>The kanji's question: the kanji itself.</summary>
   public readonly record struct Question(string Kanji);

   /// <summary>One of the kanji's radicals.</summary>
   public readonly record struct Radical(string Kanji);

   /// <summary>One of the kanji's readings, in kana.</summary>
   public readonly record struct Reading(string Kana);
}
