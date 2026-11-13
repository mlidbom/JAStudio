using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using JAStudio.Core.LanguageServices.Parsing.Tokenizing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Texts;


namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Tokens;

/// <summary>
/// The conditions there are on a token, such as a word's preceding token:
/// <list type="bullet">
/// <item>its part of speech, inflection type and inflected form, as the tokenizer gives them: <see cref="PartOfSpeechIs"/>, <see cref="InflectionTypeIsIn"/><br/>
/// and <see cref="InflectedFormIsIn"/>;</item>
/// <item>its texts: its <see cref="Surface"/>, <see cref="BaseForm"/> and <see cref="Reading"/>;</item>
/// <item>JAStudio's own facts about it, such as <see cref="IsAMasuStem"/>, or <see cref="IsADictionaryVerbInflection"/>, the ending JAStudio split off a<br/>
/// dictionary-form verb.</item>
/// </list>
/// </summary>
/// <remarks>
/// The facts are those <see cref="AnalysisToken"/> computes, by the same code, so a condition on one means exactly what the matching rule that<br/>
/// tested it before match-conditions meant (<see cref="Rules.MatchingRules"/>). On a piece JAStudio split off a token, they're the piece's own, not the token's.
/// </remarks>
public static class TokenConditions
{
   //review:matching: "token" for what MeCab calls a morpheme (形態素), and for the smaller pieces JAStudio splits off one, such as a
   //   dictionary-form verb's ending. Kept: language processing calls a tokenizer's output tokens, and the code says token everywhere
   //   (AnalysisToken, MeCabToken), so a rename would span the whole tokenizer.

   /// <summary>
   /// The token is there: it's not the empty token (<see cref="EmptyToken"/>) a match-condition sees where there is none, as before a sentence's first<br/>
   /// word. Written where a token is required, such as before a suffix: "the preceding token exists, and isn't a non-word character".
   /// </summary>
   public static readonly Fact<AnalysisToken> Exists = new(nameof(Exists), "exists", it => it is not EmptyToken);

   /// <summary>A godan verb, such as 読む (<see cref="AnalysisToken.IsGodanVerb"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAGodanVerb = new(nameof(IsAGodanVerb), "godanVerb", it => it.IsGodanVerb);

   /// <summary>An ichidan verb, such as 食べる (<see cref="AnalysisToken.IsIchidanVerb"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAnIchidanVerb = new(nameof(IsAnIchidanVerb), "ichidanVerb", it => it.IsIchidanVerb);

   /// <summary>A token in an irrealis form, 未然形, such as the 食べ of 食べない (<see cref="AnalysisToken.IsIrrealis"/>).</summary>
   public static readonly Fact<AnalysisToken> IsInAnIrrealisForm = new(nameof(IsInAnIrrealisForm), "irrealis", it => it.IsIrrealis);

   /// <summary>A verb in its continuative form, 連用形, the stem ます attaches to, such as the 食べ of 食べます (<see cref="AnalysisToken.IsMasuStem"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAMasuStem = new(nameof(IsAMasuStem), "masuStem", it => it.IsMasuStem);

   /// <summary>An adverb, or an i-adjective's adverbial く form (<see cref="AnalysisToken.IsAdverb"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAnAdverb = new(nameof(IsAnAdverb), "adverb", it => it.IsAdverb);

   /// <summary>A て (or で, てる, とる, …) that follows a verb's て-form stem (<see cref="AnalysisToken.HasTeFormStem"/>).</summary>
   public static readonly Fact<AnalysisToken> HasATeFormStem = new(nameof(HasATeFormStem), "teFormStem", it => it.HasTeFormStem);

   /// <summary>The stem JAStudio split off a dictionary-form verb, such as 食べ of 食べる.</summary>
   public static readonly Fact<AnalysisToken> IsADictionaryVerbFormStem = new(nameof(IsADictionaryVerbFormStem), "dictionaryVerbFormStem", it => it.IsDictionaryVerbFormStem);

