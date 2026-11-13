using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.MeCab;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.MeCab;

/// <summary>
/// The bundled IPADIC tokenizing a text with spaces before words, an emoji, which no dictionary has, and a full-width space<br/>
/// (<see cref="TokenizerDictionary.Tokenize"/>): where each token starts is counted, since MeCab.DotNet doesn't give it.
/// </summary>
public class When_MeCab_cuts_a_text_into_tokens : IAIGeneratedSpec
{
   const string Text = "  ボクは 😀ひとつ\u3000だけ、知らない";

   readonly IReadOnlyList<RawMeCabToken> _tokens = TokenizerDictionary.TheBundledIpadic.Tokenize(Text);

   [XF] public void the_tokens_are_the_texts_words_without_the_marks_of_where_it_begins_and_ends() =>
      _tokens.Select(it => it.Surface).Must().SequenceEqual(["ボク", "は", "😀", "ひとつ", "\u3000", "だけ", "、", "知ら", "ない"]);

   [XF] public void each_tokens_surface_is_the_text_where_it_starts() => _tokens.Must().Satisfy(all => all.All(it => Text.Substring(it.Start, it.Surface.Length) == it.Surface));

   [XF] public void a_word_the_dictionary_lacks_is_unknown_to_it() => _tokens.Single(it => it.Surface == "😀").Must().Satisfy(it => it.IsUnknownToTheDictionary);

   [XF] public void a_word_the_dictionary_has_is_known_to_it() => _tokens.Single(it => it.Surface == "ボク").Must().Satisfy(it => !it.IsUnknownToTheDictionary);

   [XF] public void an_inflected_tokens_base_form_is_its_dictionary_form() => _tokens.Single(it => it.Surface == "知ら").BaseForm.Must().Be("知る");

   [XF] public void the_part_of_speech_has_its_four_levels() => _tokens.Single(it => it.Surface == "ボク").PartOfSpeech.Must().Be("名詞,代名詞,一般,*");

   [XF] public void IPADIC_gives_no_lemma_so_the_word_is_the_base_form() =>
      _tokens.Single(it => it.Surface == "知ら").Must().Satisfy(it => it.Lemma == null && it.Word == "知る");

   [XF] public void the_bundled_IPADIC_is_read_in_IPADICs_format() => TokenizerDictionary.TheBundledIpadic.Format.Must().Be(TokenizerDictionaryFormat.Ipadic);
}
