using JAStudio.Core.LanguageServices.Parsing.Tokenizing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>A token's part of speech in C#, for writing a condition on it as its code reads: <c>FollowingToken.PartOfSpeech.Is("名詞", "数")</c>.</summary>
public sealed class PartOfSpeechTest<TFrom> where TFrom : class
{
   readonly IConditionPath<TFrom, AnalysisToken> _token;

   internal PartOfSpeechTest(IConditionPath<TFrom, AnalysisToken> token) => _token = token;

   /// <summary>The token's part of speech is <paramref name="japaneseLevels"/>, its levels from the top, or a kind of it (<see cref="PartOfSpeechIs"/>).</summary>
   public Condition<TFrom> Is(params string[] japaneseLevels) => _token.Placed(new PartOfSpeechIs(japaneseLevels));
}
