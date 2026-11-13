using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>A token's inflected form in C#, for writing a condition on it as its code reads: <c>LastToken.InflectedForm.In("連用形-一般", "連用形-促音便")</c>.</summary>
public sealed class InflectedFormTest<TFrom> where TFrom : class
{
   readonly IConditionPath<TFrom, IAnalysisToken> _token;

   internal InflectedFormTest(IConditionPath<TFrom, IAnalysisToken> token) => _token = token;

   /// <summary>The token's inflected form is one of <paramref name="names"/> (<see cref="InflectedFormIsIn"/>).</summary>
   public Condition<TFrom> In(params string[] names) => _token.Placed(new InflectedFormIsIn(new TextSet(names)));
}
