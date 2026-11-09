using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.KanjiReadings;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Editing;

/// <summary>
/// The tools that edit a kanji as its editors in the workspace do: through the kanji itself, so each edit is saved to the corpus checkout, and<br/>
/// passed on to the kanji's Anki note, like any edit.
/// </summary>
[McpServerToolType]
public class KanjiEditingTools
{
   const string KindDescription = "The kind of reading: \"on\", \"kun\" or \"nanori\".";
   const string KanaDescription = "The reading, in kana. An on reading may be given in katakana or hiragana: on readings are kept in hiragana.";
   const string KanjiNoteIdDescription = "The id of the kanji, as search_notes lists it.";

   static readonly IReadOnlyDictionary<string, KanjiReadingKind> KindsByName = new Dictionary<string, KanjiReadingKind>
   {
      ["on"] = KanjiReadingKind.On,
      ["kun"] = KanjiReadingKind.Kun,
      ["nanori"] = KanjiReadingKind.Nanori
   };

   readonly JPCollection _collection;

   public KanjiEditingTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "add_a_reading_to_a_kanji", Destructive = false, Idempotent = true)]
   [Description("""
                Adds a reading to a kanji, last among its readings of that kind, as a secondary reading, as adding one in its editor in the workspace
                does. Adding a reading the kanji has already changes nothing. The kanji is saved to the corpus checkout, and its Anki note updated,
                as any edit is. A kanji's readings decide which part of its vocab's readings its page highlights, and its primary readings are the
                ones it's studied by. Reports the kanji's readings before and after.
                """)]
   public KanjiReadingsChange AddAReadingToAKanji(
      [Description(KanjiNoteIdDescription)] string kanjiNoteId,
      [Description(KindDescription)] string kind,
      [Description(KanaDescription)] string kana) =>
      ChangeTheReadings(kanjiNoteId, kind, readings => readings.Add(kana));

   [McpServerTool(Name = "remove_a_reading_from_a_kanji", Destructive = true, Idempotent = true)]
   [Description("""
                Removes a reading from a kanji, as removing one in its editor in the workspace does. Removing a reading the kanji doesn't have
                fails, changing nothing. The kanji is saved to the corpus checkout, and its Anki note updated, as any edit is. Reports the kanji's
                readings before and after.
                """)]
   public KanjiReadingsChange RemoveAReadingFromAKanji(
      [Description(KanjiNoteIdDescription)] string kanjiNoteId,
      [Description(KindDescription)] string kind,
      [Description(KanaDescription)] string kana) =>
      ChangeTheReadings(kanjiNoteId, kind, readings => readings.Remove(kana));

   [McpServerTool(Name = "set_whether_a_kanji_reading_is_primary", Destructive = false, Idempotent = true)]
   [Description("""
                Makes one of a kanji's readings primary or secondary, as toggling it in its editor in the workspace does. The primary readings are
                the ones the kanji is studied by: its default mnemonic is made of them, and its page highlights them in its vocab's readings, the
                secondary ones in another style. Setting it for a reading the kanji doesn't have fails, changing nothing. The kanji is saved to the
                corpus checkout, and its Anki note updated, as any edit is. Reports the kanji's readings before and after.
                """)]
   public KanjiReadingsChange SetWhetherAKanjiReadingIsPrimary(
      [Description(KanjiNoteIdDescription)] string kanjiNoteId,
      [Description(KindDescription)] string kind,
      [Description(KanaDescription)] string kana,
      [Description("True to make the reading primary, false to make it secondary.")] bool isPrimary) =>
      ChangeTheReadings(kanjiNoteId, kind, readings =>
      {
         if(isPrimary) readings.MakePrimary(kana);
         else readings.MakeSecondary(kana);
      });

   KanjiReadingsChange ChangeTheReadings(string kanjiNoteId, string kind, Action<KanjiReadingList> change)
   {
      var kanji = KanjiWithTheId(kanjiNoteId);
      var readings = kanji.Readings.OfKind(KindNamed(kind));

      var before = KanjiReadingsReported.Of(kanji.Readings);
      change(readings);
      return new KanjiReadingsChange(kanji.GetQuestion(), before, KanjiReadingsReported.Of(kanji.Readings));
   }

   static KanjiReadingKind KindNamed(string name) =>
      KindsByName.TryGetValue(name.ToLowerInvariant(), out var kind)
         ? kind
         : throw new ArgumentException($"There is no kind of kanji reading named '{name}'. The kinds are: {string.Join(", ", KindsByName.Keys.Select(it => $"\"{it}\""))}.");

   KanjiNote KanjiWithTheId(string noteId) =>
      _collection.Kanji.WithIdOrNone(NoteId.Parse(noteId)) ?? throw new ArgumentException($"No kanji has the id {noteId}.", nameof(noteId));
}
