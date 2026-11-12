using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public class PartOfSpeechDescription
{
   public string Japanese { get; }
   public string English { get; }
   public string Explanation { get; }

   public PartOfSpeechDescription(string japanese, string english, string explanation)
   {
      Japanese = japanese;
      English = english;
      Explanation = explanation;
   }

   public override string ToString() => English;
}

public class JNPartsOfSpeech
{
   static readonly Dictionary<string, PartOfSpeechDescription> JapaneseToPartOfSpeech = new();
   static readonly Dictionary<string, JNPartsOfSpeech> FullPartsOfSpeechDictionary = new();

   public PartOfSpeechDescription Level1 { get; }
   public PartOfSpeechDescription Level2 { get; }
   public PartOfSpeechDescription Level3 { get; }
   public PartOfSpeechDescription Level4 { get; }

   static JNPartsOfSpeech()
   {
      // Initialize all part of speech descriptions
      InitializeDescriptions();
      // Force initialization of all nested classes
      List<JNPartsOfSpeech> unused =
      [
         JNPOS.Filler, JNPOS.Other.Interjection, JNPOS.Adverb.General, JNPOS.Particle.Binding,
         JNPOS.Particle.CaseMarking.General, JNPOS.Verb.Independent, JNPOS.Noun.General,
         JNPOS.Noun.Pronoun.General, JNPOS.Noun.ProperNoun.General, JNPOS.Noun.ProperNoun.Person.General,
         JNPOS.Noun.ProperNoun.Location.General, JNPOS.Noun.Suffix.General, JNPOS.Noun.Special.AuxiliaryVerbStem,
         JNPOS.Noun.Dependent.General, JNPOS.Adjective.Independent, JNPOS.Prefix.Noun, JNPOS.Symbol.General
      ];
   }

   JNPartsOfSpeech(string level1, string level2 = "*", string level3 = "*", string level4 = "*")
   {
      Level1 = JapaneseToPartOfSpeech[level1];
      Level2 = JapaneseToPartOfSpeech[level2];
      Level3 = JapaneseToPartOfSpeech[level3];
      Level4 = JapaneseToPartOfSpeech[level4];
      Levels = [.. new[] { Level1, Level2, Level3, Level4 }.Where(it => it.Japanese != "*")];
   }

   public static JNPartsOfSpeech Fetch(string unparsed) => FullPartsOfSpeechDictionary[unparsed];

   /// <summary>
   /// No part of speech at all: it has no levels, so it's within none (<see cref="IsWithin"/>). The tokenizer never gives it: it's the part of speech of<br/>
   /// the empty token a match-condition sees where there is no token (<see cref="Note.Vocabulary.Matching.Conditions.Tokens.EmptyToken"/>).
   /// </summary>
   internal static JNPartsOfSpeech None => NoPartOfSpeech.Instance;

   /// <summary>Holds <see cref="None"/>, made at its first use, after the levels' descriptions it's made of exist, and never among the tokenizer's.</summary>
   static class NoPartOfSpeech
   {
      internal static readonly JNPartsOfSpeech Instance = new("*");
   }

   /// <summary>The levels this part of speech has, from the top, such as 名詞, 接尾 and 人名 for a suffix to a person's name: four at most, without the unused ones.</summary>
   public IReadOnlyList<PartOfSpeechDescription> Levels { get; }

   /// <summary>
   /// Whether this part of speech is <paramref name="japaneseLevels"/>, or a kind of it: its first levels are those, from the top. 名詞 and 接尾 are every<br/>
   /// kind of suffix noun, 名詞 alone every kind of noun.
   /// </summary>
   public bool IsWithin(IReadOnlyList<string> japaneseLevels) => Levels.Take(japaneseLevels.Count).Select(it => it.Japanese).SequenceEqual(japaneseLevels);

