using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.Tokenizing;

namespace JAStudio.Core.UI.Web.Sentence;

/// <summary>
/// One token of a sentence, as a row of the sentence breakdown's token tables shows it in edit mode: its surface and base form, what JAStudio's<br/>
/// analysis says it is, its class, and the tokenizer's parts of speech and inflection, so that why the sentence was split as it was can be seen.
/// </summary>
public sealed class TokenViewModel
{
   static readonly List<(Func<AnalysisToken, bool> IsTrueOf, string Name)> Flags =
   [
      (it => it.IsPastTenseStem, "past_tense_stem"),
      (it => it.IsPastTenseMarker, "past_tense_marker"),
      (it => it.IsMasuStem, "masu_stem"),
      (it => it.IsAdverb, "adverb"),
      (it => it.IsIrrealis, "irrealis"),
      (it => it.IsEndOfStatement, "end_of_statement"),
      (it => it.HasTeFormStem, "has_te_form_stem"),
      (it => it.IsNonWordCharacter, "non_word_character"),
      (it => it.IsDictionaryVerbFormStem, "dictionary_verb_form_stem"),
      (it => it.IsDictionaryVerbInflection, "dictionary_verb_inflection"),
      (it => it.IsGodanPotentialStem, "godan_potential_stem"),
      (it => it.IsGodanImperativeStem, "godan_imperative_stem"),
      (it => it.IsIchidanImperativeStem, "ichidan_imperative_stem"),
      (it => it.IsGodanPotentialInflection, "godan_potential_inflection"),
      (it => it.IsGodanImperativeInflection, "godan_imperative_inflection"),
      (it => it.IsIchidanImperativeInflection, "ichidan_imperative_inflection"),
      (it => it.IsInflectableWord, "inflectable_word"),
      (it => it.IsIchidanVerb, "ichidan_verb"),
      (it => it.IsGodanVerb, "godan_verb")
   ];

   readonly AnalysisToken _token;

   public TokenViewModel(AnalysisToken token) => _token = token;

   public string Surface => _token.Surface;

   public string BaseForm => _token.BaseForm;

   /// <summary>What JAStudio's analysis says the token is, such as <c>masu_stem</c> or <c>godan_verb</c>, each that's true of it.</summary>
   public List<string> TrueFlags => [.. Flags.Where(it => it.IsTrueOf(_token)).Select(it => it.Name)];

   /// <summary>The <see cref="TrueFlags"/>, separated by commas.</summary>
   public string TrueFlagsListed => string.Join(", ", TrueFlags);

   /// <summary>The token's class, such as <c>MeCabToken</c>, the tokenizer's own, or one of JAStudio's, made by splitting the tokenizer's.</summary>
   public string TokenClass => _token.GetType().Name;

   /// <summary>Whether the token is one of JAStudio's own, made from the tokenizer's, rather than one the tokenizer gave.</summary>
   public bool IsJAStudiosOwn => !_token.IsTheTokenizersOwn;

   /// <summary>The tokenizer's four levels of parts of speech, from the broadest, each in Japanese and in English.</summary>
   public List<TokenPartOfSpeech> PartsOfSpeech
   {
      get
      {
         var partsOfSpeech = _token.SourceToken.PartsOfSpeech;
         return [.. new[] { partsOfSpeech.Level1, partsOfSpeech.Level2, partsOfSpeech.Level3, partsOfSpeech.Level4 }.Select(it => new TokenPartOfSpeech(it.Japanese, it.English))];
      }
   }

   public string InflectedForm => _token.SourceToken.InflectedForm.ToString();

   public string InflectionType => _token.SourceToken.InflectionType.ToString();
}
