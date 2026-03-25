using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using Xunit;

namespace JAStudio.Core.Specifications.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// Tests ported from test_explore_token_part_of_speech_information.py
/// </summary>
public class ExploreTokenPartOfSpeechInformationTests : IDisposable
{
   readonly JNTokenizer _tokenizer;

   public ExploreTokenPartOfSpeechInformationTests() => _tokenizer = JNTokenizer.GetInstance();

   public void Dispose() {}

   [Theory]
   [MemberData(nameof(GetTokenTestData))]
   public void IdentifySomethingWords(string sentence, List<JNToken> expectedTokens)
   {
      var tokenized = _tokenizer.Tokenize(sentence);
      Assert.Equal(expectedTokens, tokenized.Tokens);
   }

   public static IEnumerable<object[]> GetTokenTestData()
   {
      return new List<object[]>
             {
                new object[]
                {
                   "こんなに",
                   new List<JNToken>
                   {
                      new(JNPOS.Adverb.ParticleConnection, "こんなに", "こんなに")
                   }
                },
                new object[]
                {
                   "こんなに疲れている",
                   new List<JNToken>
                   {
                      new(JNPOS.Adverb.ParticleConnection, "こんなに", "こんなに"),
                      new(JNPOS.Verb.Independent, "疲れる", "疲れ", InflectionTypes.Ichidan.Regular, InflectionForms.Continuative.RenyoukeiMasuStem),
                      new(JNPOS.Particle.Conjunctive, "て", "て"),
                      new(JNPOS.Verb.Dependent, "いる", "いる", InflectionTypes.Ichidan.Regular, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "こんなに食べている",
                   new List<JNToken>
                   {
                      new(JNPOS.Adverb.ParticleConnection, "こんなに", "こんなに"),
                      new(JNPOS.Verb.Independent, "食べる", "食べ", InflectionTypes.Ichidan.Regular, InflectionForms.Continuative.RenyoukeiMasuStem),
                      new(JNPOS.Particle.Conjunctive, "て", "て"),
                      new(JNPOS.Verb.Dependent, "いる", "いる", InflectionTypes.Ichidan.Regular, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "こんなにする",
                   new List<JNToken>
                   {
                      new(JNPOS.PreNounAdjectival, "こんな", "こんな"),
                      new(JNPOS.Particle.CaseMarking.General, "に", "に"),
                      new(JNPOS.Verb.Independent, "する", "する", InflectionTypes.Sahen.Suru, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "こんなに食べる",
                   new List<JNToken>
                   {
                      new(JNPOS.PreNounAdjectival, "こんな", "こんな"),
                      new(JNPOS.Particle.CaseMarking.General, "に", "に"),
                      new(JNPOS.Verb.Independent, "食べる", "食べる", InflectionTypes.Ichidan.Regular, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "そんなに好きだ",
                   new List<JNToken>
                   {
                      new(JNPOS.Adverb.General, "そんなに", "そんなに"),
                      new(JNPOS.Noun.NaAdjectiveStem, "好き", "好き"),
                      new(JNPOS.BoundAuxiliary, "だ", "だ", InflectionTypes.Special.Da, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "そんなに走った",
                   new List<JNToken>
                   {
                      new(JNPOS.Adverb.General, "そんなに", "そんなに"),
                      new(JNPOS.Verb.Independent, "走る", "走っ", InflectionTypes.Godan.Ru, InflectionForms.Continuative.TaConnection),
                      new(JNPOS.BoundAuxiliary, "た", "た", InflectionTypes.Special.Ta, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "来い",
                   new List<JNToken>
                   {
                      new(JNPOS.Verb.Independent, "来る", "来い", InflectionTypes.Kahen.KuruKanji, InflectionForms.ImperativeMeireikei.I)
                   }
                },
                new object[]
                {
                   "飛べない",
                   new List<JNToken>
                   {
                      new(JNPOS.Verb.Independent, "飛べる", "飛べ", InflectionTypes.Ichidan.Regular, InflectionForms.Irrealis.GeneralIrrealisMizenkei),
                      new(JNPOS.BoundAuxiliary, "ない", "ない", InflectionTypes.Special.Nai, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "飛ばない",
                   new List<JNToken>
                   {
                      new(JNPOS.Verb.Independent, "飛ぶ", "飛ば", InflectionTypes.Godan.Bu, InflectionForms.Irrealis.GeneralIrrealisMizenkei),
                      new(JNPOS.BoundAuxiliary, "ない", "ない", InflectionTypes.Special.Nai, InflectionForms.Basic.DictionaryForm)
                   }
                },
                new object[]
                {
                   "飛べ",
                   new List<JNToken>
                   {
                      new(JNPOS.Verb.Independent, "飛べる", "飛べ", InflectionTypes.Ichidan.Regular, InflectionForms.Continuative.RenyoukeiMasuStem)
                   }
                },
                new object[]
                {
                   "会う",
                   new List<JNToken>
                   {
                      new(JNPOS.Verb.Independent, "会う", "会う", InflectionTypes.Godan.UGemination, InflectionForms.Basic.DictionaryForm)
                   }
                }
             };
   }
}
