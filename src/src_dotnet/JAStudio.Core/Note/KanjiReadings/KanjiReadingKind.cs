namespace JAStudio.Core.Note.KanjiReadings;

/// <summary>The kinds of reading a kanji has, each kept in its own <see cref="KanjiReadingList"/> of the kanji's <see cref="KanjiNoteReadings"/>.</summary>
public enum KanjiReadingKind
{
   /// <summary>An on reading (on'yomi): one that came from Chinese with the kanji, mostly used in words of several kanji.</summary>
   On,

   /// <summary>A kun reading (kun'yomi): a native Japanese word the kanji was given to write, mostly used where the kanji stands alone or with okurigana.</summary>
   Kun,

   /// <summary>A nanori reading: one used only in names.</summary>
   Nanori
}
