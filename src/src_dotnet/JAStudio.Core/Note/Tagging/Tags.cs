// ReSharper disable MemberCanBeInternal todo: review
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JAStudio.Core.Note.Tagging;

public static class Tags
{
   const string FRoot = "-::";
   const string FSentence = FRoot + "sentence::";
   const string FKanji = FRoot + "kanji::";
   const string FSentenceUses = FSentence + "uses::";
   const string FVocab = FRoot + "vocab::";
   const string FVocabMatching = FVocab + "matching::";
   const string FVocabMatchingUses = FVocabMatching + "uses::";
   const string FSource = "source::";

   public static class Sentence
   {
      public static class Uses
      {
         public static readonly Tag IncorrectMatches = Tag.FromName(FSentenceUses + "incorrect-matches");
         public static readonly Tag HiddenMatches = Tag.FromName(FSentenceUses + "hidden-matches");
      }
   }

   public static class Kanji
   {
      public static readonly Tag IsRadical = Tag.FromName(FKanji + "is-radical");
      public static readonly Tag IsRadicalPurely = Tag.FromName(FKanji + "is-radical-purely");
      public static readonly Tag IsRadicalSilent = Tag.FromName(FKanji + "is-radical-silent");
      public static readonly Tag InVocabMainForm = Tag.FromName(FKanji + "in-vocab-main-form");
      public static readonly Tag InAnyVocabForm = Tag.FromName(FKanji + "in-any-vocab-form");

      public static readonly Tag WithSingleKanjiVocab = Tag.FromName(FKanji + "single-kanji-vocab");
      public static readonly Tag WithSingleKanjiVocabWithDifferentReading = Tag.FromName(FKanji + "single-kanji-vocab-with-different-reading");
      public static readonly Tag WithNoPrimaryOnReadings = Tag.FromName(FKanji + "no-primary-on-readings");
      public static readonly Tag WithNoPrimaryReadings = Tag.FromName(FKanji + "no-primary-readings");
      public static readonly Tag WithVocabWithPrimaryOnReading = Tag.FromName(FKanji + "has-vocab-with-primary-on-reading");

      public static readonly Tag HasNonPrimaryOnReadingVocab = Tag.FromName(FKanji + "has-non-primary-on-reading-vocab");
   }

   public static class Vocab
   {
      public const string Root = FVocab; // Keep as string for startswith() checks

      public static readonly Tag IsIchidanHidingGodanPotential = Tag.FromName(FVocab + "is-ichidan-hiding-godan-potential");

      public static class Matching
      {
         public static class Uses
         {
            public static readonly Tag PrefixIsNot = Tag.FromName(FVocabMatchingUses + "prefix-is-not");
            public static readonly Tag SuffixIsNot = Tag.FromName(FVocabMatchingUses + "suffix-is-not");
            public static readonly Tag RequiredPrefix = Tag.FromName(FVocabMatchingUses + "required-prefix");
            public static readonly Tag SurfaceIsNot = Tag.FromName(FVocabMatchingUses + "surface-is-not");
         }
      }
   }

   /// <summary>
   /// The anki branch, <c>anki::…</c>: a note's Anki tags (<see cref="AnkiTags"/>), the tags its study-note has in Anki, each below <c>anki::</c>,<br/>
   /// such as <c>anki::marked</c>. They're the student's: read from Anki as the collection loads, held in memory only, and never changed in JAStudio.<br/>
   /// So no tag of the note's own can be in the branch, and the branch is never renamed or deleted. See <c>dev_docs/tags.md</c>.
   /// </summary>
   public static class Anki
   {
      public const string Folder = "anki::"; // Keep as string for startswith() checks

      /// <summary>The anki branch as a whole, <c>anki</c>.</summary>
      public static TagBranch Branch => new("anki");

      /// <summary>The tag a note shows for the tag <paramref name="nameInAnki"/> of its study-note in Anki: <c>anki::marked</c> for <c>marked</c>.</summary>
      public static Tag For(string nameInAnki) => Tag.FromName(Folder + nameInAnki);

      /// <summary>Why nothing in the anki branch can be added, removed, renamed or deleted in JAStudio.</summary>
      public const string WhyItIsReadOnly = "The tags below anki:: are the notes' tags in Anki, which JAStudio only reads: change them in Anki.";
   }

   /// <summary>The priority tags, which order a vocab's sentences (<see cref="JPNote.PriorityTagValue"/>): the student's, set in Anki, so they're Anki tags.</summary>
   public const string PriorityFolder = Anki.Folder + FRoot + "priority::"; // Keep as string for startswith() checks

