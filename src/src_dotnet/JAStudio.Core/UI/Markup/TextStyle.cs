using System;

namespace JAStudio.Core.UI.Markup;

/// <summary>
/// How a run of a note's text is styled, as the markup around it in the note says (<see cref="StyledText"/>): HTML, such as <c>&lt;b&gt;</c>, and<br/>
/// JAStudio's own tags, such as <c>&lt;kanji&gt;</c>. A run inside several such elements has each of their styles.
/// </summary>
[Flags]
public enum TextStyle
{
   Plain = 0,

   /// <summary><c>&lt;b&gt;</c> or <c>&lt;strong&gt;</c>: emphasized, such as the word a sentence is about in its source question.</summary>
   Bold = 1,

   /// <summary><c>&lt;i&gt;</c> or <c>&lt;em&gt;</c>.</summary>
   Italic = 1 << 1,

   /// <summary><c>&lt;u&gt;</c>.</summary>
   Underlined = 1 << 2,

   /// <summary><c>&lt;s&gt;</c>, <c>&lt;strike&gt;</c>, <c>&lt;del&gt;</c> or <c>&lt;obsolete&gt;</c>: no longer true, such as a meaning that was wrong.</summary>
   StruckThrough = 1 << 3,

   /// <summary><c>&lt;ja&gt;</c>: Japanese inside other text, such as an English answer or explanation.</summary>
   Japanese = 1 << 4,

   /// <summary><c>&lt;tag&gt;</c>: a label inside an answer, such as <c>suff:</c> or <c>な:</c>, saying what the meaning after it is for.</summary>
   Label = 1 << 5,

   /// <summary><c>&lt;kanji&gt;</c> or <c>&lt;kan&gt;</c>: a kanji, named in a mnemonic or an explanation.</summary>
   Kanji = 1 << 6,

   /// <summary><c>&lt;radical&gt;</c> or <c>&lt;rad&gt;</c>: a kanji's radical, named in a mnemonic.</summary>
   Radical = 1 << 7,

   /// <summary><c>&lt;vocabulary&gt;</c>, <c>&lt;vocab&gt;</c> or <c>&lt;voc&gt;</c>: a vocab, named in a mnemonic or an explanation.</summary>
   Vocabulary = 1 << 8,

   /// <summary><c>&lt;reading&gt;</c> or <c>&lt;read&gt;</c>: a reading, named in a mnemonic.</summary>
   Reading = 1 << 9,

   /// <summary><c>&lt;primary&gt;</c>: what is primary among several, such as a kanji's primary reading among its readings, or its primary meaning.</summary>
   Primary = 1 << 10,

   /// <summary><c>&lt;span class="readingsSeparator"&gt;</c>: the bar between a kanji's on and kun readings, as the pages show them.</summary>
   ReadingsSeparator = 1 << 11
}
