using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>A token's inflected form in C#, for writing a condition on it as its code reads: <c>LastToken.InflectedForm.In("連用形", "連用タ接続")</c>.</summary>
public sealed class InflectedFormTest<TFrom> where TFrom : class
{
   readonly IConditionPath<TFrom, IAnalysisToken> _token;

   internal InflectedFormTest(IConditionPath<TFrom, IAnalysisToken> token) => _token = token;

   /// <summary>The token's inflected form is one of <paramref name="names"/> (<see cref="InflectedFormIsOneOf"/>).</summary>
   public Condition<TFrom> In(params string[] names) => _token.Placed(TokenConditions.InflectedFormIsOneOf(names));
}
