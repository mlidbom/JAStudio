using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>A token a sentence's analysis works on, as <see cref="AnalysisTools.AnalyzeSentence"/> reports it.</summary>
/// <param name="PartsOfSpeech">The tokenizer's parts of speech for the token it came from, most general first, such as <c>動詞・一般</c>.</param>
/// <param name="InflectionType">The tokenizer's inflection type for the token it came from, such as <c>五段-カ行</c>, or <c>*</c> for none.</param>
/// <param name="InflectedForm">The tokenizer's inflected form for the token it came from, such as <c>終止形-一般</c>, or <c>*</c> for none.</param>
/// <param name="MadeFrom">
/// The surfaces of the tokenizer's tokens JAStudio's pre-processing made this token from, separated by spaces: the token it split this off, or the<br/>
/// tokens it corrected into this (<see cref="CorrectedToken"/>). None when it's the tokenizer's own.
/// </param>
public record AnalyzedToken(string Surface, string BaseForm, string PartsOfSpeech, string InflectionType, string InflectedForm, string? MadeFrom)
{
   public static AnalyzedToken Of(IAnalysisToken token)
   {
      var tokenizerToken = token.SourceToken;
      return new AnalyzedToken(token.Surface,
                               token.BaseForm,
                               PartsOfSpeechOf(tokenizerToken.PartsOfSpeech),
                               tokenizerToken.InflectionType.Name,
                               tokenizerToken.InflectedForm.Name,
                               token.IsTheTokenizersOwn ? null : string.Join(" ", TheTokenizersTokensOf(tokenizerToken).Select(it => it.Surface)));
   }

   /// <summary>The tokenizer's tokens <paramref name="token"/> stands for: those a correction put it in place of, or the token itself.</summary>
   static IReadOnlyList<JNToken> TheTokenizersTokensOf(JNToken token) => token is CorrectedToken corrected ? corrected.TokenizedAs : [token];

   static string PartsOfSpeechOf(JNPartsOfSpeech partsOfSpeech) =>
      string.Join("・", new[] { partsOfSpeech.Level1, partsOfSpeech.Level2, partsOfSpeech.Level3, partsOfSpeech.Level4 }
                          .Select(it => it.Japanese)
                          .Where(it => it != "*" && it != ""));
}
