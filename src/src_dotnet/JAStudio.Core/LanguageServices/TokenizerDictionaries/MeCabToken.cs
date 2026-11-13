using System.Collections.Generic;
using Compze.Contracts;
using MeCab;

namespace JAStudio.Core.LanguageServices.TokenizerDictionaries;

/// <summary>
/// A token MeCab cuts from a text, described as its <see cref="TokenizerDictionary"/> describes it, in what IPADIC and UniDic both tell: where<br/>
/// it is, its part of speech, inflection and base form, and UniDic's lemma. To compare how two dictionaries tokenize, not to parse with:<br/>
/// JAStudio's parsing reads tokens as <see cref="JanomeEx.Tokenizing.JNToken"/>.
/// </summary>
/// <param name="Start">Where the token starts in the text, counted in <see cref="char"/>s.</param>
/// <param name="Surface">The token's text, as it is in the text.</param>
/// <param name="IsUnknownToTheDictionary">Whether the dictionary doesn't have the word, so MeCab guessed its part of speech from its characters.</param>
/// <param name="PartOfSpeech">The part of speech, its four levels separated by commas, <see cref="Nothing"/> for a level not used, such as <c>名詞,一般,*,*</c>.</param>
/// <param name="InflectionType">The inflection type, such as IPADIC's <c>五段・ラ行</c> or UniDic's <c>五段-ラ行</c>, or <see cref="Nothing"/>.</param>
/// <param name="InflectionForm">The inflection form, such as IPADIC's <c>連用タ接続</c> or UniDic's <c>連用形-促音便</c>, or <see cref="Nothing"/>.</param>
/// <param name="BaseForm">The base form, as spelled in the text: 知ら's is 知る.</param>
/// <param name="Lemma">UniDic's lemma, its word for the token, uniting its spellings; <c>null</c> for IPADIC, which has none.</param>
public sealed record MeCabToken(
   int Start,
   string Surface,
   bool IsUnknownToTheDictionary,
   string PartOfSpeech,
   string InflectionType,
   string InflectionForm,
   string BaseForm,
   string? Lemma)
{
   /// <summary>MeCab's "nothing", for a field with no value.</summary>
   public const string Nothing = "*";

   /// <summary>Where the token ends in the text: the first <see cref="char"/> after it.</summary>
   public int End => Start + Surface.Length;

   /// <summary>
   /// The token's word, as its dictionary tells it: UniDic's lemma, and for IPADIC, which has no lemma, the base form. Two dictionaries'<br/>
   /// words compared tell which spellings the second unites that the first doesn't.
   /// </summary>
   public string Word => Lemma ?? BaseForm;

   /// <summary>
   /// The tokens of <paramref name="nodes"/>, the nodes MeCab parsed <paramref name="text"/> into, without the nodes that mark where the text<br/>
   /// begins and ends.
   /// </summary>
   /// <remarks>
   /// MeCab.DotNet leaves most nodes' <see cref="MeCabNode.BPos"/> and <see cref="MeCabNode.EPos"/> at 0, so where each token starts is counted:<br/>
   /// a node's <see cref="MeCabNode.RLength"/> is its <see cref="MeCabNode.Length"/> plus the whitespace MeCab skipped before it.
   /// </remarks>
   internal static IReadOnlyList<MeCabToken> EachOf(string text, IEnumerable<MeCabNode> nodes, TokenizerDictionaryFormat format)
   {
      var tokens = new List<MeCabToken>();
      var end = 0;
      foreach(var node in nodes)
      {
         if(node.Stat is MeCabNodeStat.Bos or MeCabNodeStat.Eos) continue;

         var start = end + node.RLength - node.Length;
         tokens.Add(format.TokenOf(node, start));
         end = start + node.Length;
      }

      Contract.State.Assert(end <= text.Length, () => $"The tokens of {text} end at {end}, past its end.");
      return tokens;
   }
}
