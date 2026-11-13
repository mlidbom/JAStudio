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
      // Force initialization of every nested class, so that every part of speech the tokenizer gives is registered: a member of each, since
      // using a nested class's member initializes that class alone, not the class it's nested in
      List<JNPartsOfSpeech> unused =
      [
         JNPOS.Pronoun, JNPOS.Noun.Numeral, JNPOS.Noun.Common.General, JNPOS.Noun.Proper.General, JNPOS.Noun.Proper.Person.General, JNPOS.Noun.Proper.Place.General,
         JNPOS.AdjectivalNoun.General, JNPOS.Interjection.General, JNPOS.Verb.General, JNPOS.Adjective.General, JNPOS.Particle.Binding,
         JNPOS.Suffix.VerbLike, JNPOS.Suffix.NounLike.General, JNPOS.Symbol.General, JNPOS.SupplementarySymbol.General, JNPOS.SupplementarySymbol.AsciiArt.General
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

   /// <summary>Every part of speech the tokenizer gives, with all its levels.</summary>
   internal static IEnumerable<JNPartsOfSpeech> All => FullPartsOfSpeechDictionary.Values;

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

   /// <summary>A symbol, punctuation or whitespace: 記号, 補助記号 or 空白.</summary>
   public bool IsNonWordCharacter() => Level1.Japanese is "記号" or "補助記号" or "空白";

   /// <summary>A noun or a pronoun: UniDic has pronouns as a part of speech of their own, where IPADIC had them as a kind of noun.</summary>
   public bool IsNoun() => Level1.Japanese is "名詞" or "代名詞";

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
      AddDescription(new PartOfSpeechDescription("代名詞", "pronoun", "Replaces a noun, e.g., he, she, it"));
      AddDescription(new PartOfSpeechDescription("形状詞", "adjectival noun", "Na-adjectives, describing nouns with な"));
      AddDescription(new PartOfSpeechDescription("連体詞", "pre-noun adjectival / adnominal-adjective", "Modifies nouns directly"));
      AddDescription(new PartOfSpeechDescription("副詞", "adverb", "Modifies verbs/adjectives"));
      AddDescription(new PartOfSpeechDescription("接続詞", "conjunction", "Connects words/clauses"));
      AddDescription(new PartOfSpeechDescription("感動詞", "interjection", "Expresses emotion"));
      AddDescription(new PartOfSpeechDescription("動詞", "verb", "Indicates action"));
      AddDescription(new PartOfSpeechDescription("形容詞", "i-adjective", "Describes nouns"));
      AddDescription(new PartOfSpeechDescription("助動詞", "bound-auxiliary", "Modifies verb tense/mood"));
      AddDescription(new PartOfSpeechDescription("助詞", "particle", "Functional word indicating relation such as marking direct object, subject etc"));
      AddDescription(new PartOfSpeechDescription("接頭辞", "prefix", "Added to beginning of words"));
      AddDescription(new PartOfSpeechDescription("接尾辞", "suffix", "Appended to end of words"));
      AddDescription(new PartOfSpeechDescription("記号", "symbol", "Symbols and characters, such as letters"));
      AddDescription(new PartOfSpeechDescription("補助記号", "punctuation", "Punctuation, brackets and other supplementary symbols"));
      AddDescription(new PartOfSpeechDescription("空白", "space", "Whitespace or blank space"));

      // Level 2, 3, 4 common
      AddDescription(new PartOfSpeechDescription("*", "*", "Wildcard or general category"));
      AddDescription(new PartOfSpeechDescription("一般", "general", "Generic, non-specific"));

      // Level 2
      AddDescription(new PartOfSpeechDescription("普通名詞", "common-noun", "A noun that isn't a name"));
      AddDescription(new PartOfSpeechDescription("固有名詞", "proper-noun", "Names of specific entities, like Tokyo"));
      AddDescription(new PartOfSpeechDescription("数詞", "numeral", "Numbers"));
      AddDescription(new PartOfSpeechDescription("助動詞語幹", "auxiliary-verb-stem", "Base form of auxiliary verbs, such as そう and よう"));
      AddDescription(new PartOfSpeechDescription("非自立可能", "possibly-dependent", "May depend on another word, as an auxiliary does, such as いる after a て-form"));
      AddDescription(new PartOfSpeechDescription("タリ", "tari", "Classical adjectival noun that takes たる or と, such as 堂々"));
      AddDescription(new PartOfSpeechDescription("フィラー", "filler", "Sound/word filling a pause"));
      AddDescription(new PartOfSpeechDescription("係助詞", "binding", "Connects words/clauses, e.g., は, も"));
      AddDescription(new PartOfSpeechDescription("副助詞", "adverbial", "Adverbial particle, modifies verbs"));
      AddDescription(new PartOfSpeechDescription("接続助詞", "conjunctive", "Connects clauses or sentences"));
      AddDescription(new PartOfSpeechDescription("格助詞", "case-particle", "Indicates grammatical case"));
      AddDescription(new PartOfSpeechDescription("準体助詞", "nominalizing", "Makes what precedes it a noun, such as の and ん"));
      AddDescription(new PartOfSpeechDescription("終助詞", "sentence-ending", "Ends the sentence"));
      AddDescription(new PartOfSpeechDescription("名詞的", "noun-like", "Makes a noun"));
      AddDescription(new PartOfSpeechDescription("動詞的", "verb-like", "Makes a verb"));
      AddDescription(new PartOfSpeechDescription("形容詞的", "adjective-like", "Makes an i-adjective"));
      AddDescription(new PartOfSpeechDescription("形状詞的", "adjectival-noun-like", "Makes an adjectival noun"));
      AddDescription(new PartOfSpeechDescription("文字", "character", "A letter or other character"));
      AddDescription(new PartOfSpeechDescription("句点", "period", "Ending punctuation mark"));
      AddDescription(new PartOfSpeechDescription("読点", "comma", "Punctuation to separate elements"));
      AddDescription(new PartOfSpeechDescription("括弧開", "opening-bracket", "Opening bracket punctuation"));
      AddDescription(new PartOfSpeechDescription("括弧閉", "closing-bracket", "Closing bracket punctuation"));
      AddDescription(new PartOfSpeechDescription("ＡＡ", "ascii-art", "Pictures drawn with characters"));

      // Level 3
      AddDescription(new PartOfSpeechDescription("サ変可能", "suru-verb", "Nouns convertible into verbs with 'する'"));
      AddDescription(new PartOfSpeechDescription("形状詞可能", "adjectival", "Nouns that can be used as adjectival nouns, with な"));
      AddDescription(new PartOfSpeechDescription("サ変形状詞可能", "suru-verb-or-adjectival", "Nouns that can take する, or be used as adjectival nouns"));
      AddDescription(new PartOfSpeechDescription("副詞可能", "adverbial", "Nouns that can function as adverbs"));
      AddDescription(new PartOfSpeechDescription("助数詞可能", "counter-possible", "Nouns that can be used as counters"));
      AddDescription(new PartOfSpeechDescription("助数詞", "counter", "Counting words, e.g., つ, 本"));
      AddDescription(new PartOfSpeechDescription("人名", "person-name", "Names of people"));
      AddDescription(new PartOfSpeechDescription("地名", "place-name", "Names of places"));
      AddDescription(new PartOfSpeechDescription("顔文字", "emoticon", "Faces drawn with characters"));

      // Level 4
      AddDescription(new PartOfSpeechDescription("名", "first-name", "Given names"));
      AddDescription(new PartOfSpeechDescription("姓", "surname", "Family names or surnames"));
      AddDescription(new PartOfSpeechDescription("国", "country", "Names of countries"));
   }
}