   /// <summary>Whether any part of speech the tokenizer gives is <paramref name="japaneseLevels"/>, or a kind of it (<see cref="IsWithin"/>).</summary>
   public static bool AnyIsWithin(IReadOnlyList<string> japaneseLevels) => FullPartsOfSpeechDictionary.Values.Any(it => it.IsWithin(japaneseLevels));

   /// <summary>The English name of the part-of-speech level <paramref name="japaneseLevel"/>, such as "suffix" for 接尾.</summary>
   public static string EnglishNameOf(string japaneseLevel) => JapaneseToPartOfSpeech[japaneseLevel].English;

   public bool IsNonWordCharacter() => Level1.Japanese == "記号";
   public bool IsNoun() => Level1.Japanese == "名詞";

   public override string ToString() =>
      string.Concat(
         "1:" + KanaUtils.PadToLength(Level1.Japanese, 5),
         "2:" + KanaUtils.PadToLength(Level2.Japanese.Replace("*", ""), 6),
         "3:" + KanaUtils.PadToLength(Level3.Japanese.Replace("*", ""), 6),
         "4:" + KanaUtils.PadToLength(Level4.Japanese.Replace("*", ""), 6)
      );

   static void AddDescription(PartOfSpeechDescription desc)
   {
      JapaneseToPartOfSpeech[desc.Japanese] = desc;
   }

   internal static JNPartsOfSpeech AddFullPartOfSpeech(string level1, string level2 = "*", string level3 = "*", string level4 = "*")
   {
      var combined = $"{level1},{level2},{level3},{level4}";
      var partsOfSpeech = new JNPartsOfSpeech(level1, level2, level3, level4);
      FullPartsOfSpeechDictionary[combined] = partsOfSpeech;
      return partsOfSpeech;
   }

