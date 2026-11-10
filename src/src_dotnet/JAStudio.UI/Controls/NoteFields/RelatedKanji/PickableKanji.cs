using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;

namespace JAStudio.UI.Controls.NoteFields.RelatedKanji;

/// <summary>
/// A kanji a <see cref="KanjiListEditor"/> offers for adding, as its box lists it: by its character and its answer. Typing the character, part of one of<br/>
/// its readings, or part of its answer finds it, so 走 is found by typing はし or run too.
/// </summary>
sealed class PickableKanji
{
   readonly string[] _characterAndReadings;

   PickableKanji(KanjiNote kanji)
   {
      Kanji = kanji.GetQuestion();
      Answer = kanji.AnswerText;
      _characterAndReadings = [Kanji, .. kanji.Readings.AllKana()];
   }

   /// <summary>The kanji's character: what's added when it's picked.</summary>
   public string Kanji { get; }

   /// <summary>The kanji's answer, without its markup, shown beside its character.</summary>
   public string Answer { get; }

   /// <summary>Whether <paramref name="typed"/> is the kanji's character, part of one of its readings, or part of its answer, in upper or lower case.</summary>
   public bool IsFoundBy(string typed) => _characterAndReadings.Any(it => it.Contains(typed, StringComparison.Ordinal)) || Answer.Contains(typed, StringComparison.OrdinalIgnoreCase);

   /// <summary>Every kanji of <paramref name="collection"/>, in the order of their characters.</summary>
   public static IReadOnlyList<PickableKanji> AllIn(KanjiCollection collection) => [.. collection.All().Select(it => new PickableKanji(it)).OrderBy(it => it.Kanji, StringComparer.Ordinal)];
}
