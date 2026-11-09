using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.JamdictEx;

/// <summary>
/// The dictionary's (JMdict's) parts of speech, as it describes them, such as "godan verb with 'ku' ending", and the <see cref="PartOfSpeech"/>s<br/>
/// each one is to JAStudio. Every part of speech the dictionary has is mapped, so a vocab filled from the dictionary only gets parts of speech JAStudio knows.
/// </summary>
/// <remarks>
/// The dictionary's archaic verb and adjective classes map to their kind alone: every nidan verb subtype to <see cref="PartOfSpeech.NidanVerb"/>,<br/>
/// every yodan one to <see cref="PartOfSpeech.YodanVerb"/>. "unclassified" maps to nothing.
/// </remarks>
static class DictionaryPartsOfSpeech
{
   static readonly FrozenDictionary<string, PartOfSpeech[]> PartsOfSpeechByDictionaryPartOfSpeech = new Dictionary<string, PartOfSpeech[]>(StringComparer.OrdinalIgnoreCase)
   {
      ["noun (common) (futsuumeishi)"] = [PartOfSpeech.Noun],
      ["noun, used as a suffix"] = [PartOfSpeech.Noun, PartOfSpeech.Suffix],
      ["noun, used as a prefix"] = [PartOfSpeech.Prefix, PartOfSpeech.Noun],
      ["noun or participle which takes the aux. verb suru"] = [PartOfSpeech.SuruVerb],
      ["nouns which may take the genitive case particle 'no'"] = [PartOfSpeech.Noun, PartOfSpeech.NoAdjective],
      ["noun or verb acting prenominally"] = [PartOfSpeech.Prenominal],
      ["pronoun"] = [PartOfSpeech.Pronoun],

      ["transitive verb"] = [PartOfSpeech.Transitive],
      ["intransitive verb"] = [PartOfSpeech.Intransitive],
      ["verb unspecified"] = [PartOfSpeech.Verb],

      ["godan verb with 'u' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'ru' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'mu' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'nu' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'gu' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'ku' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'su' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'bu' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'tsu' ending"] = [PartOfSpeech.GodanVerb],
      ["godan verb with 'u' ending (special class)"] = [PartOfSpeech.GodanVerb, PartOfSpeech.SpecialClass],
      ["godan verb - -aru special class"] = [PartOfSpeech.GodanVerb, PartOfSpeech.SpecialClassAru],
      ["godan verb - iku/yuku special class"] = [PartOfSpeech.GodanVerb, PartOfSpeech.SpecialClass],
      ["godan verb with 'ru' ending (irregular verb)"] = [PartOfSpeech.GodanVerb, PartOfSpeech.Irregular],
      ["irregular nu verb"] = [PartOfSpeech.NuVerb],

      ["ichidan verb"] = [PartOfSpeech.IchidanVerb],
      ["ichidan verb - zuru verb (alternative form of -jiru verbs)"] = [PartOfSpeech.IchidanVerb, PartOfSpeech.ZuruVerb],
      ["ichidan verb - kureru special class"] = [PartOfSpeech.IchidanVerb, PartOfSpeech.SpecialClass],

      ["suru verb - included"] = [PartOfSpeech.SuruVerb],
      ["suru verb - special class"] = [PartOfSpeech.SuruVerb, PartOfSpeech.SpecialClass],
      ["su verb - precursor to the modern suru"] = [PartOfSpeech.SuVerb],
      ["kuru verb - special class"] = [PartOfSpeech.KuruVerb, PartOfSpeech.SpecialClass],
      ["irregular ru verb, plain form ends with -ri"] = [PartOfSpeech.RiVerb],

      ["yodan verb with 'ru' ending (archaic)"] = [PartOfSpeech.YodanVerb],
      ["yodan verb with 'ku' ending (archaic)"] = [PartOfSpeech.YodanVerb],
      ["yodan verb with 'hu/fu' ending (archaic)"] = [PartOfSpeech.YodanVerb],
      ["yodan verb with 'su' ending (archaic)"] = [PartOfSpeech.YodanVerb],
      ["yodan verb with 'tsu' ending (archaic)"] = [PartOfSpeech.YodanVerb],
      ["yodan verb with 'mu' ending (archaic)"] = [PartOfSpeech.YodanVerb],
      ["yodan verb with 'bu' ending (archaic)"] = [PartOfSpeech.YodanVerb],
      ["yodan verb with 'gu' ending (archaic)"] = [PartOfSpeech.YodanVerb],

      ["nidan verb with 'u' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'ru' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'mu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'dzu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'hu/fu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'tsu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'gu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'yu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'ku' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'su' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'nu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'zu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (lower class) with 'u' ending and 'we' conjugation (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (upper class) with 'ru' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (upper class) with 'tsu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (upper class) with 'yu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (upper class) with 'gu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (upper class) with 'bu' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (upper class) with 'ku' ending (archaic)"] = [PartOfSpeech.NidanVerb],
      ["nidan verb (upper class) with 'hu/fu' ending (archaic)"] = [PartOfSpeech.NidanVerb],

      ["adjective (keiyoushi)"] = [PartOfSpeech.IAdjective],
      ["adjective (keiyoushi) - yoi/ii class"] = [PartOfSpeech.IAdjective],
      ["auxiliary adjective"] = [PartOfSpeech.IAdjective, PartOfSpeech.Auxiliary],
      ["adjectival nouns or quasi-adjectives (keiyodoshi)"] = [PartOfSpeech.NaAdjective],
      ["archaic/formal form of na-adjective"] = [PartOfSpeech.NariAdjective],
      ["'taru' adjective"] = [PartOfSpeech.TaruAdjective],
      ["'ku' adjective (archaic)"] = [PartOfSpeech.KuAdjective],
      ["'shiku' adjective (archaic)"] = [PartOfSpeech.ShikuAdjective],
      ["pre-noun adjectival (rentaishi)"] = [PartOfSpeech.PreNounAdjectival],

      ["adverb (fukushi)"] = [PartOfSpeech.Adverb],
      ["adverb taking the 'to' particle"] = [PartOfSpeech.ToAdverb],

      ["expressions (phrases, clauses, etc.)"] = [PartOfSpeech.Expression],
      ["auxiliary"] = [PartOfSpeech.Auxiliary],
      ["auxiliary verb"] = [PartOfSpeech.Auxiliary],
      ["copula"] = [PartOfSpeech.Copula],
      ["conjunction"] = [PartOfSpeech.Conjunction],
      ["interjection (kandoushi)"] = [PartOfSpeech.Interjection],
      ["particle"] = [PartOfSpeech.Particle],
      ["prefix"] = [PartOfSpeech.Prefix],
      ["suffix"] = [PartOfSpeech.Suffix],
      ["counter"] = [PartOfSpeech.Counter],
      ["numeric"] = [PartOfSpeech.Numeric],

      ["unclassified"] = []
   }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

   /// <summary>The <see cref="PartOfSpeech"/>s the dictionary's <paramref name="dictionaryPartsOfSpeech"/> are to JAStudio.</summary>
   /// <exception cref="ArgumentException">One of them is a part of speech the dictionary didn't have when this was written, so it isn't mapped yet.</exception>
   public static FrozenSet<PartOfSpeech> Of(IEnumerable<string> dictionaryPartsOfSpeech) => dictionaryPartsOfSpeech.SelectMany(Of).ToFrozenSet();

   static PartOfSpeech[] Of(string dictionaryPartOfSpeech) =>
      PartsOfSpeechByDictionaryPartOfSpeech.TryGetValue(dictionaryPartOfSpeech, out var partsOfSpeech)
         ? partsOfSpeech
         : throw new ArgumentException($"The dictionary's part of speech '{dictionaryPartOfSpeech}' isn't mapped to JAStudio's parts of speech. Add it to {nameof(DictionaryPartsOfSpeech)}.");
}
