using Compze.Internals.SystemCE;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing.PreProcessingStage;

static class IchidanGodanPotentialOrImperativeHybridSplitter
{
   static readonly HashSet<string> PotentialOrImperativeGodanLastCompoundParts = ["える", "え"];

   public static List<IAnalysisToken>? TrySplit(JNToken token, VocabCollection vocabs, DictLookup dictLookup)
   {
      if(IsAnIchidanImperativeInRo(token))
      {
         return null;
      }

      var hiddenGodan = TryFindGodanHiddenInIchidanUsingDictionary(vocabs, dictLookup, token) ??
                        TryFindVocabBasedPotentialOrImperativeGodanCompound(token, vocabs, dictLookup);

      if(hiddenGodan != null)
      {
         if(IsPotentialGodan(vocabs, dictLookup, token, hiddenGodan))
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
   static bool IsAnIchidanImperativeInRo(JNToken token) => Equals(token.InflectedForm, InflectionForms.Imperative) && token.Surface.EndsWithOrdinal("ろ");

   static List<IAnalysisToken> SplitGodanImperative(JNToken token, string godanBase)
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

   static List<IAnalysisToken> SplitGodanPotential(JNToken token, string godanBase)
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

   static string? TryFindVocabBasedPotentialOrImperativeGodanCompound(JNToken token, VocabCollection vocabs, DictLookup dictLookup)
   {
      foreach(var vocab in vocabs.WithQuestion(token.BaseForm))
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

   static bool IsPotentialGodan(VocabCollection vocabs, DictLookup dictLookup, JNToken token, string godanBase)
   {
      if(token.Surface.EndsWithOrdinal("る") || (token.Next != null && token.Next.IsValidGodanPotentialFormInflection()))
      {
         var godanDictEntry = WordInfo.LookupGodan(vocabs, dictLookup, godanBase);
         if(godanDictEntry == null)
         {
            return false;
         }

         // Intransitive verbs don't take を so this is most likely actually the ichidan verb
         if(godanDictEntry.IsIntransitive && token.Previous != null && token.Previous.Surface == "を" &&
            WordInfo.IsIchidan(vocabs, dictLookup, token.BaseForm))
         {
            return false;
         }

         return true;
      }

      return false;
   }

   static bool IsImperativeGodan(VocabCollection vocabs, DictLookup dictLookup, JNToken token, string godanBase)
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

   static string? TryFindGodanHiddenInIchidanUsingDictionary(VocabCollection vocabs, DictLookup dictLookup, JNToken token)
   {
      if(token.BaseForm.Length >= 2 &&
         BaseFormHasGodanPotentialEnding(token.BaseForm) &&
         token.IsIchidanVerb)
      {
         var possibleGodanForm = Conjugator.ConstructRootVerbForPossiblyPotentialGodanVerbDictionaryForm(token.BaseForm);
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
