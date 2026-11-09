using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Mcp.Tools.Analysis;

/// <summary>A token a sentence's analysis works on, as <see cref="AnalysisTools.AnalyzeSentence"/> reports it.</summary>
/// <param name="PartsOfSpeech">The tokenizer's parts of speech for the token it came from, most general first, such as <c>動詞・自立</c>.</param>
/// <param name="InflectionType">The tokenizer's inflection type for the token it came from, such as <c>五段・カ行イ音便</c>, or <c>*</c> for none.</param>
/// <param name="InflectedForm">The tokenizer's inflected form for the token it came from, such as <c>基本形</c>, or <c>*</c> for none.</param>
/// <param name="SplitFrom">The surface of the tokenizer's token that JAStudio's preprocessing split this token from, or none when it's the tokenizer's own.</param>
public record AnalyzedToken(string Surface, string BaseForm, string PartsOfSpeech, string InflectionType, string InflectedForm, string? SplitFrom)
{
   public static AnalyzedToken Of(IAnalysisToken token)
   {
      var tokenizerToken = token.SourceToken;
      return new AnalyzedToken(token.Surface,
                               token.BaseForm,
                               PartsOfSpeechOf(tokenizerToken.PartsOfSpeech),
                               tokenizerToken.InflectionType.Name,
                               tokenizerToken.InflectedForm.Name,
                               token is JNToken ? null : tokenizerToken.Surface);
   }

   static string PartsOfSpeechOf(JNPartsOfSpeech partsOfSpeech) =>
      string.Join("・", new[] { partsOfSpeech.Level1, partsOfSpeech.Level2, partsOfSpeech.Level3, partsOfSpeech.Level4 }
                          .Select(it => it.Japanese)
                          .Where(it => it != "*" && it != ""));
}
