using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// JAStudio's reading of an ichidan verb missing from the tokenizer's dictionary, which the tokenizer therefore cuts into a godan verb's e-stem<br/>
/// followed by an ending that can only follow an ichidan verb's stem: 磨げる ("can polish") becomes 磨げ, read as the irrealis of 磨ぐ, followed by<br/>
/// an uninflecting auxiliary る. The tokenizer's tokens stay as it gave them; the facts JAStudio reads from them are the verb's<br/>
/// (<see cref="JNToken.IsIchidanVerb"/> and the rest), and the pre-processing stage splits them as it splits the verbs the dictionary knows<br/>
/// (<see cref="IchidanVerbUnknownToTheTokenizerSplitter"/>).
/// </summary>
/// <remarks>
/// Nearly all such verbs are the potential forms of godan verbs. The tokenizer's dictionary, UniDic, has most potential forms as ichidan verbs of<br/>
/// their own (泳げる, 嗅げる, もらえる), but not all of them (磨げる). Read as the tokenizer cuts them, the analysis shows the auxiliary る where the<br/>
/// dictionary form's る is, and loses the potential altogether before ない, た, ます and て.
/// </remarks>
/// <remarks>
/// Only endings that cannot follow a godan e-stem in modern Japanese make one. Left alone are the conditional ば (磨げば), the classical concessive<br/>
/// ど and ども (待てど), the classical り in its own form (極まれり), and whatever follows an imperative (飲めとか). Comparing the tokenizer's tokens<br/>
/// for 嗅げる, which its dictionary knows, and 磨げる across their forms shows that the endings themselves come out alike, with two exceptions:<br/>
/// the dictionary form's る and the hypothetical form's れ are tokens of their own. Those two are the rest of the verb itself, part of it as the<br/>
/// e-stem is; any other ending follows the verb. The tokenizer gives the e-stem a form it guesses, 未然形, so the e-stem is told by its kana and<br/>
/// the verb's form by its ending.
/// </remarks>
sealed class IchidanVerbUnknownToTheTokenizer
{
   const string IchidanDictionaryFormEnding = "る";
   const string HypotheticalFormEnding = "れ";

   /// <summary>The base forms of the endings that follow an ichidan verb's irrealis stem, as in 泳げない and 泳げず.</summary>
   static readonly HashSet<string> BaseFormsOfEndingsAfterTheIrrealisStem = ["ない", "ぬ", "ず"];

   /// <summary>
   /// The base forms of the endings that follow an ichidan verb's continuative stem, as in 泳げた, 泳げます, 泳げて, 泳げてる, 泳げそう and 泳げまい:<br/>
   /// the tokenizer's dictionary, UniDic, has まい after an ichidan verb's continuative.
   /// </summary>
   static readonly HashSet<string> BaseFormsOfEndingsAfterTheContinuativeStem = ["た", "ます", "て", "てる", "そう", "まい"];

   IchidanVerbUnknownToTheTokenizer(JNToken eStem, JNToken ending, InflectionForm form, bool theEndingIsPartOfIt)
   {
      EStem = eStem;
      Ending = ending;
      Form = form;
      TheEndingIsPartOfIt = theEndingIsPartOfIt;
   }

   /// <summary>The tokenizer's token for the verb's stem, a godan verb's e-stem as the tokenizer reads it: the 磨げ of 磨げる and 磨げない.</summary>
   public JNToken EStem { get; }

   /// <summary>The tokenizer's token after <see cref="EStem"/>: the verb's own る or れ, or an ending that follows the verb, such as ない.</summary>
   public JNToken Ending { get; }

   /// <summary>The form the verb is in: the dictionary form or the hypothetical, with <see cref="Ending"/> part of it, or the irrealis or continuative before it.</summary>
   public InflectionForm Form { get; }

   /// <summary>Whether <see cref="Ending"/> is the rest of the verb itself, its dictionary form's る or its hypothetical form's れ, rather than an ending that follows it.</summary>
   public bool TheEndingIsPartOfIt { get; }

   /// <summary>The verb in its dictionary form, its stem and る: 磨げる.</summary>
   public string BaseForm => EStem.Surface + IchidanDictionaryFormEnding;

   public bool IsInTheDictionaryForm => TheEndingIsPartOfIt && Ending.Surface == IchidanDictionaryFormEnding;

   public bool IsInTheHypotheticalForm => TheEndingIsPartOfIt && Ending.Surface == HypotheticalFormEnding;

   // <The facts the analysis reads from the tokenizer's tokens that are part of the verb>
   public bool IsIrrealis => InflectionForms.IsIrrealis(Form);
   public bool IsMasuStem => Equals(Form, InflectionForms.Continuative);
   public bool IsPastTenseStem => InflectionForms.IsAPastTenseStem(Form);

   /// <summary>Whether the verb ends a statement: never before an ending that follows it; with its own ending, as that ending does.</summary>
   public bool EndsAStatement => TheEndingIsPartOfIt && Ending.EndsAStatementBeforeTheTokenAfterIt;
   // </The facts the analysis reads from the tokenizer's tokens that are part of the verb>

