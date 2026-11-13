namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

// The interface that is used in the text analysis pipeline
// Defaults to returning False for all bool properties, and inheritors override whatever properties they can
public abstract class IAnalysisToken
{
   public abstract string Surface { get; }
   public abstract string BaseForm { get; }

   public virtual bool IsIchidanVerb => SourceToken.IsIchidanVerb;
   public virtual bool IsGodanVerb => SourceToken.IsGodanVerb;
   public virtual bool IsPastTenseStem => SourceToken.IsPastTenseStem;
   public virtual bool IsPastTenseMarker => SourceToken.IsPastTenseMarker;
   public virtual bool IsMasuStem => SourceToken.IsMasuStem;
   public virtual bool IsAdverb => SourceToken.IsAdverb;
   public virtual bool IsIrrealis => SourceToken.IsIrrealis;
   public virtual bool IsEndOfStatement => SourceToken.IsEndOfStatement;
   public virtual bool HasTeFormStem => false;

   public virtual bool IsInflectableWord => false;
   public virtual bool IsNonWordCharacter => false;
   public virtual bool IsDictionaryVerbFormStem => false;
   public virtual bool IsDictionaryVerbInflection => false;

   // <Only true for split tokens>
   public virtual bool IsGodanPotentialStem => false;
   public virtual bool IsGodanImperativeStem => false;
   public virtual bool IsIchidanImperativeStem => false;
   public virtual bool IsGodanPotentialInflection => false;
   public virtual bool IsGodanImperativeInflection => false;
   public virtual bool IsIchidanImperativeInflection => false;
   // </Only true for split tokens>

   public abstract JNToken SourceToken { get; }

   /// <summary>
   /// Whether the token is one the tokenizer gave, as it gave it, rather than one of JAStudio's own, a piece the pre-processing stage split off a<br/>
   /// token (<see cref="SplitTokenBase"/>).
   /// </summary>
   public virtual bool IsTheTokenizersOwn => false;
}
