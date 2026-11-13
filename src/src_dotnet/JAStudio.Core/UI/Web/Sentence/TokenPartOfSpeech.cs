namespace JAStudio.Core.UI.Web.Sentence;

/// <summary>One level of the parts of speech the tokenizer gives a token (<see cref="TokenViewModel.PartsOfSpeech"/>), in Japanese and in English.</summary>
public sealed record TokenPartOfSpeech(string Japanese, string English)
{
   /// <summary>The Japanese, then the English, as the token tables show them, such as <c>名詞:noun</c>.</summary>
   public string InJapaneseAndEnglish => $"{Japanese}:{English}";
}
