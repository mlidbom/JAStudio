using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;

namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// A part of speech a vocab can have, such as <see cref="GodanVerb"/> or <see cref="NaAdjective"/>: one of the closed set <see cref="All"/>,<br/>
/// so a vocab can only have parts of speech JAStudio knows. Most are what the dictionary's parts of speech map to (<see cref="DictionaryPartsOfSpeech"/>),<br/>
/// a few are JAStudio's own, such as <see cref="MasuSuffix"/>.
/// </summary>
/// <remarks>
/// A vocab's parts of speech decide how it's matched in sentences, such as whether it inflects as a godan or an ichidan verb,<br/>
/// and the markers its page shows. The corpus stores each by its <see cref="Name"/>.<br/>
/// Parts of speech are in alphabetical order of their <see cref="Name"/>s (<see cref="CompareTo"/>), wherever a list of them is shown or stored.
/// </remarks>
public sealed class PartOfSpeech : IComparable<PartOfSpeech>
{
   /// <summary>Every part of speech, in the order declared. Declared before them, since each adds itself, and static fields are initialized in the order declared.</summary>
   static readonly List<PartOfSpeech> Declared = [];

   public static readonly PartOfSpeech Noun = new("noun");
   public static readonly PartOfSpeech ProperNoun = new("proper noun");
   public static readonly PartOfSpeech Pronoun = new("pronoun");
   public static readonly PartOfSpeech AdverbialNoun = new("adverbial noun");
   public static readonly PartOfSpeech IndependentNoun = new("independent noun");

   public static readonly PartOfSpeech GodanVerb = new("godan verb");
   public static readonly PartOfSpeech IchidanVerb = new("ichidan verb");
   public static readonly PartOfSpeech SuruVerb = new("suru verb");
   public static readonly PartOfSpeech KuruVerb = new("kuru verb");
   public static readonly PartOfSpeech ZuruVerb = new("zuru verb");
   public static readonly PartOfSpeech NuVerb = new("nu verb");
   public static readonly PartOfSpeech SuVerb = new("su verb");

   /// <summary>An archaic verb of four conjugation stems, the forerunner of most godan verbs.</summary>
   public static readonly PartOfSpeech YodanVerb = new("yodan verb");

   /// <summary>An archaic verb of two conjugation stems, the forerunner of most ichidan verbs.</summary>
   public static readonly PartOfSpeech NidanVerb = new("nidan verb");

   /// <summary>The archaic irregular verb whose plain form ends in -り, such as あり, the forerunner of ある.</summary>
   public static readonly PartOfSpeech RiVerb = new("ri verb");

   /// <summary>A verb the dictionary doesn't say the kind of.</summary>
   public static readonly PartOfSpeech Verb = new("verb");

   public static readonly PartOfSpeech Transitive = new("transitive");
   public static readonly PartOfSpeech Intransitive = new("intransitive");

   /// <summary>A verb of a special class, conjugated irregularly in places, such as 行く, 来る or くれる.</summary>
   public static readonly PartOfSpeech SpecialClass = new("special-class");

   /// <summary>A godan verb of the -aru special class, such as いらっしゃる, whose masu-stem ends in -い rather than -り.</summary>
   public static readonly PartOfSpeech SpecialClassAru = new("special-class-aru");

   public static readonly PartOfSpeech Irregular = new("irregular");

   // ReSharper disable once InconsistentNaming -- approved by Magnus 2026-11-09: "i-adjective" is the domain term, which the naming rule mistakes for an interface prefix
   public static readonly PartOfSpeech IAdjective = new("i-adjective");
   public static readonly PartOfSpeech NaAdjective = new("na-adjective");
   public static readonly PartOfSpeech NoAdjective = new("no-adjective");
   public static readonly PartOfSpeech TaruAdjective = new("taru-adjective");

   /// <summary>An archaic adjective whose plain form ends in -く, the forerunner of most i-adjectives.</summary>
   public static readonly PartOfSpeech KuAdjective = new("ku-adjective");