   /// <summary>The ending JAStudio split off a dictionary-form verb, such as る of 食べる.</summary>
   public static readonly Fact<AnalysisToken> IsADictionaryVerbInflection = new(nameof(IsADictionaryVerbInflection), "dictionaryVerbInflection", it => it.IsDictionaryVerbInflection);

   /// <summary>The stem of a godan verb's potential form. Only a piece JAStudio split off a token is one (<see cref="AnalysisToken.IsGodanPotentialStem"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAGodanPotentialStem = new(nameof(IsAGodanPotentialStem), "godanPotentialStem", it => it.IsGodanPotentialStem);

   /// <summary>The inflection of a godan verb's potential form. Only a piece JAStudio split off a token is one (<see cref="AnalysisToken.IsGodanPotentialInflection"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAGodanPotentialInflection = new(nameof(IsAGodanPotentialInflection), "godanPotentialInflection", it => it.IsGodanPotentialInflection);

   /// <summary>The stem of a godan verb's imperative. Only a piece JAStudio split off a token is one (<see cref="AnalysisToken.IsGodanImperativeStem"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAGodanImperativeStem = new(nameof(IsAGodanImperativeStem), "godanImperativeStem", it => it.IsGodanImperativeStem);

   /// <summary>The inflection of a godan verb's imperative. Only a piece JAStudio split off a token is one (<see cref="AnalysisToken.IsGodanImperativeInflection"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAGodanImperativeInflection = new(nameof(IsAGodanImperativeInflection), "godanImperativeInflection", it => it.IsGodanImperativeInflection);

   /// <summary>The stem of an ichidan verb's imperative. Only a piece JAStudio split off a token is one (<see cref="AnalysisToken.IsIchidanImperativeStem"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAnIchidanImperativeStem = new(nameof(IsAnIchidanImperativeStem), "ichidanImperativeStem", it => it.IsIchidanImperativeStem);

   /// <summary>The inflection of an ichidan verb's imperative. Only a piece JAStudio split off a token is one (<see cref="AnalysisToken.IsIchidanImperativeInflection"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAnIchidanImperativeInflection = new(nameof(IsAnIchidanImperativeInflection), "ichidanImperativeInflection", it => it.IsIchidanImperativeInflection);

   /// <summary>A token in a continuative form before た with its sound change, such as 連用形-促音便 (<see cref="AnalysisToken.IsPastTenseStem"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAPastTenseStem = new(nameof(IsAPastTenseStem), "pastTenseStem", it => it.IsPastTenseStem);

   /// <summary>The た of a past tense, or its だ, as in 読んだ: inflection type 特殊・タ (<see cref="AnalysisToken.IsPastTenseMarker"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAPastTenseMarker = new(nameof(IsAPastTenseMarker), "pastTenseMarker", it => it.IsPastTenseMarker);

   /// <summary>
   /// Nothing follows the token but the sentence's end, a sentence-ending particle, a sentence-end character, or a non-word character<br/>
   /// (<see cref="AnalysisToken.IsEndOfStatement"/>).
   /// </summary>
   public static readonly Fact<AnalysisToken> EndsAStatement = new(nameof(EndsAStatement), "endOfStatement", it => it.IsEndOfStatement);

   /// <summary>A character that is no word, such as punctuation (<see cref="AnalysisToken.IsNonWordCharacter"/>).</summary>
   public static readonly Fact<AnalysisToken> IsANonWordCharacter = new(nameof(IsANonWordCharacter), "nonWordCharacter", it => it.IsNonWordCharacter);

   /// <summary>A word that inflects, such as a verb or an i-adjective (<see cref="AnalysisToken.IsInflectableWord"/>).</summary>
   public static readonly Fact<AnalysisToken> IsAnInflectableWord = new(nameof(IsAnInflectableWord), "inflectableWord", it => it.IsInflectableWord);

   /// <summary>The token's surface, its text as the sentence has it.</summary>
   public static readonly ConditionSubject<AnalysisToken, string> Surface = new(nameof(Surface), "surface", it => it.Surface, TextConditions.Level);

