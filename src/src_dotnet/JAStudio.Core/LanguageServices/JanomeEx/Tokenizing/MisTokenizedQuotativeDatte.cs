using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compze.Contracts;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// A だって that the tokenizer produced as a single "even" particle, where it is actually the copula だ followed by the
/// quoting particle って, as in 友達だって言えばよかった ("should have said that (I'm) a friend").<br/>
/// <see cref="JNTokenizer"/> corrects it by inserting a <see cref="JNToken.SplitterTokenText"/> between だ and って and
/// tokenizing again, so that the tokenizer itself produces correct だ and って tokens.
/// </summary>
/// <remarks>
/// The phrasing is very common, and once the tokens are produced the mistake cannot be corrected further down the analysis:
/// the sentence shows a だって that is not there and loses compounds such as って言う, って思う and って聞く.
/// </remarks>
/// <remarks>
/// A だって directly followed by one of <see cref="BaseFormsOfVerbsOfSayingThinkingOrHearing"/> is treated as mis-tokenized.
/// Every such sentence in the sentence database (about 45 out of about 49,000 sentences) is the quoting phrasing.<br/>
/// The sentence-initial conjunction だって (だって言ったじゃん, "but I said so!") is left alone because the tokenizer gives
/// it a conjunction part of speech rather than <see cref="JNPOS.Particle.Adverbial"/>.<br/>
/// Verbs such as 分かる, 知る and 信じる are left out of the list because they commonly follow the genuine "even" particle:
/// 俺だってわかんねぇよ ("even I don't know"), 子供だって知ってる ("even a child knows").<br/>
/// Known misfire: 俺だって言うよ ("even I would say so") is split, because it is indistinguishable from the quoting phrasing
/// without understanding the meaning. No such sentence exists in the sentence database.
/// </remarks>
static class MisTokenizedQuotativeDatte
{
   const string Datte = "だって";
   const string DatteWithSplitterTokenBetweenDaAndTte = $"だ{JNToken.SplitterTokenText}って";

   static readonly HashSet<string> BaseFormsOfVerbsOfSayingThinkingOrHearing = ["言う", "いう", "言える", "思う", "思える", "思い込む", "聞く"];

   public static bool AnyIn(List<JNToken> tokens) => tokens.Any(IsMisTokenizedQuotativeDatte);

   /// <summary>
   /// Returns <paramref name="tokenizerInput"/> with a <see cref="JNToken.SplitterTokenText"/> inserted between だ and って
   /// in every mis-tokenized quotative だって among the <paramref name="tokens"/> the tokenizer produced from it.
   /// </summary>
   public static string SplitDaFromTteInEach(string tokenizerInput, List<JNToken> tokens)
   {
      var result = new StringBuilder();
      var positionAfterPreviousToken = 0;
      foreach(var token in tokens)
      {
         var tokenStart = StartOf(token, positionAfterPreviousToken);
         result.Append(tokenizerInput, positionAfterPreviousToken, tokenStart - positionAfterPreviousToken);
         result.Append(IsMisTokenizedQuotativeDatte(token) ? DatteWithSplitterTokenBetweenDaAndTte : token.Surface);
         positionAfterPreviousToken = tokenStart + token.Surface.Length;
      }

      result.Append(tokenizerInput, positionAfterPreviousToken, tokenizerInput.Length - positionAfterPreviousToken);
      return result.ToString();

      int StartOf(JNToken token, int searchStart)
      {
         var tokenStart = tokenizerInput.IndexOf(token.Surface, searchStart, StringComparison.Ordinal);
         Contract.State.Assert(tokenStart >= 0, () => $"Token surface '{token.Surface}' not found in tokenizer input '{tokenizerInput}' after position {searchStart}");
         return tokenStart;
      }
   }

   static bool IsMisTokenizedQuotativeDatte(JNToken token) =>
      token.Surface == Datte
      && Equals(token.PartsOfSpeech, JNPOS.Particle.Adverbial)
      && token.Next is {} nextToken
      && BaseFormsOfVerbsOfSayingThinkingOrHearing.Contains(nextToken.BaseForm);
}
