// ReSharper disable MemberCanBeInternal todo: review
namespace JAStudio.Core.Note;

public static class Tags
{
   const string FRoot = "-::";
   const string FSentence = FRoot + "sentence::";
   const string FKanji = FRoot + "kanji::";
   const string FSentenceUses = FSentence + "uses::";
   const string FVocab = FRoot + "vocab::";
   const string FVocabMatching = FVocab + "matching::";
   const string FVocabRegister = FVocab + "register::";
   const string FVocabMatchingRequires = FVocabMatching + "requires::";
   const string FVocabMatchingForbids = FVocabMatching + "forbids::";
   const string FVocabMatchingTodo = FVocabMatching + "todo::";
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
      public static readonly Tag WithStudyingSingleKanjiVocabWithDifferentReading = Tag.FromName(FKanji + "studying-single-kanji-vocab-with-different-reading");
      public static readonly Tag WithNoPrimaryOnReadings = Tag.FromName(FKanji + "no-primary-on-readings");
      public static readonly Tag WithNoPrimaryReadings = Tag.FromName(FKanji + "no-primary-readings");
      public static readonly Tag WithStudyingVocab = Tag.FromName(FKanji + "studying-vocab");
      public static readonly Tag WithVocabWithPrimaryOnReading = Tag.FromName(FKanji + "has-vocab-with-primary-on-reading");
      public static readonly Tag WithStudyingVocabWithPrimaryOnReading = Tag.FromName(FKanji + "studying-vocab-with-primary-on-reading");