   static void InitializeDescriptions()
   {
      // Level 1
      AddDescription(new PartOfSpeechDescription("名詞", "noun", "Names things or ideas"));
      AddDescription(new PartOfSpeechDescription("形容詞", "i-adjective", "Describes nouns"));
      AddDescription(new PartOfSpeechDescription("連体詞", "pre-noun adjectival / adnominal-adjective", "Modifies nouns directly"));
      AddDescription(new PartOfSpeechDescription("接続詞的", "conjunctive", "words or expressions that function in a manner similar to conjunctions"));
      AddDescription(new PartOfSpeechDescription("動詞", "verb", "Indicates action"));
      AddDescription(new PartOfSpeechDescription("副詞", "adverb", "Modifies verbs/adjectives"));
      AddDescription(new PartOfSpeechDescription("助動詞", "bound-auxiliary", "Modifies verb tense/mood"));
      AddDescription(new PartOfSpeechDescription("助詞", "particle", "Functional word indicating relation such as marking direct object, subject etc"));
      AddDescription(new PartOfSpeechDescription("接続詞", "conjunction", "Connects words/clauses"));
      AddDescription(new PartOfSpeechDescription("感動詞", "interjection", "Expresses emotion"));
      AddDescription(new PartOfSpeechDescription("接頭詞", "prefix", "Added to beginning of words"));
      AddDescription(new PartOfSpeechDescription("フィラー", "filler", "Sound/word filling a pause"));
      AddDescription(new PartOfSpeechDescription("記号", "symbol", "Punctuation, special symbols"));
      AddDescription(new PartOfSpeechDescription("その他", "others", "Miscellaneous, doesn't fit other categories"));

      // Level 2, 3, 4 common
      AddDescription(new PartOfSpeechDescription("*", "*", "Wildcard or general category"));
      AddDescription(new PartOfSpeechDescription("一般", "general", "Generic, non-specific"));
      AddDescription(new PartOfSpeechDescription("サ変接続", "suru-verb", "Nouns convertible into verbs with 'する'"));
      AddDescription(new PartOfSpeechDescription("特殊", "special", "Irregular forms"));
      AddDescription(new PartOfSpeechDescription("副詞可能", "adverbial", "Nouns/verbs that can function as adverbs"));
      AddDescription(new PartOfSpeechDescription("形容動詞語幹", "na-adjective stem", "Base form of na-adjectives"));

      // Level 2
      AddDescription(new PartOfSpeechDescription("自立", "independent", "Not dependent on other words"));
      AddDescription(new PartOfSpeechDescription("動詞接続", "verb-connective", "indicates a form or a word that is used to connect with or modify a verb"));
      AddDescription(new PartOfSpeechDescription("代名詞", "pronoun", "Replaces a noun, e.g., he, she, it"));
      AddDescription(new PartOfSpeechDescription("係助詞", "binding", "Connects words/clauses, e.g., は, も"));
      AddDescription(new PartOfSpeechDescription("読点", "comma", "Punctuation to separate elements"));
      AddDescription(new PartOfSpeechDescription("連体化", "adnominalization", "Turns word into modifier for nouns"));
      AddDescription(new PartOfSpeechDescription("副助詞", "adverbial", "Adverbial particle, modifies verbs"));
      AddDescription(new PartOfSpeechDescription("副助詞／並立助詞／終助詞", "adverbial/coordinating-conjunction/ending", "Various particle types"));
      AddDescription(new PartOfSpeechDescription("形容詞接続", "adjective-connection", "Connects adjectives"));
      AddDescription(new PartOfSpeechDescription("副詞化", "adverbialization", "Turns word into adverb"));
      AddDescription(new PartOfSpeechDescription("間投", "interjection", "Expresses emotion or marks a pause"));
      AddDescription(new PartOfSpeechDescription("非自立", "dependent", "Depends on another word to convey meaning"));
      AddDescription(new PartOfSpeechDescription("括弧閉", "closing-bracket", "Closing bracket punctuation"));
      AddDescription(new PartOfSpeechDescription("括弧開", "opening-bracket", "Opening bracket punctuation"));
      AddDescription(new PartOfSpeechDescription("接尾", "suffix", "Appended to end of words"));
      AddDescription(new PartOfSpeechDescription("接続助詞", "conjunctive", "Connects clauses or sentences"));
      AddDescription(new PartOfSpeechDescription("助詞類接続", "particle-connection", "Connects particles"));
      AddDescription(new PartOfSpeechDescription("数", "numeric", "Numerical expressions"));
      AddDescription(new PartOfSpeechDescription("動詞非自立的", "auxiliary-verb", "Auxiliary verb form"));
      AddDescription(new PartOfSpeechDescription("数接続", "numeric-connection", "Numeric connectors"));
      AddDescription(new PartOfSpeechDescription("句点", "period", "Ending punctuation mark"));
      AddDescription(new PartOfSpeechDescription("格助詞", "case-particle", "Indicates grammatical case"));
      AddDescription(new PartOfSpeechDescription("アルファベット", "alphabet", "Alphabetical characters"));
      AddDescription(new PartOfSpeechDescription("ナイ形容詞語幹", "negative-adjective-stem", "Negative adjective stem form"));
      AddDescription(new PartOfSpeechDescription("空白", "space", "Whitespace or blank space"));
      AddDescription(new PartOfSpeechDescription("名詞接続", "noun-connection", "Noun connectors"));
      AddDescription(new PartOfSpeechDescription("終助詞", "sentence-ending", "Ends the sentence"));
      AddDescription(new PartOfSpeechDescription("固有名詞", "proper-noun", "Names of specific entities, like Tokyo"));
      AddDescription(new PartOfSpeechDescription("並立助詞", "coordinating-conjunction", "Connects equal grammatical items, e.g., and, or"));
      AddDescription(new PartOfSpeechDescription("引用文字列", "quoted-character-string", "quoted-character-string"));

      // Level 3
      AddDescription(new PartOfSpeechDescription("助数詞", "counter", "Counting words, e.g., つ, 本"));
      AddDescription(new PartOfSpeechDescription("連語", "compound", "Compound words, two or more words combined"));
      AddDescription(new PartOfSpeechDescription("人名", "person-name", "Names of people"));
      AddDescription(new PartOfSpeechDescription("地域", "region", "Names of regions, cities, countries"));
      AddDescription(new PartOfSpeechDescription("引用", "quotation", "Quotation or citation"));
      AddDescription(new PartOfSpeechDescription("組織", "organization", "Names of organizations, companies"));
      AddDescription(new PartOfSpeechDescription("助動詞語幹", "auxiliary-verb-stem", "Base form of auxiliary verbs"));
      AddDescription(new PartOfSpeechDescription("縮約", "contraction", "Contracted forms, e.g., can't, don't"));

      // Level 4
      AddDescription(new PartOfSpeechDescription("名", "first-name", "Given names"));
      AddDescription(new PartOfSpeechDescription("姓", "surname", "Family names or surnames"));
      AddDescription(new PartOfSpeechDescription("国", "country", "Names of countries"));
   }
}

