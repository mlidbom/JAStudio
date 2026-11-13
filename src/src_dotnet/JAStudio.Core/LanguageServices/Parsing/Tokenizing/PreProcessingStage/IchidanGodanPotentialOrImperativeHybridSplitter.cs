using Compze.Internals.SystemCE;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing.PreProcessingStage;

static class IchidanGodanPotentialOrImperativeHybridSplitter
{
   static readonly HashSet<string> PotentialOrImperativeGodanLastCompoundParts = ["える", "え"];

   public static List<AnalysisToken>? TrySplit(MeCabToken token, VocabCollection vocabs, DictLookup dictLookup)
   {
      if(IsAnIchidanImperativeInRo(token))
      {
         return null;
      }

      var hiddenGodan = TheGodanHiddenIn(token.BaseForm, token.IsIchidanVerb, vocabs, dictLookup);

      if(hiddenGodan != null)
      {
         if(IsPotentialGodan(vocabs, dictLookup, token.BaseForm, EndsInRuOrIsFollowedByAPotentialFormsInflection(token), token.Previous, hiddenGodan))
         {
            return SplitGodanPotential(token, hiddenGodan);
         }

         if(IsImperativeGodan(vocabs, dictLookup, token, hiddenGodan))
         {
            return SplitGodanImperative(token, hiddenGodan);
         }
      }

      return null;
   }

   /// <summary>An imperative in ろ, such as 食べろ, which no godan verb's imperative or potential form looks like.</summary>
   static bool IsAnIchidanImperativeInRo(MeCabToken token) => Equals(token.InflectedForm, UniDicInflectionForms.Imperative) && token.Surface.EndsWithOrdinal("ろ");

   static List<AnalysisToken> SplitGodanImperative(MeCabToken token, string godanBase)
   {
      var godanSurface = token.Surface[..^1];
      var imperativePart = token.Surface[^1..];
      var imperativeBase = imperativePart == "い" ? "い" : "え";
      return
      [
         new SplitToken(token, godanSurface, godanBase, isInflectableWord: true, isGodanImperativeStem: true),
         new SplitToken(token, imperativePart, imperativeBase, isInflectableWord: true, isGodanImperativeInflection: true)
      ];
   }

   static List<AnalysisToken> SplitGodanPotential(MeCabToken token, string godanBase)
   {
      var isDictionaryForm = token.Surface.EndsWithOrdinal("る");

      var basePart = token.Surface.TrimEnd('る')[..^1];
      var potentialSurface = token.Surface.Substring(basePart.Length);
      var potentialBase = isDictionaryForm ? potentialSurface : potentialSurface + "る";

      if(isDictionaryForm)
      {
         potentialSurface = potentialSurface[..^1];
         return
         [
            new GodanPotentialDictionaryFormStem(token, basePart, godanBase),
            new GodanPotentialInflectionDictionaryFormStem(token, potentialSurface, potentialBase),
            new GodanPotentialInflectionDictionaryFormInflection(token)
         ];
      }

      return
      [
         new SplitToken(token, basePart, godanBase, isInflectableWord: true, isGodanPotentialStem: true),
         new SplitToken(token, potentialSurface, potentialBase, isInflectableWord: true, isGodanPotentialInflection: true)
      ];
   }

   /// <summary>
   /// The godan verb whose potential form the ichidan verb <paramref name="ichidanBaseForm"/> is, as <see cref="TrySplit"/> decides it for a token: 磨ぐ<br/>
   /// for 磨げる. Null when it's no godan verb's potential. For an ichidan verb the tokenizer cut in two (<see cref="IchidanVerbUnknownToTheTokenizer"/>).
   /// </summary>
   /// <param name="ichidanBaseForm">The ichidan verb's base form, such as 磨げる.</param>
   /// <param name="endsInRuOrIsFollowedByAPotentialFormsInflection">Whether the verb ends in its dictionary form's る, or is followed by an ending a potential form takes.</param>
   /// <param name="preceding">The token before the verb, which tells an intransitive godan verb's potential from the ichidan verb after を.</param>
   /// <param name="vocabs">The vocab, which can name a verb the dictionary doesn't, as JAStudio's word lookups (<see cref="WordInfo"/>) take them.</param>
   /// <param name="dictLookup">The dictionary JAStudio looks words up in.</param>
   internal static string? TheGodanVerbWhosePotentialFormIs(string ichidanBaseForm, bool endsInRuOrIsFollowedByAPotentialFormsInflection, MeCabToken? preceding, VocabCollection vocabs, DictLookup dictLookup) =>
      TheGodanHiddenIn(ichidanBaseForm, isIchidanVerb: true, vocabs, dictLookup) is {} godan &&
      IsPotentialGodan(vocabs, dictLookup, ichidanBaseForm, endsInRuOrIsFollowedByAPotentialFormsInflection, preceding, godan)
         ? godan
         : null;

   /// <summary>The godan verb an ichidan verb, or a vocab compound, named <paramref name="baseForm"/> may be the potential or imperative of, or null.</summary>
   static string? TheGodanHiddenIn(string baseForm, bool isIchidanVerb, VocabCollection vocabs, DictLookup dictLookup) =>
      TryFindGodanHiddenInIchidanUsingDictionary(vocabs, dictLookup, baseForm, isIchidanVerb) ??
      TryFindVocabBasedPotentialOrImperativeGodanCompound(baseForm, vocabs, dictLookup);

