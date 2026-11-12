using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>
/// The token a word's match-condition sees where there is no token: before a sentence's first word, as its <see cref="Words.WordConditions.PrecedingToken"/>,<br/>
/// and after its last, as its <see cref="Words.WordConditions.FollowingToken"/>. It has no part of speech, no inflection and none of JAStudio's facts, and<br/>
/// its texts are empty. Every condition is tested on it exactly as written: "the preceding token is a godan verb" isn't met by it, and "the<br/>
/// preceding token isn't a godan verb" is. <see cref="TokenConditions.Exists"/> tells it from a token there is.
/// </summary>
/// <remarks>
/// A missing token used to make any condition on it false without testing it, so a negation inside it, written there or in a definition it uses,<br/>
/// was never tested at a sentence's edges. Magnus found the outcomes "utterly insane" (2026-11-12): see <c>dev_docs/matching-configuration.md</c>, "A<br/>
/// missing token: the empty token".
/// </remarks>
public sealed class EmptyToken : IAnalysisToken
{
   EmptyToken() {}

   /// <summary>The empty token: there's one, as there's nothing to tell one from another.</summary>
   public static EmptyToken Instance { get; } = new();

   /// <summary>The tokenizer's token the empty token stands for, for the conditions that read one: no part of speech, no inflection and no reading.</summary>
   static readonly JNToken NoToken = new(JNPartsOfSpeech.None, baseForm: "", surface: "");

   public override string Surface => "";
   public override string BaseForm => "";
   public override JNToken SourceToken => NoToken;

   public override bool IsIchidanVerb => false;
   public override bool IsGodanVerb => false;
   public override bool IsPastTenseStem => false;
   public override bool IsPastTenseMarker => false;
   public override bool IsMasuStem => false;
   public override bool IsAdverb => false;
   public override bool IsIrrealis => false;
   public override bool IsEndOfStatement => false;
   public override bool HasTeFormStem => false;
   public override bool IsInflectableWord => false;
   public override bool IsNonWordCharacter => false;
   public override bool IsDictionaryVerbFormStem => false;
   public override bool IsDictionaryVerbInflection => false;
   public override bool IsGodanPotentialStem => false;
   public override bool IsGodanImperativeStem => false;
   public override bool IsIchidanImperativeStem => false;
   public override bool IsGodanPotentialInflection => false;
   public override bool IsGodanImperativeInflection => false;
   public override bool IsIchidanImperativeInflection => false;

   public override string ToString() => nameof(EmptyToken);
}