      public static readonly Tag HasStudyingVocabWithNoMatchingPrimaryReading = Tag.FromName(FKanji + "has-studying-vocab-with-no-matching-primary-reading");
      public static readonly Tag HasStudyingVocabForEachPrimaryReading = Tag.FromName(FKanji + "has-studying-vocab-for-each-primary-reading");
      public static readonly Tag HasPrimaryReadingWithNoStudyingVocab = Tag.FromName(FKanji + "has-primary-reading-with-no-studying-vocab");
      public static readonly Tag HasNonPrimaryOnReadingVocab = Tag.FromName(FKanji + "has-non-primary-on-reading-vocab");
      public static readonly Tag HasNonPrimaryOnReadingVocabWithOnlyKnownKanji = Tag.FromName(FKanji + "has-non-primary-on-reading-vocab-with-only-known-kanji");
   }

   public static class Vocab
   {
      public const string Root = FVocab; // Keep as string for startswith() checks

      public static readonly Tag HasNoStudyingSentences = Tag.FromName(FVocab + "has-no-studying-sentences");
      public static readonly Tag QuestionOverridesForm = Tag.FromName(FVocab + "question-overrides-form");
      public static readonly Tag IsCompositionallyTransparentCompound = Tag.FromName(FVocab + "is-compositionally-transparent-compound");
      public static readonly Tag IsIchidanHidingGodanPotential = Tag.FromName(FVocab + "is-ichidan-hiding-godan-potential");

      public static class Register
      {
         public static readonly Tag Polite = Tag.FromName(FVocabRegister + "polite");
         public static readonly Tag Formal = Tag.FromName(FVocabRegister + "formal");
         public static readonly Tag Humble = Tag.FromName(FVocabRegister + "humble");
         public static readonly Tag Honorific = Tag.FromName(FVocabRegister + "honorific");
         public static readonly Tag Informal = Tag.FromName(FVocabRegister + "informal");
         public static readonly Tag Slang = Tag.FromName(FVocabRegister + "slang");
         public static readonly Tag RoughMasculine = Tag.FromName(FVocabRegister + "rough");
         public static readonly Tag SoftFeminine = Tag.FromName(FVocabRegister + "soft");
         public static readonly Tag Derogatory = Tag.FromName(FVocabRegister + "derogatory");
         public static readonly Tag Vulgar = Tag.FromName(FVocabRegister + "vulgar");
         public static readonly Tag Archaic = Tag.FromName(FVocabRegister + "archaic");
         public static readonly Tag Sensitive = Tag.FromName(FVocabRegister + "sensitive");
         public static readonly Tag Childish = Tag.FromName(FVocabRegister + "childish");
         public static readonly Tag Literary = Tag.FromName(FVocabRegister + "literary");
      }

      public static class Matching
      {
         public static readonly Tag YieldLastTokenToOverlappingCompound = Tag.FromName(FVocabMatching + "yield-last-token-to-upcoming-compound");
         public static readonly Tag IsPoisonWord = Tag.FromName(FVocabMatching + "is-poison-word");
         public static readonly Tag IsInflectingWord = Tag.FromName(FVocabMatching + "is-inflecting-word");

         public static class Requires
         {
            public const string FolderName = FVocabMatchingRequires;

            public static readonly Tag MasuStem = Tag.FromName(FVocabMatchingRequires + "masu_stem");
            public static readonly Tag Godan = Tag.FromName(FVocabMatchingRequires + "godan");
            public static readonly Tag Ichidan = Tag.FromName(FVocabMatchingRequires + "ichidan");
            public static readonly Tag Irrealis = Tag.FromName(FVocabMatchingRequires + "irrealis");
            public static readonly Tag AdverbHead = Tag.FromName(FVocabMatchingRequires + "preceding_adverb");
            public static readonly Tag PastTenseStem = Tag.FromName(FVocabMatchingRequires + "past-tense-stem");
            public static readonly Tag DictionaryFormStart = Tag.FromName(FVocabMatchingRequires + "dictionary_form_stem");
            public static readonly Tag DictionaryFormHead = Tag.FromName(FVocabMatchingRequires + "dictionary_form_prefix");
            public static readonly Tag IchidanImperative = Tag.FromName(FVocabMatchingRequires + "ichidan_imperative");
            public static readonly Tag GodanPotential = Tag.FromName(FVocabMatchingRequires + "godan_potential");
            public static readonly Tag GodanImperative = Tag.FromName(FVocabMatchingRequires + "godan_imperative");
            public static readonly Tag GodanImperativeHead = Tag.FromName(FVocabMatchingRequires + "godan_imperative_prefix");
            public static readonly Tag TeFormStemStart = Tag.FromName(FVocabMatchingRequires + "te-form-stem");
            public static readonly Tag TeFormStemHead = Tag.FromName(FVocabMatchingRequires + "te-form-prefix");
            public static readonly Tag SentenceEnd = Tag.FromName(FVocabMatchingRequires + "sentence-end");
            public static readonly Tag SentenceStart = Tag.FromName(FVocabMatchingRequires + "sentence-start");
            public static readonly Tag Surface = Tag.FromName(FVocabMatchingRequires + "surface");
            public static readonly Tag SingleToken = Tag.FromName(FVocabMatchingRequires + "single-token");
            public static readonly Tag Compound = Tag.FromName(FVocabMatchingRequires + "compound");
         }

         public static class Forbids
         {
            public static readonly Tag MasuStem = Tag.FromName(FVocabMatchingForbids + "masu_stem");
            public static readonly Tag Godan = Tag.FromName(FVocabMatchingForbids + "godan");
            public static readonly Tag Ichidan = Tag.FromName(FVocabMatchingForbids + "ichidan");
            public static readonly Tag Irrealis = Tag.FromName(FVocabMatchingForbids + "irrealis");
            public static readonly Tag AdverbHead = Tag.FromName(FVocabMatchingForbids + "preceding_adverb");
            public static readonly Tag PastTenseStem = Tag.FromName(FVocabMatchingForbids + "past-tense-stem");
            public static readonly Tag DictionaryFormStart = Tag.FromName(FVocabMatchingForbids + "dictionary_form_stem");
            public static readonly Tag DictionaryFormHead = Tag.FromName(FVocabMatchingForbids + "dictionary_form_prefix");
            public static readonly Tag IchidanImperative = Tag.FromName(FVocabMatchingForbids + "ichidan_imperative");
            public static readonly Tag GodanPotential = Tag.FromName(FVocabMatchingForbids + "godan_potential");
            public static readonly Tag GodanImperative = Tag.FromName(FVocabMatchingForbids + "godan_imperative");
            public static readonly Tag GodanImperativeHead = Tag.FromName(FVocabMatchingForbids + "godan_imperative_prefix");
            public static readonly Tag TeFormStemStart = Tag.FromName(FVocabMatchingForbids + "te-form-stem");
            public static readonly Tag TeFormStemHead = Tag.FromName(FVocabMatchingForbids + "te-form-prefix");
            public static readonly Tag SentenceEnd = Tag.FromName(FVocabMatchingForbids + "sentence-end");
            public static readonly Tag SentenceStart = Tag.FromName(FVocabMatchingForbids + "sentence-start");
            public static readonly Tag Surface = Tag.FromName(FVocabMatchingForbids + "surface");
            public static readonly Tag AutoYielding = Tag.FromName(FVocabMatchingForbids + "auto_yielding");
         }

         public static class Todo
         {
            public static readonly Tag WithPrecedingVowel = Tag.FromName(FVocabMatchingTodo + "match-with-preceding-vowel");
         }

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

      public static readonly Tag ImmersionKit = Tag.FromName(FSource + "immersion_kit");
      public static readonly Tag Jamdict = Tag.FromName(FSource + "jamdict");
   }

   public static readonly Tag DisableKanaOnly = Tag.FromName("_disable_uk");
   public static readonly Tag UsuallyKanaOnly = Tag.FromName("_uk");
   public static readonly Tag TTSAudio = Tag.FromName("_tts_audio");
}
