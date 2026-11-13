using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.Parsing.Tokenizing;
using Xunit;

namespace JAStudio.Core.Specifications.LanguageServices.Parsing.Tokenizing;

/// <summary>
/// The tokens the tokenizer, reading with UniDic, cuts some sentences into, with their parts of speech, inflection types and forms, and base forms.<br/>
/// Ported from test_explore_token_part_of_speech_information.py, which pinned IPADIC's; restated for UniDic when the tokenizer switched to it.
/// </summary>
public class ExploreTokenPartOfSpeechInformationTests : IDisposable
{
   readonly MeCabTokenizer _tokenizer;

   public ExploreTokenPartOfSpeechInformationTests() => _tokenizer = MeCabTokenizer.GetInstance();

   public void Dispose() {}

   [Theory]
   [MemberData(nameof(GetTokenTestData))]
   public void IdentifySomethingWords(string sentence, List<MeCabToken> expectedTokens)
   {
      var tokenized = _tokenizer.Tokenize(sentence);
      Assert.Equal(expectedTokens, tokenized.Tokens);
   }

   static UniDicInflectionType Type(string name) => UniDicInflectionTypes.GetByName(name);
   static UniDicInflectionForm Form(string name) => UniDicInflectionForms.GetByName(name);

   static MeCabToken Konna() => new(UniDicPartsOfSpeech.AdjectivalNoun.General, "こんな", "こんな");
   static MeCabToken Sonna() => new(UniDicPartsOfSpeech.AdjectivalNoun.General, "そんな", "そんな");
   static MeCabToken TheCopulasNi() => new(UniDicPartsOfSpeech.BoundAuxiliary, "だ", "に", Type("助動詞-ダ"), Form("連用形-ニ"));

   public static IEnumerable<object[]> GetTokenTestData() =>
      new List<object[]>
      {
         new object[] { "こんなに", new List<MeCabToken> { Konna(), TheCopulasNi() } },
         new object[]
         {
            "こんなに疲れている",
            new List<MeCabToken>
            {
               Konna(), TheCopulasNi(),
               new(UniDicPartsOfSpeech.Verb.General, "疲れる", "疲れ", Type("下一段-ラ行"), Form("連用形-一般")),
               new(UniDicPartsOfSpeech.Particle.Conjunctive, "て", "て"),
               new(UniDicPartsOfSpeech.Verb.PossiblyDependent, "いる", "いる", Type("上一段-ア行"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "こんなに食べている",
            new List<MeCabToken>
            {
               Konna(), TheCopulasNi(),
               new(UniDicPartsOfSpeech.Verb.General, "食べる", "食べ", Type("下一段-バ行"), Form("連用形-一般")),
               new(UniDicPartsOfSpeech.Particle.Conjunctive, "て", "て"),
               new(UniDicPartsOfSpeech.Verb.PossiblyDependent, "いる", "いる", Type("上一段-ア行"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "こんなにする",
            new List<MeCabToken> { Konna(), TheCopulasNi(), new(UniDicPartsOfSpeech.Verb.PossiblyDependent, "する", "する", Type("サ行変格"), Form("終止形-一般")) }
         },
         new object[]
         {
            "こんなに食べる",
            new List<MeCabToken> { Konna(), TheCopulasNi(), new(UniDicPartsOfSpeech.Verb.General, "食べる", "食べる", Type("下一段-バ行"), Form("終止形-一般")) }
         },
         new object[]
         {
            "そんなに好きだ",
            new List<MeCabToken>
            {
               Sonna(), TheCopulasNi(),
               new(UniDicPartsOfSpeech.AdjectivalNoun.General, "好き", "好き"),
               new(UniDicPartsOfSpeech.BoundAuxiliary, "だ", "だ", Type("助動詞-ダ"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "そんなに走った",
            new List<MeCabToken>
            {
               Sonna(), TheCopulasNi(),
               new(UniDicPartsOfSpeech.Verb.General, "走る", "走っ", Type("五段-ラ行"), Form("連用形-促音便")),
               new(UniDicPartsOfSpeech.BoundAuxiliary, "た", "た", Type("助動詞-タ"), Form("終止形-一般"))
            }
         },
         new object[] { "来い", new List<MeCabToken> { new(UniDicPartsOfSpeech.Verb.PossiblyDependent, "来る", "来い", Type("カ行変格"), Form("命令形")) } },
         new object[]
         {
            "飛べない",
            new List<MeCabToken>
            {
               new(UniDicPartsOfSpeech.Verb.General, "飛べる", "飛べ", Type("下一段-バ行"), Form("未然形-一般")),
               new(UniDicPartsOfSpeech.BoundAuxiliary, "ない", "ない", Type("助動詞-ナイ"), Form("終止形-一般"))
            }
         },
         new object[]
         {
            "飛ばない",
            new List<MeCabToken>
            {
               new(UniDicPartsOfSpeech.Verb.General, "飛ぶ", "飛ば", Type("五段-バ行"), Form("未然形-一般")),
               new(UniDicPartsOfSpeech.BoundAuxiliary, "ない", "ない", Type("助動詞-ナイ"), Form("終止形-一般"))
            }
         },
         new object[] { "飛べ", new List<MeCabToken> { new(UniDicPartsOfSpeech.Verb.General, "飛ぶ", "飛べ", Type("五段-バ行"), Form("命令形")) } },
         new object[] { "会う", new List<MeCabToken> { new(UniDicPartsOfSpeech.Verb.General, "会う", "会う", Type("五段-ワア行"), Form("終止形-一般")) } }
      };
}