/// <summary>
/// The parts of speech the tokenizer's dictionary, UniDic, gives, each with its levels from the top: every one it has is registered here, so that<br/>
/// <see cref="JNPartsOfSpeech.Fetch"/> knows it.
/// </summary>
public static class JNPOS
{
   public static readonly JNPartsOfSpeech Pronoun = JNPartsOfSpeech.AddFullPartOfSpeech("代名詞");
   public static readonly JNPartsOfSpeech Adverb = JNPartsOfSpeech.AddFullPartOfSpeech("副詞");
   public static readonly JNPartsOfSpeech BoundAuxiliary = JNPartsOfSpeech.AddFullPartOfSpeech("助動詞");
   public static readonly JNPartsOfSpeech PreNounAdjectival = JNPartsOfSpeech.AddFullPartOfSpeech("連体詞");
   public static readonly JNPartsOfSpeech Conjunction = JNPartsOfSpeech.AddFullPartOfSpeech("接続詞");
   public static readonly JNPartsOfSpeech Prefix = JNPartsOfSpeech.AddFullPartOfSpeech("接頭辞");
   public static readonly JNPartsOfSpeech Space = JNPartsOfSpeech.AddFullPartOfSpeech("空白");

   public static class Noun
   {
      public static readonly JNPartsOfSpeech Numeral = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "数詞");
      public static readonly JNPartsOfSpeech AuxiliaryVerbStem = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "助動詞語幹");

      public static class Common
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "普通名詞", "一般");
         public static readonly JNPartsOfSpeech SuruVerb = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "普通名詞", "サ変可能");
         public static readonly JNPartsOfSpeech Adjectival = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "普通名詞", "形状詞可能");
         public static readonly JNPartsOfSpeech SuruVerbOrAdjectival = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "普通名詞", "サ変形状詞可能");
         public static readonly JNPartsOfSpeech Adverbial = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "普通名詞", "副詞可能");
         public static readonly JNPartsOfSpeech CounterPossible = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "普通名詞", "助数詞可能");
      }

      public static class Proper
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "一般");

         public static class Person
         {
            public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "人名", "一般");
            public static readonly JNPartsOfSpeech Firstname = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "人名", "名");
            public static readonly JNPartsOfSpeech Surname = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "人名", "姓");
         }

         public static class Place
         {
            public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "地名", "一般");
            public static readonly JNPartsOfSpeech Country = JNPartsOfSpeech.AddFullPartOfSpeech("名詞", "固有名詞", "地名", "国");
         }
      }
   }

   /// <summary>Adjectival nouns, 形状詞: the na-adjectives, such as 静か, and the stems of the auxiliaries そう and よう.</summary>
   public static class AdjectivalNoun
   {
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("形状詞", "一般");
      public static readonly JNPartsOfSpeech Tari = JNPartsOfSpeech.AddFullPartOfSpeech("形状詞", "タリ");
      public static readonly JNPartsOfSpeech AuxiliaryVerbStem = JNPartsOfSpeech.AddFullPartOfSpeech("形状詞", "助動詞語幹");
   }

   public static class Interjection
   {
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("感動詞", "一般");
      public static readonly JNPartsOfSpeech Filler = JNPartsOfSpeech.AddFullPartOfSpeech("感動詞", "フィラー");
   }

   /// <summary>
   /// Verbs. UniDic doesn't tell a verb used on its own from one used as an auxiliary, as IPADIC's 自立 and 非自立 did: those that can be<br/>
   /// auxiliaries, such as いる, おく and 来る, are 非自立可能 wherever they are.
   /// </summary>
   public static class Verb
   {
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("動詞", "一般");
      public static readonly JNPartsOfSpeech PossiblyDependent = JNPartsOfSpeech.AddFullPartOfSpeech("動詞", "非自立可能");
      public static readonly HashSet<JNPartsOfSpeech> AllTypes = [General, PossiblyDependent];
   }

   public static class Adjective
   {
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("形容詞", "一般");
      public static readonly JNPartsOfSpeech PossiblyDependent = JNPartsOfSpeech.AddFullPartOfSpeech("形容詞", "非自立可能");
      public static readonly HashSet<JNPartsOfSpeech> AllTypes = [General, PossiblyDependent];
   }

   public static class Particle
   {
      public static readonly JNPartsOfSpeech Binding = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "係助詞");
      public static readonly JNPartsOfSpeech Adverbial = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "副助詞");
      public static readonly JNPartsOfSpeech Conjunctive = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "接続助詞");
      public static readonly JNPartsOfSpeech CaseMarking = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "格助詞");
      public static readonly JNPartsOfSpeech Nominalizing = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "準体助詞");
      public static readonly JNPartsOfSpeech SentenceEnding = JNPartsOfSpeech.AddFullPartOfSpeech("助詞", "終助詞");
   }

   public static class Suffix
   {
      public static readonly JNPartsOfSpeech VerbLike = JNPartsOfSpeech.AddFullPartOfSpeech("接尾辞", "動詞的");
      public static readonly JNPartsOfSpeech AdjectiveLike = JNPartsOfSpeech.AddFullPartOfSpeech("接尾辞", "形容詞的");
      public static readonly JNPartsOfSpeech AdjectivalNounLike = JNPartsOfSpeech.AddFullPartOfSpeech("接尾辞", "形状詞的");

      public static class NounLike
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("接尾辞", "名詞的", "一般");
         public static readonly JNPartsOfSpeech SuruVerb = JNPartsOfSpeech.AddFullPartOfSpeech("接尾辞", "名詞的", "サ変可能");
         public static readonly JNPartsOfSpeech Adverbial = JNPartsOfSpeech.AddFullPartOfSpeech("接尾辞", "名詞的", "副詞可能");
         public static readonly JNPartsOfSpeech Counter = JNPartsOfSpeech.AddFullPartOfSpeech("接尾辞", "名詞的", "助数詞");
      }
   }

   public static class Symbol
   {
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "一般");
      public static readonly JNPartsOfSpeech Character = JNPartsOfSpeech.AddFullPartOfSpeech("記号", "文字");
   }

   /// <summary>Punctuation, brackets and other supplementary symbols, 補助記号.</summary>
   public static class SupplementarySymbol
   {
      public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("補助記号", "一般");
      public static readonly JNPartsOfSpeech Period = JNPartsOfSpeech.AddFullPartOfSpeech("補助記号", "句点");
      public static readonly JNPartsOfSpeech Comma = JNPartsOfSpeech.AddFullPartOfSpeech("補助記号", "読点");
      public static readonly JNPartsOfSpeech OpeningBracket = JNPartsOfSpeech.AddFullPartOfSpeech("補助記号", "括弧開");
      public static readonly JNPartsOfSpeech ClosingBracket = JNPartsOfSpeech.AddFullPartOfSpeech("補助記号", "括弧閉");

      public static class AsciiArt
      {
         public static readonly JNPartsOfSpeech General = JNPartsOfSpeech.AddFullPartOfSpeech("補助記号", "ＡＡ", "一般");
         public static readonly JNPartsOfSpeech Emoticon = JNPartsOfSpeech.AddFullPartOfSpeech("補助記号", "ＡＡ", "顔文字");
      }
   }
}