   static bool EndsInRuOrIsFollowedByAPotentialFormsInflection(MeCabToken token) =>
      token.Surface.EndsWithOrdinal("る") || (token.Next != null && token.Next.IsValidGodanPotentialFormInflection());

   static string? TryFindVocabBasedPotentialOrImperativeGodanCompound(string baseForm, VocabCollection vocabs, DictLookup dictLookup)
   {
      foreach(var vocab in vocabs.WithQuestion(baseForm))
      {
         var compoundParts = vocab.CompoundParts.Primary();
         //review: A last part of え:imperative, as in 引け (引く + え:imperative) and 11 other vocab, doesn't count here, since only the plain える and え
         //   do. That's how it was when compound parts were strings, so typing them (slice 8) kept it, but え:imperative is the imperative this looks for.
         //   Should it count? It would change how those 12 vocab's sentences parse.
         if(compoundParts.Count == 2 && compoundParts[1].IsAPlainWordAmong(PotentialOrImperativeGodanLastCompoundParts))
         {
            if(WordInfo.IsGodan(vocabs, dictLookup, compoundParts[0].Text))
            {
               return compoundParts[0].Text;
            }
         }
      }

      return null;
   }

   static bool IsPotentialGodan(VocabCollection vocabs, DictLookup dictLookup, string ichidanBaseForm, bool endsInRuOrIsFollowedByAPotentialFormsInflection, MeCabToken? preceding, string godanBase)
   {
      if(endsInRuOrIsFollowedByAPotentialFormsInflection)
      {
         var godanDictEntry = WordInfo.LookupGodan(vocabs, dictLookup, godanBase);
         if(godanDictEntry == null)
         {
            return false;
         }

         // Intransitive verbs don't take を so this is most likely actually the ichidan verb
         if(godanDictEntry.IsIntransitive && preceding != null && preceding.Surface == "を" &&
            WordInfo.IsIchidan(vocabs, dictLookup, ichidanBaseForm))
         {
            return false;
         }

         return true;
      }

      return false;
   }

   static bool IsImperativeGodan(VocabCollection vocabs, DictLookup dictLookup, MeCabToken token, string godanBase)
   {
      if(!Conjugator.GodanImperativeVerbEndings.Contains(token.Surface[^1..]))
      {
         return false;
      }

      var godanWordInfo = WordInfo.Lookup(vocabs, dictLookup, godanBase);
      if(WordInfo.Lookup(vocabs, dictLookup, token.BaseForm) == null)
      {
         // We check for potential godan before this, so if there is no ichidan verb in the dictionary,
         // the only thing left is an imperative godan
         return true;
      } else if(godanWordInfo != null && godanWordInfo.IsIntransitive && token.Previous != null && token.Previous.Surface == "を")
      {
         return false; // Intransitive verbs don't take を so this is most likely actually the ichidan verb
      } else if(token.Next != null && token.Next.CannotFollowIchidanStem())
      {
         return true;
      } else if(token.Next == null || token.IsEndOfStatement)
      {
         return true;
      } else if(token.Next != null && token.Next.IsMoreLikelyToFollowImperativeThanIchidanStem())
      {
         return true;
      }

      return false;
   }

   public static bool BaseFormHasGodanPotentialEnding(string baseForm)
   {
      if(baseForm.Length < 2)
      {
         return false;
      }

      var ending = baseForm[^2..];
      return Conjugator.GodanPotentialVerbEndingToDictionaryFormEndings.ContainsKey(ending);
   }

   static string? TryFindGodanHiddenInIchidanUsingDictionary(VocabCollection vocabs, DictLookup dictLookup, string baseForm, bool isIchidanVerb)
   {
      if(baseForm.Length >= 2 &&
         BaseFormHasGodanPotentialEnding(baseForm) &&
         isIchidanVerb)
      {
         var possibleGodanForm = Conjugator.ConstructRootVerbForPossiblyPotentialGodanVerbDictionaryForm(baseForm);
         if(WordInfo.IsGodan(vocabs, dictLookup, possibleGodanForm))
         {
            return possibleGodanForm;
         }
      }

      return null;
   }

   public static bool IsIchidanHidingGodan(VocabNote vocab) => TryGetGodanHiddenByIchidan(vocab) != null;

   public static WordInfoEntry? TryGetGodanHiddenByIchidan(VocabNote vocab)
   {
      var services = vocab.Services;
      var question = vocab.GetQuestion();
      if(BaseFormHasGodanPotentialEnding(question))
      {
         var possibleGodanForm = Conjugator.ConstructRootVerbForPossiblyPotentialGodanVerbDictionaryForm(question);
         var godanDictEntry = WordInfo.LookupGodan(services.Collection.Vocab, services.DictLookup, possibleGodanForm);
         if(godanDictEntry != null && godanDictEntry.IsGodan)
         {
            return godanDictEntry;
         }
      }

      return null;
   }
}