   /// <summary>An archaic adjective whose plain form ends in -し, the forerunner of i-adjectives ending in -しい.</summary>
   public static readonly PartOfSpeech ShikuAdjective = new("shiku-adjective");

   /// <summary>The archaic, or formal, form of a na-adjective, taking なり or たる.</summary>
   public static readonly PartOfSpeech NariAdjective = new("nari-adjective");

   public static readonly PartOfSpeech PreNounAdjectival = new("pre-noun-adjectival");

   /// <summary>A noun or verb that can stand before a noun to describe it.</summary>
   public static readonly PartOfSpeech Prenominal = new("prenominal");

   public static readonly PartOfSpeech Adverb = new("adverb");
   public static readonly PartOfSpeech ToAdverb = new("to-adverb");
   public static readonly PartOfSpeech Adverbial = new("adverbial");

   public static readonly PartOfSpeech Expression = new("expression");
   public static readonly PartOfSpeech Auxiliary = new("auxiliary");
   public static readonly PartOfSpeech Copula = new("copula");
   public static readonly PartOfSpeech Conjunction = new("conjunction");
   public static readonly PartOfSpeech Interjection = new("interjection");
   public static readonly PartOfSpeech Particle = new("particle");
   public static readonly PartOfSpeech Prefix = new("prefix");
   public static readonly PartOfSpeech Suffix = new("suffix");
   public static readonly PartOfSpeech Counter = new("counter");
   public static readonly PartOfSpeech Numeric = new("numeric");
   public static readonly PartOfSpeech Numeral = new("numeral");
   public static readonly PartOfSpeech Abbreviation = new("abbreviation");

   /// <summary>A mimetic word (giongo or gitaigo), such as キラキラ, which imitates a sound or depicts a state. The dictionary has no part of speech for it.</summary>
   public static readonly PartOfSpeech Mimetic = new("mimetic");

   /// <summary>A word mostly used as part of compounds rather than on its own, such as 泡. The dictionary has no part of speech for it.</summary>
   public static readonly PartOfSpeech InCompounds = new("in compounds");

   /// <summary>JAStudio's own: a word that follows the masu-stem (連用形) of a verb.</summary>
   public static readonly PartOfSpeech MasuSuffix = new("masu-suffix");

   /// <summary>The kinds of verb, as opposed to what a verb may also be, such as <see cref="Transitive"/>.</summary>
   static readonly HashSet<PartOfSpeech> KindsOfVerb = [GodanVerb, IchidanVerb, SuruVerb, KuruVerb, ZuruVerb, NuVerb, SuVerb, YodanVerb, NidanVerb, RiVerb, Verb];

   PartOfSpeech(string name)
   {
      Name = name;
      Declared.Add(this);
   }

   /// <summary>Every part of speech there is, in alphabetical order of <see cref="Name"/>.</summary>
   public static IReadOnlyList<PartOfSpeech> All { get; } = [.. Declared.Order()];

   /// <summary>The part of speech's name, as the corpus stores it and the vocab's editor shows it, such as "godan verb".</summary>
   public string Name { get; }

   /// <summary>Whether it's one of the kinds of verb, such as <see cref="GodanVerb"/>, rather than something a verb may also be, such as <see cref="Transitive"/>.</summary>
   public bool IsAKindOfVerb => KindsOfVerb.Contains(this);

   /// <summary>The part of speech named <paramref name="name"/>, which must be one of <see cref="All"/>.</summary>
   public static PartOfSpeech Named(string name) =>
      All.SingleOrDefault(it => it.Name == name) ?? throw new ArgumentException($"There is no part of speech named '{name}'.", nameof(name));

   /// <summary>Whether there is a part of speech named <paramref name="name"/>.</summary>
   public static bool Exists(string name) => All.Any(it => it.Name == name);

   /// <summary>Orders parts of speech alphabetically by their <see cref="Name"/>s.</summary>
   public int CompareTo(PartOfSpeech? other) => string.CompareOrdinal(Name, other?.Name);

   public override string ToString() => Name;
}