public static class JNPOS
{
   public static readonly JNPartsOfSpeech Filler = JNPartsOfSpeech.AddFullPartOfSpeech("フィラー");
   public static readonly JNPartsOfSpeech BoundAuxiliary = JNPartsOfSpeech.AddFullPartOfSpeech("助動詞");
   public static readonly JNPartsOfSpeech PreNounAdjectival = JNPartsOfSpeech.AddFullPartOfSpeech("連体詞");
   public static readonly JNPartsOfSpeech Interjection = JNPartsOfSpeech.AddFullPartOfSpeech("感動詞");
   public static readonly JNPartsOfSpeech Conjunction = JNPartsOfSpeech.AddFullPartOfSpeech("接続詞");

   public static class Other
   {
      public static readonly JNPartsOfSpeech Interjection = JNPartsOfSpeech.AddFullPartOfSpeech("その他", "間投");
   }

   public static class Adverb
   {
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("副詞", "一般");
      public static readonly JNPartsOfSpeech ParticleConnection = JNPartsOfSpeech.AddFullPartOfSpeech("副詞", "助詞類接続");
   }

   public static class Particle
   {
      public static readonly JNPartsOfSpeech CoordinatingConjunction = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "並立助詞");
      public static readonly JNPartsOfSpeech Binding = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "係助詞");
      public static readonly JNPartsOfSpeech Adverbial = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "副助詞");
      public static readonly JNPartsOfSpeech AdverbialCoordinatingEnding = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "副助詞／並立助詞／終助詞");
      public static readonly JNPartsOfSpeech Adverbialization = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "副詞化");
      public static readonly JNPartsOfSpeech Conjunctive = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "接続助詞");
      public static readonly JNPartsOfSpeech Special = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "特殊");
      public static readonly JNPartsOfSpeech SentenceEnding = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "終助詞");
      public static readonly JNPartsOfSpeech Adnominalization = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "連体化");

      public static class CaseMarking
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "格助詞", "一般");
         public static readonly JNPartsOfSpeech Quotation = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "格助詞", "引用");
         public static readonly JNPartsOfSpeech Compound = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "格助詞", "連語");
      }
   }

   public static class Verb
   {
      public static readonly JNPartsOfSpeech Suffix = JNPartsOfSpeech.AddFullPartOfSpeech("動詞", "接尾");
      public static readonly JNPartsOfSpeech Independent = JNPartsOfSpeech.AddFullPartOfSpeech("動詞", "自立");
      public static readonly JNPartsOfSpeech Dependent = JNPartsOfSpeech.AddFullPartOfSpeech("動詞", "非自立");
      public static readonly HashSet<JNPartsOfSpeech> AllTypes = [Independent, Dependent, Suffix];
   }

   public static class Noun
   {
      public static readonly JNPartsOfSpeech SuruVerb = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "サ変接続");
      public static readonly JNPartsOfSpeech NegativeAdjectiveStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "ナイ形容詞語幹");
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "一般");
      public static readonly JNPartsOfSpeech Adverbial = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "副詞可能");
      public static readonly JNPartsOfSpeech AuxiliaryVerb = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "動詞非自立的");
      public static readonly JNPartsOfSpeech NaAdjectiveStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "形容動詞語幹");
      public static readonly JNPartsOfSpeech Numeric = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "数");
      public static readonly JNPartsOfSpeech Conjunctive = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接続詞的");
      public static readonly JNPartsOfSpeech QuotedCharacterString = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "引用文字列");

      public static class Pronoun
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "代名詞", "一般");
         public static readonly JNPartsOfSpeech Contracted = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "代名詞", "縮約");
      }

      public static class ProperNoun
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "一般");
         public static readonly JNPartsOfSpeech Organization = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "組織");

         public static class Person
         {
            public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "人名", "一般");
            public static readonly JNPartsOfSpeech Firstname = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "人名", "名");
            public static readonly JNPartsOfSpeech Surname = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "人名", "姓");
         }

         public static class Location
         {
            public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "地域", "一般");
            public static readonly JNPartsOfSpeech Country = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "地域", "国");
         }
      }

      public static class Suffix
      {
         public static readonly JNPartsOfSpeech SuruVerbConnection = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "サ変接続");
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "一般");
         public static readonly JNPartsOfSpeech PersonsName = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "人名");
         public static readonly JNPartsOfSpeech Adverbial = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "副詞可能");
         public static readonly JNPartsOfSpeech AuxiliaryVerbStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "助動詞語幹");
         public static readonly JNPartsOfSpeech Counter = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "助数詞");
         public static readonly JNPartsOfSpeech Region = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "地域");
         public static readonly JNPartsOfSpeech NaAdjectiveStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "形容動詞語幹");
         public static readonly JNPartsOfSpeech Special = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "接尾", "特殊");
      }

      public static class Special
      {
         public static readonly JNPartsOfSpeech AuxiliaryVerbStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "特殊", "助動詞語幹");
      }

      public static class Dependent
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "非自立", "一般");
         public static readonly JNPartsOfSpeech Adverbial = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "非自立", "副詞可能");
         public static readonly JNPartsOfSpeech AuxiliaryVerbStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "非自立", "助動詞語幹");
         public static readonly JNPartsOfSpeech NaAdjectiveStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "非自立", "形容動詞語幹");
      }
   }

   public static class Adjective
   {
      public static readonly JNPartsOfSpeech Suffix = JNPartsOfSpeech.AddFullPartOfSpeech("形容詞", "接尾");
      public static readonly JNPartsOfSpeech Independent = JNPartsOfSpeech.AddFullPartOfSpeech("形容詞", "自立");
      public static readonly JNPartsOfSpeech Dependent = JNPartsOfSpeech.AddFullPartOfSpeech("形容詞", "非自立");
      public static readonly HashSet<JNPartsOfSpeech> AllTypes = [Independent, Dependent, Suffix];
   }

   public static class Prefix
   {
      public static readonly JNPartsOfSpeech Noun = JNPartsOfSpeech.AddFullPartOfSpeech("接頭詞", "名詞接続");
      public static readonly JNPartsOfSpeech Adjective = JNPartsOfSpeech.AddFullPartOfSpeech("接頭詞", "形容詞接続");
      public static readonly JNPartsOfSpeech Number = JNPartsOfSpeech.AddFullPartOfSpeech("接頭詞", "数接続");
      public static readonly JNPartsOfSpeech VerbConnective = JNPartsOfSpeech.AddFullPartOfSpeech("接頭詞", "動詞接続");
   }

   public static class Symbol
   {
      public static readonly JNPartsOfSpeech Alphabet = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "アルファベット");
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "一般");
      public static readonly JNPartsOfSpeech Period = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "句点");
      public static readonly JNPartsOfSpeech ClosingBracket = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "括弧閉");
      public static readonly JNPartsOfSpeech OpeningBracket = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "括弧開");
      public static readonly JNPartsOfSpeech Space = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "空白");
      public static readonly JNPartsOfSpeech Comma = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "読点");
   }
}
