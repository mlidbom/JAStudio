using Compze.Internals.SystemCE;
using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public class JNToken : IAnalysisToken
{
   public const string SplitterTokenText = "removethissplittertoken";

   readonly string _baseForm;
   readonly string _surface;

   public JNToken(
      JNPartsOfSpeech partsOfSpeech,
      string baseForm,
      string surface,
      InflectionType? inflectionType = null,
      InflectionForm? inflectedForm = null,
      string reading = "",
      string phonetic = "",
      string lemma = "")
   {
      _baseForm = baseForm;
      _surface = surface;
      InflectionType = inflectionType ?? InflectionTypes.Unknown;
      InflectedForm = inflectedForm ?? InflectionForms.Unknown;
      Reading = reading;
      Phonetic = phonetic;
      Lemma = lemma;
      PartsOfSpeech = partsOfSpeech;
   }

   public InflectionType InflectionType { get; }
   public InflectionForm InflectedForm { get; }

   /// <summary>The token's reading, in katakana, as it's written: は's is ハ.</summary>
   public string Reading { get; }

   /// <summary>The token's pronunciation, in katakana: は's is ワ.</summary>
   public string Phonetic { get; }

   /// <summary>
   /// The tokenizer dictionary's lemma, its word for the token, uniting the word's spellings: ボク's is 僕, and the 書け of 書ける, a potential form, has 書く.<br/>
   /// Spelled the dictionary's way, sometimes with a sub-classification after a hyphen, such as 僕-代名詞; a proper noun's is its reading in katakana.
   /// </summary>
   public string Lemma { get; }

   public JNPartsOfSpeech PartsOfSpeech { get; }

   public JNToken? Next { get; internal set; }

   public JNToken? Previous { get; internal set; }

   public override string ToString() =>
      string.Concat(
         "JNToken(",
         KanaUtils.PadToLength($"'{BaseForm}'", 6),
         ", ",
         KanaUtils.PadToLength($"'{Surface}'", 6),
         ", ",
         KanaUtils.PadToLength($"'{InflectionType}'", 6),
         ", ",
         KanaUtils.PadToLength($"'{InflectedForm}'", 10),
         ", ",
         PartsOfSpeech.ToString()
      );

   public override bool Equals(object? obj)
   {
      if(obj is JNToken other)
      {
         return BaseForm == other.BaseForm &&
                Surface == other.Surface &&
                Equals(InflectionType, other.InflectionType) &&
                Equals(InflectedForm, other.InflectedForm) &&
                Equals(PartsOfSpeech, other.PartsOfSpeech);
      }

      return false;
   }

   public override int GetHashCode() => HashCode.Combine(BaseForm, Surface, InflectionType, InflectedForm, PartsOfSpeech);

   // <IAnalysisToken implementation>
   public override bool IsPastTenseStem => InflectionForms.IsAPastTenseStem(InflectedForm);
   public override bool IsPastTenseMarker => Equals(InflectionType, InflectionTypes.Auxiliary.Ta);

   public override bool IsMasuStem => Equals(InflectedForm, InflectionForms.Continuative) && IsVerb;

   public override bool IsAdverb =>
      Equals(PartsOfSpeech, JNPOS.Adverb) ||
      (IsAdjective && Surface.EndsWithOrdinal("く"));

   public override string Surface => _surface;
   public override string BaseForm => _baseForm;

   static readonly HashSet<string> PseudoVerbsForInflectionPurposes = ["ます"];

   public override bool IsInflectableWord =>
      IsVerb || IsAdjective || PseudoVerbsForInflectionPurposes.Contains(BaseForm);

   public override bool IsNonWordCharacter => PartsOfSpeech.IsNonWordCharacter();
   public override bool IsIrrealis => InflectionForms.IsIrrealis(InflectedForm);
   public override JNToken SourceToken => this;
   // </IAnalysisToken implementation>

   /// <summary>
   /// A verb, or an auxiliary that inflects as one, such as てる, ちゃう, れる and させる, which IPADIC, the tokenizer's former dictionary, had as<br/>
   /// verbs and UniDic has as auxiliaries, or ます.
   /// </summary>
   public bool IsVerb =>
      JNPOS.Verb.AllTypes.Contains(PartsOfSpeech) ||
      Equals(PartsOfSpeech, JNPOS.Suffix.VerbLike) ||
      (Equals(PartsOfSpeech, JNPOS.BoundAuxiliary) && InflectsAsAVerb) ||
      Equals(InflectionType, InflectionTypes.Auxiliary.Masu);

   bool InflectsAsAVerb => IsGodanVerb || IsIchidanVerb || IsKuruVerb || IsSuruVerb;

   /// <summary>An i-adjective, or a suffix that makes one, such as っぽい.</summary>
   public bool IsAdjective => JNPOS.Adjective.AllTypes.Contains(PartsOfSpeech) || Equals(PartsOfSpeech, JNPOS.Suffix.AdjectiveLike);

   public override bool IsIchidanVerb => InflectionTypes.Ichidan.Includes(InflectionType);

   public override bool IsGodanVerb => InflectionType.Family == InflectionTypes.Godan.Family;

   public bool IsKuruVerb => InflectionType.Family == InflectionTypes.Kahen.Family;

   public bool IsSuruVerb => InflectionType.Family == InflectionTypes.Sahen.Family;

   public bool IsDictionaryForm() => InflectionForms.IsDictionaryForm(InflectedForm);

   static readonly HashSet<string> ProgressiveForms = ["でる", "どる", "てる", "とる", "とん"];
   static readonly HashSet<string> TeForms = ["て", "って", "で"];
   static readonly HashSet<string> PossibleHasTeFormStemTokenSurfaces;

   static JNToken()
   {
      PossibleHasTeFormStemTokenSurfaces = new HashSet<string>(["て", "って", "で", "てる", "てん", "とん"]);
      PossibleHasTeFormStemTokenSurfaces.UnionWith(ProgressiveForms);
   }

   public override bool HasTeFormStem
   {
      get
      {
         if(!PossibleHasTeFormStemTokenSurfaces.Contains(Surface))
         {
            return false;
         }

         var previous = Previous;
         if(previous == null)
         {
            return false;
         }

         if(Equals(previous.InflectionType, InflectionTypes.Auxiliary.Nai))
         {
            return false;
         }

         if(previous.IsTheContinuativeOfAnAdjectiveBeforeTe)
         {
            return true;
         }

         if(Equals(PartsOfSpeech, JNPOS.Particle.Conjunctive))
         {
            return true;
         }

         if((previous.IsPastTenseStem || previous.IsMasuStem) &&
            (ProgressiveForms.Contains(BaseForm) || ProgressiveForms.Contains(Surface)))
         {
            return true;
         }

         return false;
      }
   }

   /// <summary>The く continuative of an i-adjective or of たい, which て follows: the 良く of 良くて, the たく of 食べたくて.</summary>
   bool IsTheContinuativeOfAnAdjectiveBeforeTe =>
      (IsAdjective || Equals(InflectionType, InflectionTypes.Auxiliary.Tai)) &&
      Equals(InflectedForm, InflectionForms.Continuative) &&
      Surface.EndsWithOrdinal("く");

   public bool IsTeForm =>
      Equals(PartsOfSpeech, JNPOS.Particle.Conjunctive) && TeForms.Contains(Surface);

   public bool IsProgressiveForm() =>
      Surface == "てる" ||
      (ProgressiveForms.Contains(Surface) && Previous is { IsPastTenseStem: true }) ||
      (Surface == "いる" && Previous != null && Previous.IsTeForm);

   public override bool IsEndOfStatement =>
      Next == null ||
      Equals(Next.PartsOfSpeech, JNPOS.Particle.SentenceEnding) ||
      AnalysisConstants.SentenceEndCharacters.Contains(Next.Surface) ||
      Next.IsNonWordCharacter;

   static readonly HashSet<string> InvalidIchidanInflectionSurfaces = ["っ"];

   public bool CannotFollowIchidanStem() => InvalidIchidanInflectionSurfaces.Contains(Surface);

   /// <summary>A noun or a pronoun, which follow an imperative, as in 行け夏目, more often than an ichidan verb's stem.</summary>
   public bool IsMoreLikelyToFollowImperativeThanIchidanStem() => PartsOfSpeech.IsNoun();

   static readonly HashSet<string> InvalidGodanPotentialFormSurfaces = ["っ", "う"];

   /// <summary>The coordinating たり and だり, as in 泳げたり, which UniDic has as adverbial particles where IPADIC had coordinating ones.</summary>
   static readonly HashSet<string> CoordinatingTari = ["たり", "だり"];

   // Initialized via property to defer to after JNPOS is initialized
   static HashSet<JNPartsOfSpeech>? _validPotentialFormInflectionsPos;

   static HashSet<JNPartsOfSpeech> ValidPotentialFormInflectionsPos =>
      _validPotentialFormInflectionsPos ??=
      [
         JNPOS.BoundAuxiliary,
         JNPOS.AdjectivalNoun.AuxiliaryVerbStem,
         JNPOS.Particle.Conjunctive
      ];

   /// <summary>
   /// Whether this token can follow the stem of a godan verb's potential form, as ない, そう, て and たり follow 泳げ. Not the passive れる and られる,<br/>
   /// nor the causative せる and させる: they follow a verb's irrealis, as in 清められ, and IPADIC, the tokenizer's former dictionary, had them as<br/>
   /// verb suffixes, which this never took, where UniDic has them as auxiliaries.
   /// </summary>
   public bool IsValidGodanPotentialFormInflection() =>
      (ValidPotentialFormInflectionsPos.Contains(PartsOfSpeech) || (Equals(PartsOfSpeech, JNPOS.Particle.Adverbial) && CoordinatingTari.Contains(Surface))) &&
      !InvalidGodanPotentialFormSurfaces.Contains(Surface) &&
      !IsThePassiveOrTheCausative;

   static readonly HashSet<string> TheCausatives = ["せる", "させる"];

   bool IsThePassiveOrTheCausative => Equals(InflectionType, InflectionTypes.Auxiliary.Reru) || TheCausatives.Contains(BaseForm);
}
