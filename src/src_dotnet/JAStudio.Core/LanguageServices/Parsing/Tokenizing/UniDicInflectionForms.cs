using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.Parsing.Tokenizing;

/// <summary>
/// An inflection form, the form the tokenizer's dictionary, UniDic, says an inflecting word has in the text (<c>cForm</c>): 連用形-促音便 for the<br/>
/// 書い of 書いた, 終止形-一般 for 書く.
/// </summary>
public class UniDicInflectionForm(string name, string description)
{
   public string Name { get; } = name;
   public string Description { get; } = description;

   public override string ToString() => $"{Name} - {Description}";

   public override bool Equals(object? obj) => obj is UniDicInflectionForm other && Name == other.Name;

   public override int GetHashCode() => Name.GetHashCode();
}

/// <summary>Every inflection form the tokenizer's dictionary, UniDic, gives (<see cref="UniDicInflectionForm"/>), and the groups of them JAStudio's analysis asks about.</summary>
public static class UniDicInflectionForms
{
   static readonly Dictionary<string, UniDicInflectionForm> AllDict = new();

   public static readonly UniDicInflectionForm Unknown = Add("*", "Unknown");

   static UniDicInflectionForms()
   {
      foreach(var (name, description) in EveryFormTheDictionaryHas) Add(name, description);
   }

   static UniDicInflectionForm Add(string name, string description)
   {
      var form = new UniDicInflectionForm(name, description);
      AllDict[name] = form;
      return form;
   }

   public static UniDicInflectionForm GetByName(string name) => AllDict[name];

   /// <summary>Whether <paramref name="name"/> is the name of an inflected form the tokenizer gives, such as 連用形-促音便.</summary>
   public static bool IsKnown(string name) => AllDict.ContainsKey(name);

   /// <summary>Every inflected form the tokenizer gives: all but <see cref="Unknown"/>.</summary>
   internal static IEnumerable<UniDicInflectionForm> All => AllDict.Values.Where(it => !Equals(it, Unknown));

   /// <summary>Whether <paramref name="form"/> is one of the forms named <paramref name="names"/>.</summary>
   static bool IsOneOf(UniDicInflectionForm form, params string[] names) => names.Contains(form.Name);

   /// <summary>
   /// The dictionary form: the conclusive (終止形-一般), ending a sentence, and the attributive (連体形-一般), before a noun. They're the same for<br/>
   /// verbs and i-adjectives; the tokenizer tells them apart by what follows.
   /// </summary>
   public static bool IsDictionaryForm(UniDicInflectionForm form) => IsOneOf(form, "終止形-一般", "連体形-一般");

   /// <summary>The continuative, the stem ます attaches to, such as the 書き of 書きます and the 食べ of 食べた (連用形-一般).</summary>
   public static UniDicInflectionForm Continuative => GetByName("連用形-一般");

   /// <summary>
   /// A godan verb's or an i-adjective's continuative before た and て, with its sound change: the 書い of 書いた (連用形-イ音便), the 言っ of 言った<br/>
   /// and the よかっ of よかった (連用形-促音便), the 読ん of 読んだ (連用形-撥音便), the 問う of 問うた (連用形-ウ音便).
   /// </summary>
   public static bool IsAPastTenseStem(UniDicInflectionForm form) => IsOneOf(form, "連用形-促音便", "連用形-イ音便", "連用形-撥音便", "連用形-ウ音便");

   /// <summary>The irrealis, the stem ない and the passive attach to, such as the 書か of 書かない (未然形), with its variants.</summary>
   public static bool IsIrrealis(UniDicInflectionForm form) => form.Name.StartsWith("未然形", System.StringComparison.Ordinal);

   /// <summary>The irrealis, such as the 書か of 書かない (未然形-一般).</summary>
   public static UniDicInflectionForm Irrealis => GetByName("未然形-一般");

   /// <summary>The hypothetical, the stem ば attaches to, such as the 書け of 書けば (仮定形-一般).</summary>
   public static UniDicInflectionForm Hypothetical => GetByName("仮定形-一般");

   /// <summary>The imperative, such as 書け and 食べろ (命令形).</summary>
   public static UniDicInflectionForm Imperative => GetByName("命令形");

   /// <summary>The volitional, such as 書こう and 食べよう, which the tokenizer gives as one token (意志推量形).</summary>
   public static UniDicInflectionForm Volitional => GetByName("意志推量形");

   /// <summary>Every inflection form UniDic 2025.12 gives, with its English: the same in the spoken (unidic-csj-202512) and the written (unidic-cwj-202512).</summary>
   static readonly (string Name, string Description)[] EveryFormTheDictionaryHas =
   [
      ("ク語法", "Ku-nominalization (classical)"),
      ("仮定形-一般", "Hypothetical"),
      ("仮定形-融合", "Hypothetical, contracted"),
      ("命令形", "Imperative"),
      ("已然形-一般", "Realis (classical)"),
      ("已然形-補助", "Realis (classical), auxiliary"),
      ("意志推量形", "Volitional/conjectural"),
      ("未然形-サ", "Irrealis, sa"),
      ("未然形-セ", "Irrealis, se"),
      ("未然形-一般", "Irrealis"),
      ("未然形-撥音便", "Irrealis with ん sound change"),
      ("未然形-融合", "Irrealis, contracted"),
      ("未然形-補助", "Irrealis, auxiliary"),
      ("終止形-一般", "Conclusive (dictionary form)"),
      ("終止形-促音便", "Conclusive with っ sound change"),
      ("終止形-撥音便", "Conclusive with ん sound change"),
      ("終止形-省略", "Conclusive, shortened"),
      ("終止形-融合", "Conclusive, contracted"),
      ("終止形-補助", "Conclusive, auxiliary"),
      ("語幹-サ", "Stem, sa"),
      ("語幹-一般", "Stem"),
      ("連体形-イ音便", "Attributive with い sound change"),
      ("連体形-ウ音便", "Attributive with う sound change"),
      ("連体形-一般", "Attributive (dictionary form)"),
      ("連体形-促音便", "Attributive with っ sound change"),
      ("連体形-撥音便", "Attributive with ん sound change"),
      ("連体形-省略", "Attributive, shortened"),
      ("連体形-補助", "Attributive, auxiliary"),
      ("連用形-イ音便", "Continuative with い sound change"),
      ("連用形-ウ音便", "Continuative with う sound change"),
      ("連用形-ト", "Continuative, to"),
      ("連用形-ニ", "Continuative, ni"),
      ("連用形-一般", "Continuative (masu stem)"),
      ("連用形-促音便", "Continuative with っ sound change"),
      ("連用形-撥音便", "Continuative with ん sound change"),
      ("連用形-省略", "Continuative, shortened"),
      ("連用形-融合", "Continuative, contracted"),
      ("連用形-補助", "Continuative, auxiliary")
   ];
}