   /// <summary>
   /// The source branch, <c>source::…</c>: where each note's content comes from, such as <c>source::wani::level::19</c> or <c>source::anime::natsume::s4::11</c>.<br/>
   /// It's the record of where the corpus's content comes from, for licensing, so changing a note's source tags warns, and the branch itself,<br/>
   /// <see cref="Branch"/>, is never renamed or deleted. See <c>dev_docs/tags.md</c>.
   /// </summary>
   /// <remarks>
   /// The source tags JAStudio sets when it creates a note, such as <see cref="Jamdict"/>, are added and removed by hand as any other:<br/>
   /// nothing sets them again afterwards. Only their names stay as they are (<see cref="IsDefinedByJAStudio"/>), since JAStudio creates notes with them.
   /// </remarks>
   public static class Source
   {
      public const string Folder = FSource; // Keep as string for startswith() checks

      /// <summary>The source branch as a whole, <c>source</c>, never renamed or deleted (<see cref="TagBranch.WhyItCannotBeRenamedByHand"/>).</summary>
      public static TagBranch Branch => new("source");

      /// <summary>A vocab JAStudio created from the dictionary, JMdict, which gave it its answer and readings.</summary>
      public static readonly Tag Jamdict = Tag.FromName(FSource + "jamdict");

      /// <summary>
      /// Our own content, made by Magnus or Claude: a vocab JAStudio creates when the dictionary has no entry for it, or derives from<br/>
      /// another vocab, such as its suffix version, and a kanji created by hand, such as a radical or component.
      /// </summary>
      public static readonly Tag Jastudio = Tag.FromName(FSource + "jastudio");
   }

   public static readonly Tag TTSAudio = Tag.FromName("_tts_audio");

   /// <summary>
   /// Whether the tag named <paramref name="tagName"/>, ignoring case, is a jastudio-defined-tag: one of the tags defined here,<br/>
   /// in <see cref="Tags"/> and its nested classes, whose names JAStudio's code uses. So they, and the branches holding them, aren't renamed by hand:<br/>
   /// JAStudio would go on using the old name. Most of them JAStudio also maintains (<see cref="IsMaintainedByJAStudio"/>).
   /// </summary>
   public static bool IsDefinedByJAStudio(string tagName) => NamesOfTheTagsDefinedByJAStudio.Value.Contains(tagName);

   /// <summary>
   /// Whether the tag named <paramref name="tagName"/>, ignoring case, is a jastudio-maintained-tag: a jastudio-defined-tag JAStudio keeps up to date<br/>
   /// itself. Most are generated by every full rebuild, such as <see cref="Kanji.InAnyVocabForm"/>, and <see cref="TTSAudio"/> is set by an editor<br/>
   /// of its own. So they aren't added to or removed from notes by hand, nor deleted: a hand edit would be overwritten, or contradict that editor.<br/>
   /// The source tags JAStudio sets when it creates a note (<see cref="Source"/>) aren't maintained: nothing sets them again.
   /// </summary>
   public static bool IsMaintainedByJAStudio(string tagName) => NamesOfTheTagsMaintainedByJAStudio.Value.Contains(tagName);

   /// <summary>
   /// Why <paramref name="tagName"/> can't be a tag a person adds to or removes from notes, or null when it can: it's not a valid tag name<br/>
   /// (<see cref="Tag.WhyItCannotBeTheNameOfATag"/>), or it's a tag JAStudio maintains (<see cref="IsMaintainedByJAStudio"/>).
   /// </summary>
   public static string? WhyItCannotBeATagAPersonManages(string tagName) =>
      Tag.WhyItCannotBeTheNameOfATag(tagName)
      ?? (IsMaintainedByJAStudio(tagName) ? $"{tagName} is maintained by JAStudio, which generates it or has an editor of its own for it." : null);

   /// <summary>Read on first use, by when every tag field here has its value.</summary>
   static readonly Lazy<HashSet<string>> NamesOfTheTagsDefinedByJAStudio = new(() => new HashSet<string>(NamesOfTheTagsDeclaredIn(typeof(Tags)), StringComparer.OrdinalIgnoreCase));

   /// <summary>Read on first use, by when every tag field here has its value.</summary>
   static readonly Lazy<HashSet<string>> NamesOfTheTagsMaintainedByJAStudio =
      new(() => new HashSet<string>(NamesOfTheTagsDefinedByJAStudio.Value.Except(NamesOfTheTagsDeclaredIn(typeof(Source)), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase));

   static IEnumerable<string> NamesOfTheTagsDeclaredIn(Type type) =>
   [
      ..type.GetFields(BindingFlags.Public | BindingFlags.Static).Select(it => it.GetValue(null)).OfType<Tag>().Select(it => it.Name),
      ..type.GetNestedTypes().SelectMany(NamesOfTheTagsDeclaredIn)
   ];
}
