using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>
/// The conditions on a token, written in C# as a condition's code reads (<see cref="Code.ConditionCode"/>), on whatever leads to a token: a word's<br/>
/// <see cref="Words.WordConditions.PrecedingToken"/> and the other tokens of <see cref="Words.WordConditions"/>, so that<br/>
/// <c>PrecedingToken.IsAGodanVerb</c>, <c>PrecedingToken.Surface.EndsWithOneOf("ん")</c> and <c>FollowingToken.PartOfSpeech.Is("名詞", "数")</c> are C#<br/>
/// as they stand. Each forwards to its condition in <see cref="TokenConditions"/>, which defines it.
/// </summary>
public static class TokenConditionsInCode
{
   //review:matching: "InCode", for the conditions as C# writes them, beside TokenConditions, which defines them, and the code the editor shows
   //   (ConditionCode). TextConditionsInCode is named alike. "Fluent" would say how, not what.
   extension<TFrom>(IConditionPath<TFrom, IAnalysisToken> token) where TFrom : class
   {
      /// <summary>The token meets <see cref="TokenConditions.Exists"/>.</summary>
      public Condition<TFrom> Exists => token.Placed(TokenConditions.Exists);

      /// <summary>The token meets <see cref="TokenConditions.IsAGodanVerb"/>.</summary>
      public Condition<TFrom> IsAGodanVerb => token.Placed(TokenConditions.IsAGodanVerb);

      /// <summary>The token meets <see cref="TokenConditions.IsAnIchidanVerb"/>.</summary>
      public Condition<TFrom> IsAnIchidanVerb => token.Placed(TokenConditions.IsAnIchidanVerb);

      /// <summary>The token meets <see cref="TokenConditions.IsInAnIrrealisForm"/>.</summary>
      public Condition<TFrom> IsInAnIrrealisForm => token.Placed(TokenConditions.IsInAnIrrealisForm);

      /// <summary>The token meets <see cref="TokenConditions.IsAMasuStem"/>.</summary>
      public Condition<TFrom> IsAMasuStem => token.Placed(TokenConditions.IsAMasuStem);

      /// <summary>The token meets <see cref="TokenConditions.IsAnAdverb"/>.</summary>
      public Condition<TFrom> IsAnAdverb => token.Placed(TokenConditions.IsAnAdverb);

      /// <summary>The token meets <see cref="TokenConditions.HasATeFormStem"/>.</summary>
      public Condition<TFrom> HasATeFormStem => token.Placed(TokenConditions.HasATeFormStem);

      /// <summary>The token meets <see cref="TokenConditions.IsADictionaryVerbFormStem"/>.</summary>
      public Condition<TFrom> IsADictionaryVerbFormStem => token.Placed(TokenConditions.IsADictionaryVerbFormStem);

      /// <summary>The token meets <see cref="TokenConditions.IsADictionaryVerbInflection"/>.</summary>
      public Condition<TFrom> IsADictionaryVerbInflection => token.Placed(TokenConditions.IsADictionaryVerbInflection);

      /// <summary>The token meets <see cref="TokenConditions.IsAGodanPotentialStem"/>.</summary>
      public Condition<TFrom> IsAGodanPotentialStem => token.Placed(TokenConditions.IsAGodanPotentialStem);

      /// <summary>The token meets <see cref="TokenConditions.IsAGodanPotentialInflection"/>.</summary>
      public Condition<TFrom> IsAGodanPotentialInflection => token.Placed(TokenConditions.IsAGodanPotentialInflection);

      /// <summary>The token meets <see cref="TokenConditions.IsAGodanImperativeStem"/>.</summary>
      public Condition<TFrom> IsAGodanImperativeStem => token.Placed(TokenConditions.IsAGodanImperativeStem);

      /// <summary>The token meets <see cref="TokenConditions.IsAGodanImperativeInflection"/>.</summary>
      public Condition<TFrom> IsAGodanImperativeInflection => token.Placed(TokenConditions.IsAGodanImperativeInflection);

      /// <summary>The token meets <see cref="TokenConditions.IsAnIchidanImperativeStem"/>.</summary>
      public Condition<TFrom> IsAnIchidanImperativeStem => token.Placed(TokenConditions.IsAnIchidanImperativeStem);

      /// <summary>The token meets <see cref="TokenConditions.IsAnIchidanImperativeInflection"/>.</summary>
      public Condition<TFrom> IsAnIchidanImperativeInflection => token.Placed(TokenConditions.IsAnIchidanImperativeInflection);

      /// <summary>The token meets <see cref="TokenConditions.IsAPastTenseStem"/>.</summary>
      public Condition<TFrom> IsAPastTenseStem => token.Placed(TokenConditions.IsAPastTenseStem);

      /// <summary>The token meets <see cref="TokenConditions.IsAPastTenseMarker"/>.</summary>
      public Condition<TFrom> IsAPastTenseMarker => token.Placed(TokenConditions.IsAPastTenseMarker);

      /// <summary>The token meets <see cref="TokenConditions.EndsAStatement"/>.</summary>
      public Condition<TFrom> EndsAStatement => token.Placed(TokenConditions.EndsAStatement);

      /// <summary>The token meets <see cref="TokenConditions.IsANonWordCharacter"/>.</summary>
      public Condition<TFrom> IsANonWordCharacter => token.Placed(TokenConditions.IsANonWordCharacter);

      /// <summary>The token meets <see cref="TokenConditions.IsAnInflectableWord"/>.</summary>
      public Condition<TFrom> IsAnInflectableWord => token.Placed(TokenConditions.IsAnInflectableWord);

      /// <summary>The token's surface (<see cref="TokenConditions.Surface"/>), for a condition on it, such as <c>Surface.EndsWithOneOf("ん")</c>.</summary>
      public IConditionPath<TFrom, string> Surface => ConditionPath<TFrom, string>.Through(token, TokenConditions.Surface);

      /// <summary>The token's base form (<see cref="TokenConditions.BaseForm"/>), for a condition on it.</summary>
      public IConditionPath<TFrom, string> BaseForm => ConditionPath<TFrom, string>.Through(token, TokenConditions.BaseForm);

      /// <summary>The token's reading (<see cref="TokenConditions.Reading"/>), for a condition on it.</summary>
      public IConditionPath<TFrom, string> Reading => ConditionPath<TFrom, string>.Through(token, TokenConditions.Reading);

      /// <summary>The token's part of speech, for <see cref="PartOfSpeechTest{TFrom}.Is"/>.</summary>
      public PartOfSpeechTest<TFrom> PartOfSpeech => new(token);

      /// <summary>The token's inflection type, for <see cref="InflectionTypeTest{TFrom}.In"/>.</summary>
      public InflectionTypeTest<TFrom> InflectionType => new(token);

      /// <summary>The token's inflected form, for <see cref="InflectedFormTest{TFrom}.In"/>.</summary>
      public InflectedFormTest<TFrom> InflectedForm => new(token);

      /// <summary>The token meets all of <paramref name="conditions"/>, conditions on a token.</summary>
      public Condition<TFrom> AllOf(params Condition<IAnalysisToken>[] conditions) => token.Placed(ConditionGroup.AllOf(conditions));

      /// <summary>The token meets any of <paramref name="conditions"/>, conditions on a token.</summary>
      public Condition<TFrom> AnyOf(params Condition<IAnalysisToken>[] conditions) => token.Placed(ConditionGroup.AnyOf(conditions));
   }
}
