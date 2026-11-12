using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A value from one of the tokenizer's fixed lists, in Japanese as the tokenizer gives it, with its English: a part of speech or a kind of one, such as<br/>
/// 名詞・接尾・人名 with noun › suffix › person-name, an inflection type or a family of them, or an inflected form.<br/>
/// A condition's code shows it unquoted, with its English as a comment (<see cref="ConditionCode"/>), and the tree editor offers it to<br/>
/// pick (<see cref="ConditionTestWithTexts.PickableValues"/>), found by its Japanese or its English.
/// </summary>
public sealed record TokenizerValue(string Value, string English)
{
   //review:matching: TokenizerValue was PickableValue, named for the picker alone. A condition's code lists the same values with the same English,
   //   which it took as a (Value, English) tuple: one type for the two now, named for what it is.
}
