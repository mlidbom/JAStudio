using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>A token's inflection type in C#, for writing a condition on it as its code reads: <c>LastToken.InflectionType.In("五段")</c>.</summary>
public sealed class InflectionTypeTest<TFrom> where TFrom : class
{
   readonly IConditionPath<TFrom, IAnalysisToken> _token;

   internal InflectionTypeTest(IConditionPath<TFrom, IAnalysisToken> token) => _token = token;

   /// <summary>The token's inflection type is one of <paramref name="names"/>, or in the family of one (<see cref="InflectionTypeIsOneOf"/>).</summary>
   public Condition<TFrom> In(params string[] names) => _token.Placed(TokenConditions.InflectionTypeIsOneOf(names));
}