   /// <summary>The token's base form, the form a dictionary lists it by, such as 食べる for the 食べ of 食べた.</summary>
   public static readonly ConditionSubject<AnalysisToken, string> BaseForm = new(nameof(BaseForm), "baseForm", it => it.BaseForm, TextConditions.Level);

   /// <summary>The token's reading, in katakana, as the tokenizer gives it. A piece JAStudio split off a token has the reading of the whole token.</summary>
   public static readonly ConditionSubject<AnalysisToken, string> Reading = new(nameof(Reading), "reading", it => it.SourceToken.Reading, TextConditions.Level);

   /// <summary>The conditions on a token, and how they're read back from the corpus.</summary>
   public static readonly ConditionLevel<AnalysisToken> Level =
      new("a token",
          "Token",
          [
             Exists, IsAGodanVerb, IsAnIchidanVerb, IsInAnIrrealisForm, IsAMasuStem, IsAnAdverb, HasATeFormStem, IsADictionaryVerbFormStem, IsADictionaryVerbInflection,
             IsAGodanPotentialStem, IsAGodanPotentialInflection, IsAGodanImperativeStem, IsAGodanImperativeInflection, IsAnIchidanImperativeStem,
             IsAnIchidanImperativeInflection, IsAPastTenseStem, IsAPastTenseMarker, EndsAStatement, IsANonWordCharacter, IsAnInflectableWord
          ],
          [Surface, BaseForm, Reading],
          [
             ConditionTestWithTexts.Of("PartOfSpeech.Is",
                                       "Pick the part of speech, or the kind of one, finding it by its Japanese or its English, such as 接尾辞 or suffix",
                                       PartOfSpeechIs.WhyTypedLevelsAreNoPartOfSpeech,
                                       PartOfSpeechIs.FromTyped,
                                       it => it.JapaneseLevels,
                                       PartOfSpeechIs.PickableValues,
                                       picksOne: true),
             ConditionTestWithTexts.Of("InflectionType.In",
                                       "Pick the inflection types, or their families, finding each by its Japanese or its English, such as 五段 or godan",
                                       InflectionTypeIsIn.WhyTypedNamesMakeNoCondition,
                                       InflectionTypeIsIn.FromTyped,
                                       it => it.Names.AsTyped,
                                       InflectionTypeIsIn.PickableValues),
             ConditionTestWithTexts.Of("InflectedForm.In",
                                       "Pick the inflected forms, finding each by its Japanese or its English, such as 連用形-促音便 or sound change",
                                       InflectedFormIsIn.WhyTypedNamesMakeNoCondition,
                                       InflectedFormIsIn.FromTyped,
                                       it => it.Names.AsTyped,
                                       InflectedFormIsIn.PickableValues)
          ],
          new Dictionary<string, Func<JsonNode, Condition<AnalysisToken>>>
          {
             [PartOfSpeechIs.JsonKey] = json => new PartOfSpeechIs(json.GetValue<string>()),
             [InflectionTypeIsIn.JsonKey] = json => new InflectionTypeIsIn(TextSet.From(json)),
             [InflectedFormIsIn.JsonKey] = json => new InflectedFormIsIn(TextSet.From(json))
          });

   /// <summary>The token's part of speech, for writing a condition on a token as its code reads, inside a group on it: <c>PartOfSpeech.Is("動詞")</c>.</summary>
   public static PartOfSpeechTest<AnalysisToken> PartOfSpeech => new(ConditionPath.Itself<AnalysisToken>());

   /// <summary>The token's inflection type, for writing a condition on a token as its code reads: <c>InflectionType.In("五段")</c>.</summary>
   public static InflectionTypeTest<AnalysisToken> InflectionType => new(ConditionPath.Itself<AnalysisToken>());

   /// <summary>The token's inflected form, for writing a condition on a token as its code reads: <c>InflectedForm.In("連用形")</c>.</summary>
   public static InflectedFormTest<AnalysisToken> InflectedForm => new(ConditionPath.Itself<AnalysisToken>());
}