   /// <summary>
   /// The godan verb whose potential form the verb is, as the <see cref="IchidanGodanPotentialOrImperativeHybridSplitter"/> decides for the potential<br/>
   /// forms the dictionary knows: 磨ぐ for 磨げる. Null for a genuine ichidan verb.
   /// </summary>
   public string? TheGodanVerbWhosePotentialFormItIs(VocabCollection vocabs, DictLookup dictLookup) =>
      IchidanGodanPotentialOrImperativeHybridSplitter.TheGodanVerbWhosePotentialFormIs(BaseForm, IsFollowedByAPotentialFormsInflection, EStem.Previous, vocabs, dictLookup);

   /// <summary>
   /// Whether the verb ends in its dictionary form's る or is followed by an ending a potential form takes, as the potential forms the dictionary<br/>
   /// knows must be to be read as potentials: an ending that follows the verb always is one, or it couldn't have made one.
   /// </summary>
   bool IsFollowedByAPotentialFormsInflection =>
      IsInTheDictionaryForm || !TheEndingIsPartOfIt || (Ending.Next is {} following && following.IsValidGodanPotentialFormInflection());

   /// <summary>The verb <paramref name="token"/> is part of, its e-stem or its own ending, or null when it's part of no ichidan verb unknown to the tokenizer.</summary>
   public static IchidanVerbUnknownToTheTokenizer? ThatHasAsAPart(JNToken token) =>
      StartingAt(token) ?? (token.Previous is {} previous && StartingAt(previous) is { TheEndingIsPartOfIt: true } verbEndingHere ? verbEndingHere : null);

   /// <summary>The verb whose e-stem is <paramref name="eStem"/>, or null when it's no e-stem of an ichidan verb unknown to the tokenizer.</summary>
   static IchidanVerbUnknownToTheTokenizer? StartingAt(JNToken eStem)
   {
      if(!IsGodanEStem(eStem) || eStem.Next is not {} ending) return null;

      if(IsTheDictionaryFormEnding(ending)) return new IchidanVerbUnknownToTheTokenizer(eStem, ending, TheDictionaryFormBefore(ending.Next), theEndingIsPartOfIt: true);
      if(IsTheHypotheticalFormEnding(ending)) return new IchidanVerbUnknownToTheTokenizer(eStem, ending, InflectionForms.Hypothetical, theEndingIsPartOfIt: true);
      return IchidanStemFormBeforeOrNull(ending) is {} stemForm ? new IchidanVerbUnknownToTheTokenizer(eStem, ending, stemForm, theEndingIsPartOfIt: false) : null;
   }

   /// <summary>
   /// A godan verb's stem ending in the e-row, such as 磨げ, whatever form the tokenizer guessed it to be in. Read from the tokenizer's tags, not from<br/>
   /// the facts, which are the verb's once it is part of one.
   /// </summary>
   static bool IsGodanEStem(JNToken token) => token.IsTaggedAsAGodanVerb && Conjugator.GodanImperativeVerbEndings.Contains(token.Surface[^1..]);

   /// <summary>The dictionary form's る, which the spoken-language UniDic reads as an uninflecting auxiliary, and the written-language one as the classical り.</summary>
   static bool IsTheDictionaryFormEnding(JNToken ending) =>
      ending.Surface == IchidanDictionaryFormEnding &&
      (Equals(ending.InflectionType, InflectionTypes.Classical.Ri) || (Equals(ending.PartsOfSpeech, JNPOS.BoundAuxiliary) && Equals(ending.InflectionType, InflectionTypes.Uninflecting)));

   /// <summary>The hypothetical form's れ, which the tokenizer reads as the passive auxiliary れる, which can't follow an e-stem: the passive follows the a-stem, as in 書かれる.</summary>
   static bool IsTheHypotheticalFormEnding(JNToken ending) =>
      ending.Surface == HypotheticalFormEnding && Equals(ending.InflectionType, InflectionTypes.Auxiliary.Reru);

   static InflectionForm? IchidanStemFormBeforeOrNull(JNToken ending)
   {
      if(IsAnEndingWithABaseFormIn(BaseFormsOfEndingsAfterTheIrrealisStem)) return InflectionForms.Irrealis;
      if(IsAnEndingWithABaseFormIn(BaseFormsOfEndingsAfterTheContinuativeStem)) return InflectionForms.Continuative;
      return null;

      bool IsAnEndingWithABaseFormIn(HashSet<string> baseForms) => ending.IsValidGodanPotentialFormInflection() && baseForms.Contains(ending.BaseForm);
   }

   /// <summary>
   /// The dictionary form the tokenizer gives a verb before <paramref name="followingToken"/>: the attributive (連体形-一般) before a noun, or<br/>
   /// before の and ん, which make what precedes them a noun, as in 泳げる人 and 泳げるの; otherwise the conclusive (終止形-一般), as in 泳げる and<br/>
   /// 泳げるか.
   /// </summary>
   static InflectionForm TheDictionaryFormBefore(JNToken? followingToken) =>
      InflectionForms.GetByName(followingToken is {} following && (following.PartsOfSpeech.IsNoun() || Equals(following.PartsOfSpeech, JNPOS.Particle.Nominalizing))
                                   ? "連体形-一般"
                                   : "終止形-一般");
}
