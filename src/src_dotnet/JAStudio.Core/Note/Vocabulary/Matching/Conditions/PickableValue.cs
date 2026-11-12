namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// A value a condition-test offers to pick from (<see cref="ConditionTestWithTexts.PickableValues"/>), one of the tokenizer's, such as the part of speech<br/>
/// 名詞・接尾・人名, with its English, such as noun › suffix › person-name, by which it's found as well as by its Japanese.
/// </summary>
public sealed record PickableValue(string Value, string English);
