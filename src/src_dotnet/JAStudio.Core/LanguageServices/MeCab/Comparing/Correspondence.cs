namespace JAStudio.Core.LanguageServices.MeCab.Comparing;

/// <summary>
/// What two tokenizer dictionaries say of a token both cut alike, in one respect, such as its part of speech: IPADIC's <c>名詞,一般,*,*</c><br/>
/// against UniDic's <c>名詞,普通名詞,一般,*</c>. Counted over many tokens, they show how one dictionary's descriptions map to the other's.
/// </summary>
/// <param name="First">What the first dictionary says.</param>
/// <param name="Second">What the second dictionary says.</param>
public sealed record Correspondence(string First, string Second);
