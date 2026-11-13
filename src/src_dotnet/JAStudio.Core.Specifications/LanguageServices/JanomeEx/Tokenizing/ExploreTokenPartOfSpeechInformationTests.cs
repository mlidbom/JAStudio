using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using Xunit;

namespace JAStudio.Core.Specifications.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// The tokens the tokenizer, reading with UniDic, cuts some sentences into, with their parts of speech, inflection types and forms, and base forms.<br/>
/// Ported from test_explore_token_part_of_speech_information.py, which pinned IPADIC's; restated for UniDic when the tokenizer switched to it.
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

   static InflectionType Type(string name) => InflectionTypes.GetByName(name);
   static InflectionForm Form(string name) => InflectionForms.GetByName(name);

   static JNToken Konna() => new(JNPOS.AdjectivalNoun.General, "こんな", "こんな");
   static JNToken Sonna() => new(JNPOS.AdjectivalNoun.General, "そんな", "そんな");
   static JNToken TheCopulasNi() => new(JNPOS.BoundAuxiliary, "だ", "に", Type("助動詞-ダ"), Form("連用形-ニ"));

   public static IEnumerable<object[]> GetTokenTestData() =>
      new List<object[]>
      {
         new object[] { "こんなに", new List<JNToken> { Konna(), TheCopulasNi() } },
         new object[]
         {
            "こんなに疲れている",
            new List<JNToken>
            {
               Konna(), TheCopulasNi(),
               new(JNPOS.Verb.General, "疲れる", "疲れ", Type("下一段-ラ行"), Form("連用形-一般")),
               new(JNPOS.Particle.Conjunctive, "て", "て"),
               new(JNPOS.Verb.PossiblyDependent, "いる", "いる", Type("上一段-ア行"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "こんなに食べている",
            new List<JNToken>
            {
               Konna(), TheCopulasNi(),
               new(JNPOS.Verb.General, "食べる", "食べ", Type("下一段-バ行"), Form("連用形-一般")),
               new(JNPOS.Particle.Conjunctive, "て", "て"),
               new(JNPOS.Verb.PossiblyDependent, "いる", "いる", Type("上一段-ア行"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "こんなにする",
            new List<JNToken> { Konna(), TheCopulasNi(), new(JNPOS.Verb.PossiblyDependent, "する", "する", Type("サ行変格"), Form("終止形-一般")) }
         },
         new object[]
         {
            "こんなに食べる",
            new List<JNToken> { Konna(), TheCopulasNi(), new(JNPOS.Verb.General, "食べる", "食べる", Type("下一段-バ行"), Form("終止形-一般")) }
         },
         new object[]
         {
            "そんなに好きだ",
            new List<JNToken>
            {
               Sonna(), TheCopulasNi(),
               new(JNPOS.AdjectivalNoun.General, "好き", "好き"),
               new(JNPOS.BoundAuxiliary, "だ", "だ", Type("助動詞-ダ"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "そんなに走った",
            new List<JNToken>
            {
               Sonna(), TheCopulasNi(),
               new(JNPOS.Verb.General, "走る", "走っ", Type("五段-ラ行"), Form("連用形-促音便")),
               new(JNPOS.BoundAuxiliary, "た", "た", Type("助動詞-タ"), Form("終止形-一般"))
            }
         },
         new object[] { "来い", new List<JNToken> { new(JNPOS.Verb.PossiblyDependent, "来る", "来い", Type("カ行変格"), Form("命令形")) } },
         new object[]
         {
            "飛べない",
            new List<JNToken>
            {
               new(JNPOS.Verb.General, "飛べる", "飛べ", Type("下一段-バ行"), Form("未然形-一般")),
               new(JNPOS.BoundAuxiliary, "ない", "ない", Type("助動詞-ナイ"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "飛ばない",
            new List<JNToken>
            {
               new(JNPOS.Verb.General, "飛ぶ", "飛ば", Type("五段-バ行"), Form("未然形-一般")),
               new(JNPOS.BoundAuxiliary, "ない", "ない", Type("助動詞-ナイ"), Form("終止形-一般"))
            }
         },
         new object[] { "飛べ", new List<JNToken> { new(JNPOS.Verb.General, "飛ぶ", "飛べ", Type("五段-バ行"), Form("命令形")) } },
         new object[] { "会う", new List<JNToken> { new(JNPOS.Verb.General, "会う", "会う", Type("五段-ワア行"), Form("終止形-一般")) } }
      };
}
