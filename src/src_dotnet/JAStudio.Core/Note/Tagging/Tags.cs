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

   public const string PriorityFolder = FRoot + "priority::"; // Keep as string for startswith() checks

   public static class Source
   {
      public const string Folder = FSource; // Keep as string for startswith() checks

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
   /// in <see cref="Tags"/> and its nested classes. Most are generated, such as <see cref="Kanji.InAnyVocabForm"/>, and a few hold data JAStudio reads,<br/>
   /// such as <see cref="TTSAudio"/>. So they aren't added to or removed from notes by hand: a generated tag would be overwritten.
   /// </summary>
   public static bool IsDefinedByJAStudio(string tagName) => NamesOfTheTagsDefinedByJAStudio.Value.Contains(tagName);

   /// <summary>Read on first use, by when every tag field here has its value.</summary>
   static readonly Lazy<HashSet<string>> NamesOfTheTagsDefinedByJAStudio = new(() => new HashSet<string>(NamesOfTheTagsDeclaredIn(typeof(Tags)), StringComparer.OrdinalIgnoreCase));

   static IEnumerable<string> NamesOfTheTagsDeclaredIn(Type type) =>
   [
      ..type.GetFields(BindingFlags.Public | BindingFlags.Static).Select(it => it.GetValue(null)).OfType<Tag>().Select(it => it.Name),
      ..type.GetNestedTypes().SelectMany(NamesOfTheTagsDeclaredIn)
   ];
}
