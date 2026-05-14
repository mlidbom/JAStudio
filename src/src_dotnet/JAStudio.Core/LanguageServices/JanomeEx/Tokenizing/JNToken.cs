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
      object? inflectionType = null,
      object? inflectedForm = null,
      string reading = "",
      string phonetic = "",
      string nodeType = "")
   {
      _baseForm = baseForm;
      _surface = surface;

      // Python defaults to "*" when not provided
      inflectionType ??= "*";
      inflectedForm ??= "*";

      InflectionType = inflectionType is string str1
                          ? InflectionTypes.GetByName(str1)
                          : (InflectionType)inflectionType;

      InflectedForm = inflectedForm is string str2
                         ? InflectionForms.GetByName(str2)
                         : (InflectionForm)inflectedForm;

      Reading = reading;
      Phonetic = phonetic;
      NodeType = nodeType;
      PartsOfSpeech = partsOfSpeech;
   }

   public InflectionType InflectionType { get; }
   public InflectionForm InflectedForm { get; }
   // ReSharper disable once UnusedAutoPropertyAccessor.Global
   public string Reading { get; }
   // ReSharper disable once UnusedAutoPropertyAccessor.Global
   public string Phonetic { get; }
   // ReSharper disable once UnusedAutoPropertyAccessor.Global
   public string NodeType { get; }
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
   public override bool IsPastTenseStem => Equals(InflectedForm, InflectionForms.Continuative.TaConnection);
   public override bool IsPastTenseMarker => Equals(InflectionType, InflectionTypes.Special.Ta);

   public override bool IsMasuStem =>
      Equals(InflectedForm, InflectionForms.Continuative.RenyoukeiMasuStem) && IsVerb;

   public override bool IsAdverb =>
      Equals(PartsOfSpeech, JNPOS.Adverb.General) ||
      (JNPOS.Adjective.AllTypes.Contains(PartsOfSpeech) && Surface.EndsWith("く"));

   public override string Surface => _surface;
   public override string BaseForm => _baseForm;

   static readonly HashSet<string> PseudoVerbsForInflectionPurposes = ["ます"];

   public override bool IsInflectableWord =>
      IsVerb || IsAdjective || PseudoVerbsForInflectionPurposes.Contains(BaseForm);

   public override bool IsNonWordCharacter => PartsOfSpeech.IsNonWordCharacter();
   public override bool IsIrrealis => InflectionForms.Irrealis.AllForms.Contains(InflectedForm);
   public override JNToken SourceToken => this;
   // </IAnalysisToken implementation>

   public bool IsVerb =>
      JNPOS.Verb.AllTypes.Contains(PartsOfSpeech) ||
      Equals(InflectionType, InflectionTypes.Special.Masu);

   public bool IsAdjective => JNPOS.Adjective.AllTypes.Contains(PartsOfSpeech);

   public override bool IsIchidanVerb => InflectionType.Base == InflectionTypes.Ichidan.Base;

   static readonly HashSet<string> ActuallySuruVerbsNotGodan = ["する", "為る"];

   public override bool IsGodanVerb =>
      InflectionType.Base == InflectionTypes.Godan.Base &&
      !ActuallySuruVerbsNotGodan.Contains(BaseForm);

   public bool IsKuruVerb => InflectionType.Base == InflectionTypes.Kahen.Base;

   public bool IsSuruVerb =>
      InflectionType.Base == InflectionTypes.Sahen.Base ||
      ActuallySuruVerbsNotGodan.Contains(BaseForm);

   public bool IsDictionaryForm() => Equals(InflectedForm, InflectionForms.Basic.DictionaryForm);

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

         if(Equals(previous.InflectionType, InflectionTypes.Special.Nai))
         {
            return false;
         }

         if(InflectionForms.Continuative.TeConnectionForms.Contains(previous.InflectedForm))
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

   public bool IsTeForm =>
      Equals(PartsOfSpeech, JNPOS.Particle.Conjunctive) && TeForms.Contains(Surface);

   public bool IsProgressiveForm() =>
      Surface == "てる" ||
      (ProgressiveForms.Contains(Surface) && Previous != null &&
       Equals(Previous.InflectedForm, InflectionForms.Continuative.TaConnection)) ||
      (Surface == "いる" && Previous != null && Previous.IsTeForm);

   public override bool IsEndOfStatement =>
      Next == null ||
      Equals(Next.PartsOfSpeech, JNPOS.Particle.SentenceEnding) ||
      AnalysisConstants.SentenceEndCharacters.Contains(Next.Surface) ||
      Next.IsNonWordCharacter;

   static readonly HashSet<string> InvalidIchidanInflectionSurfaces = ["っ"];

   public bool CannotFollowIchidanStem() => InvalidIchidanInflectionSurfaces.Contains(Surface);


   public bool IsMoreLikelyToFollowImperativeThanIchidanStem()
   {
      if(PartsOfSpeech.IsNoun() && !Equals(PartsOfSpeech, JNPOS.Noun.Suffix.General))
      {
         return true;
      }

      return false;
   }

   static readonly HashSet<string> InvalidGodanPotentialFormSurfaces = ["っ", "う"];

   // Initialized via property to defer to after JNPOS is initialized
   static HashSet<JNPartsOfSpeech>? _validPotentialFormInflectionsPos;

   static HashSet<JNPartsOfSpeech> ValidPotentialFormInflectionsPos
   {
      get
      {
         if(_validPotentialFormInflectionsPos == null)
         {
            _validPotentialFormInflectionsPos =
            [
               JNPOS.BoundAuxiliary,
               JNPOS.Noun.Suffix.AuxiliaryVerbStem,
               JNPOS.Verb.Dependent,
               JNPOS.Particle.Conjunctive,
               JNPOS.Particle.CoordinatingConjunction
            ];
         }

         return _validPotentialFormInflectionsPos;
      }
   }

   public bool IsValidGodanPotentialFormInflection()
   {
      if(ValidPotentialFormInflectionsPos.Contains(PartsOfSpeech))
      {
         if(!InvalidGodanPotentialFormSurfaces.Contains(Surface))
         {
            return true;
         }
      }

      return false;
   }
}
