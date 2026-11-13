using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.MeCab;
using JAStudio.Core.LanguageServices.MeCab.Comparing;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.MeCab;

/// <summary>The bundled IPADIC compared with itself over a few sentences (<see cref="TokenizerDictionaryComparison"/>): nothing differs.</summary>
public class When_comparing_a_tokenizer_dictionary_with_itself : IAIGeneratedSpec
{
   readonly TokenizerDictionaryComparison _comparison =
      TokenizerDictionaryComparison.Of(TokenizerDictionary.TheBundledIpadic, TokenizerDictionary.TheBundledIpadic, ["ボクはひとつだけ知らない。", "今日はいい天気だね", "😀"]);

   [XF] public void every_sentence_is_counted() => _comparison.Sentences.Must().Be(3L);

   [XF] public void every_sentence_is_cut_alike() => _comparison.SentencesCutAlike.Must().Be(3L);

   [XF] public void every_token_is_cut_alike() => _comparison.TokensCutAlike.Must().Be(_comparison.FirstsTokens);

   [XF] public void no_cutting_difference_is_found() => _comparison.CuttingDifferences.Total.Must().Be(0L);

   [XF] public void each_part_of_speech_corresponds_to_itself() => _comparison.PartsOfSpeech.MostFoundFirst().Must().Satisfy(all => all.All(it => it.Thing.First == it.Thing.Second));

   [XF] public void no_base_form_or_word_differs() => (_comparison.BaseForms.Total + _comparison.Words.Total).Must().Be(0L);

   [XF] public void the_emoji_is_unknown_to_both() =>
      (_comparison.FirstsUnknownTokens, _comparison.UnknownToTheFirstOnly.Total, _comparison.UnknownToTheSecondOnly.Total).Must().Be((1L, 0L, 0L));

   [XF] public void each_correspondence_has_an_example_token() => _comparison.PartsOfSpeech.MostFoundFirst().Must().Satisfy(all => all.All(it => it.Examples.Any()));
}
