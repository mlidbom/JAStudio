using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.TokenizerDictionaries;
using JAStudio.Core.LanguageServices.TokenizerDictionaries.Comparing;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.TokenizerDictionaries;

/// <summary>Two tokenizations of the same sentence, as two tokenizer dictionaries might cut it, aligned (<see cref="AlignedSpan.Of"/>).</summary>
public class When_aligning_two_tokenizations_of_a_sentence : IAIGeneratedSpec
{
   /// <summary>ひとつだけのに cut as ひとつ|だけ|のに and as ひと|つ|だけの|に: the second cuts ひとつ finer, and cuts across the first's だけ|のに.</summary>
   public class where_the_second_cuts_a_word_finer_and_cuts_across_two_words : When_aligning_two_tokenizations_of_a_sentence
   {
      readonly IReadOnlyList<AlignedSpan> _spans = AlignedSpan.Of(TokensOf("ひとつ", "だけ", "のに"), TokensOf("ひと", "つ", "だけの", "に"));

      [XF] public void the_sentence_is_cut_into_the_smallest_stretches_both_start_and_end_tokens_at() =>
         _spans.Select(it => $"{CutOf(it.First)}/{CutOf(it.Second)}").Must().SequenceEqual(["ひとつ/ひと|つ", "だけ|のに/だけの|に"]);

      [XF] public void the_stretch_the_second_cuts_finer_is_a_difference_where_the_second_cuts_finer() =>
         CuttingDifference.In(_spans[0]).Must().Satisfy(it => it.SecondCutsFiner && !it.SecondCutsCoarser);

      [XF] public void the_stretch_they_cut_across_is_a_difference_cut_neither_finer_nor_coarser() =>
         CuttingDifference.In(_spans[1]).Must().Satisfy(it => !it.SecondCutsFiner && !it.SecondCutsCoarser);

      [XF] public void no_stretch_is_cut_alike() => _spans.Must().Satisfy(all => all.All(it => !it.IsCutAlike));
   }

   /// <summary>ひとつだけ cut as ひとつ|だけ and as ひと|つ|だけ: both cut だけ alike.</summary>
   public class where_both_cut_a_word_alike : When_aligning_two_tokenizations_of_a_sentence
   {
      readonly IReadOnlyList<AlignedSpan> _spans = AlignedSpan.Of(TokensOf("ひとつ", "だけ"), TokensOf("ひと", "つ", "だけ"));

      [XF] public void the_word_is_a_stretch_of_its_own_cut_alike() => _spans[1].Must().Satisfy(it => it.IsCutAlike && it.First[0].Surface == "だけ");
   }

   static IReadOnlyList<MeCabToken> TokensOf(params string[] surfaces)
   {
      var tokens = new List<MeCabToken>();
      foreach(var surface in surfaces)
         tokens.Add(new MeCabToken(tokens.Count == 0 ? 0 : tokens[^1].End, surface, false, "名詞,一般,*,*", MeCabToken.Nothing, MeCabToken.Nothing, surface, null));
      return tokens;
   }

   static string CutOf(IEnumerable<MeCabToken> tokens) => string.Join('|', tokens.Select(it => it.Surface));
}
