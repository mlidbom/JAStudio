using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

/// <summary>
/// An inflection type, the conjugation pattern the tokenizer's dictionary, UniDic, gives a word that inflects (<c>cType</c>): 五段-カ行 for 書く,<br/>
/// 下一段-バ行 for 食べる, 助動詞-タ for た.
/// </summary>
public class InflectionType
{
   /// <summary>
   /// The family the type is in: the part of its name before the first <c>-</c>, such as 五段 for 五段-カ行 and 助動詞 for 助動詞-タ, or its whole<br/>
   /// name when it has none, as サ行変格 has.
   /// </summary>
   public string Family { get; }

   public string Name { get; }
   public string Description { get; }

   public InflectionType(string name, string description)
   {
      Name = name;
      Description = description;
      Family = name.Split('-')[0];
   }

   public override string ToString() => $"{Name} - {Description}";

   public override bool Equals(object? obj) => obj is InflectionType other && Name == other.Name;

   public override int GetHashCode() => Name.GetHashCode();
}

/// <summary>Every inflection type the tokenizer's dictionary, UniDic, gives (<see cref="InflectionType"/>), and those JAStudio's analysis asks about by name.</summary>
public static class InflectionTypes
{
   static readonly Dictionary<string, InflectionType> AllDict = new();

   public static readonly InflectionType Unknown = Add("*", "Unknown");

   static InflectionTypes()
   {
      foreach(var (name, description) in EveryTypeTheDictionaryHas) Add(name, description);
   }

   static InflectionType Add(string name, string description)
   {
      var type = new InflectionType(name, description);
      AllDict[name] = type;
      return type;
   }

   public static InflectionType GetByName(string name) => AllDict[name];

   /// <summary>Whether <paramref name="name"/> is the name of an inflection type the tokenizer gives, such as 五段-ラ行.</summary>
   public static bool IsKnown(string name) => AllDict.ContainsKey(name);

   /// <summary>Whether <paramref name="family"/> is the family of one or more inflection types, the part of their names before <c>-</c>, such as 五段 for 五段-ラ行.</summary>
   public static bool IsKnownFamily(string family) => AllDict.Values.Any(it => it.Family == family);

   /// <summary>The names of every inflection type the tokenizer gives, and of every family of them, each once: all but <see cref="Unknown"/>'s.</summary>
   internal static IEnumerable<string> NamesOfTheTypesAndTheirFamilies =>
      AllDict.Values.Where(it => !Equals(it, Unknown)).SelectMany(it => new[] { it.Name, it.Family }).Distinct();

   /// <summary>
   /// The English of <paramref name="name"/>, an inflection type or a family of them: a type's <see cref="InflectionType.Description"/>, such as "Godan verb<br/>
   /// with 'ru' ending" for 五段-ラ行, and a family's as "every godan type" for 五段.
   /// </summary>
   public static string EnglishOf(string name) => IsKnown(name) ? GetByName(name).Description : $"every {EnglishNamesOfTheFamilies[name]} type";

   /// <summary>The English names of the families of inflection types that aren't types themselves, as サ行変格, the suru-verb type, is.</summary>
   static readonly Dictionary<string, string> EnglishNamesOfTheFamilies = new()
   {
      [Godan.Family] = "godan",
      [Ichidan.UpperFamily] = "upper ichidan",
      [Ichidan.LowerFamily] = "lower ichidan",
      [Auxiliary.Family] = "auxiliary",
      ["文語四段"] = "classical quadrigrade",
      ["文語上一段"] = "classical upper monograde",
      ["文語上二段"] = "classical upper bigrade",
      ["文語下二段"] = "classical lower bigrade",
      ["文語助動詞"] = "classical auxiliary",
      ["文語形容詞"] = "classical i-adjective"
   };

   public static class Godan
   {
      public const string Family = "五段";
   }

   /// <summary>
   /// The ichidan verbs' types: the upper (上一段, such as 見る) and the lower (下一段, such as 食べる), and the passive and potential auxiliary れる<br/>
   /// and られる (助動詞-レル), which inflects as they do.
   /// </summary>
   public static class Ichidan
   {
      public const string UpperFamily = "上一段";
      public const string LowerFamily = "下一段";

      /// <summary>Whether <paramref name="type"/> inflects as an ichidan verb.</summary>
      public static bool Includes(InflectionType type) => type.Family is UpperFamily or LowerFamily || Equals(type, Auxiliary.Reru);
   }

   public static class Adjective
   {
      public const string Family = "形容詞";
   }

   public static class Sahen
   {
      public const string Family = "サ行変格";
   }

   public static class Kahen
   {
      public const string Family = "カ行変格";
   }

   public static class Auxiliary
   {
      public const string Family = "助動詞";
      public static InflectionType Ta => GetByName("助動詞-タ");
      public static InflectionType Masu => GetByName("助動詞-マス");
      public static InflectionType Nai => GetByName("助動詞-ナイ");
      public static InflectionType Reru => GetByName("助動詞-レル");
      public static InflectionType Tai => GetByName("助動詞-タイ");
   }

   public static class Classical
   {
      /// <summary>The classical perfective り, which the tokenizer reads the る of an ichidan verb it doesn't know as, such as 磨げる's.</summary>
      public static InflectionType Ri => GetByName("文語助動詞-リ");
   }

   /// <summary>Every inflection type UniDic 2025.12 (unidic-cwj-202512) gives, with its English.</summary>
   static readonly (string Name, string Description)[] EveryTypeTheDictionaryHas =
   [
      ("カ行変格", "Kuru verb"),
      ("サ行変格", "Suru verb"),
      ("上一段-ア行", "Ichidan verb, upper (a-row)"),
      ("上一段-カ行", "Ichidan verb, upper (ka-row)"),
      ("上一段-ガ行", "Ichidan verb, upper (ga-row)"),
      ("上一段-ザ行", "Ichidan verb, upper (za-row)"),
      ("上一段-タ行", "Ichidan verb, upper (ta-row)"),
      ("上一段-ナ行", "Ichidan verb, upper (na-row)"),
      ("上一段-ハ行", "Ichidan verb, upper (ha-row)"),
      ("上一段-バ行", "Ichidan verb, upper (ba-row)"),
      ("上一段-マ行", "Ichidan verb, upper (ma-row)"),
      ("上一段-ラ行", "Ichidan verb, upper (ra-row)"),
      ("下一段-ア行", "Ichidan verb, lower (a-row)"),
      ("下一段-カ行", "Ichidan verb, lower (ka-row)"),
      ("下一段-ガ行", "Ichidan verb, lower (ga-row)"),
      ("下一段-サ行", "Ichidan verb, lower (sa-row)"),
      ("下一段-ザ行", "Ichidan verb, lower (za-row)"),
      ("下一段-タ行", "Ichidan verb, lower (ta-row)"),
      ("下一段-ダ行", "Ichidan verb, lower (da-row)"),
      ("下一段-ナ行", "Ichidan verb, lower (na-row)"),
      ("下一段-ハ行", "Ichidan verb, lower (ha-row)"),
      ("下一段-バ行", "Ichidan verb, lower (ba-row)"),
      ("下一段-マ行", "Ichidan verb, lower (ma-row)"),
      ("下一段-ラ行", "Ichidan verb, lower (ra-row)"),
      ("五段-カ行", "Godan verb with 'ku' ending"),
      ("五段-ガ行", "Godan verb with 'gu' ending"),
      ("五段-サ行", "Godan verb with 'su' ending"),
      ("五段-タ行", "Godan verb with 'tsu' ending"),
      ("五段-ナ行", "Godan verb with 'nu' ending"),
      ("五段-バ行", "Godan verb with 'bu' ending"),
      ("五段-マ行", "Godan verb with 'mu' ending"),
      ("五段-ラ行", "Godan verb with 'ru' ending"),
      ("五段-ワア行", "Godan verb with 'u' ending"),
      ("助動詞-ザマス", "Auxiliary ザマス"),
      ("助動詞-ジャ", "Auxiliary ジャ"),
      ("助動詞-タ", "Auxiliary タ"),
      ("助動詞-タイ", "Auxiliary タイ"),
      ("助動詞-ダ", "Auxiliary ダ"),
      ("助動詞-ダス", "Auxiliary ダス"),
      ("助動詞-デス", "Auxiliary デス"),
      ("助動詞-ドス", "Auxiliary ドス"),
      ("助動詞-ナイ", "Auxiliary ナイ"),
      ("助動詞-ナン", "Auxiliary ナン"),
      ("助動詞-ナンダ", "Auxiliary ナンダ"),
      ("助動詞-ヌ", "Auxiliary ヌ"),
      ("助動詞-ヒン", "Auxiliary ヒン"),
      ("助動詞-ヘン", "Auxiliary ヘン"),
      ("助動詞-マイ", "Auxiliary マイ"),
      ("助動詞-マス", "Auxiliary マス"),
      ("助動詞-ヤ", "Auxiliary ヤ"),
      ("助動詞-ヤス", "Auxiliary ヤス"),
      ("助動詞-ヤン", "Auxiliary ヤン"),
      ("助動詞-ラシイ", "Auxiliary ラシイ"),
      ("助動詞-レル", "Auxiliary レル"),
      ("助動詞-ンス", "Auxiliary ンス"),
      ("形容詞", "I-adjective"),
      ("文語カ行変格", "Classical kuru verb"),
      ("文語サ行変格", "Classical suru verb"),
      ("文語ナ行変格", "Classical na-row irregular verb"),
      ("文語ラ行変格", "Classical ra-row irregular verb"),
      ("文語上一段-カ行", "Classical upper monograde verb (ka-row)"),
      ("文語上一段-マ行", "Classical upper monograde verb (ma-row)"),
      ("文語上一段-ワ行", "Classical upper monograde verb (wa-row)"),
      ("文語上二段-カ行", "Classical upper bigrade verb (ka-row)"),
      ("文語上二段-ガ行", "Classical upper bigrade verb (ga-row)"),
      ("文語上二段-タ行", "Classical upper bigrade verb (ta-row)"),
      ("文語上二段-バ行", "Classical upper bigrade verb (ba-row)"),
      ("文語上二段-マ行", "Classical upper bigrade verb (ma-row)"),
      ("文語上二段-ラ行", "Classical upper bigrade verb (ra-row)"),
      ("文語下二段-ア行", "Classical lower bigrade verb (a-row)"),
      ("文語下二段-カ行", "Classical lower bigrade verb (ka-row)"),
      ("文語下二段-ガ行", "Classical lower bigrade verb (ga-row)"),
      ("文語下二段-サ行", "Classical lower bigrade verb (sa-row)"),
      ("文語下二段-ザ行", "Classical lower bigrade verb (za-row)"),
      ("文語下二段-タ行", "Classical lower bigrade verb (ta-row)"),
      ("文語下二段-ダ行", "Classical lower bigrade verb (da-row)"),
      ("文語下二段-ナ行", "Classical lower bigrade verb (na-row)"),
      ("文語下二段-ハ行", "Classical lower bigrade verb (ha-row)"),
      ("文語下二段-バ行", "Classical lower bigrade verb (ba-row)"),
      ("文語下二段-マ行", "Classical lower bigrade verb (ma-row)"),
      ("文語下二段-ヤ行", "Classical lower bigrade verb (ya-row)"),
      ("文語下二段-ラ行", "Classical lower bigrade verb (ra-row)"),
      ("文語下二段-ワ行", "Classical lower bigrade verb (wa-row)"),
      ("文語助動詞-キ", "Classical auxiliary キ"),
      ("文語助動詞-ケリ", "Classical auxiliary ケリ"),
      ("文語助動詞-ゴトシ", "Classical auxiliary ゴトシ"),
      ("文語助動詞-ズ", "Classical auxiliary ズ"),
      ("文語助動詞-タリ-完了", "Classical auxiliary タリ-完了"),
      ("文語助動詞-タリ-断定", "Classical auxiliary タリ-断定"),
      ("文語助動詞-ナリ-断定", "Classical auxiliary ナリ-断定"),
      ("文語助動詞-ベシ", "Classical auxiliary ベシ"),
      ("文語助動詞-マジ", "Classical auxiliary マジ"),
      ("文語助動詞-ム", "Classical auxiliary ム"),
      ("文語助動詞-リ", "Classical auxiliary リ"),
      ("文語四段-カ行", "Classical quadrigrade verb (ka-row)"),
      ("文語四段-ガ行", "Classical quadrigrade verb (ga-row)"),
      ("文語四段-サ行", "Classical quadrigrade verb (sa-row)"),
      ("文語四段-タ行", "Classical quadrigrade verb (ta-row)"),
      ("文語四段-ハ行", "Classical quadrigrade verb (ha-row)"),
      ("文語四段-バ行", "Classical quadrigrade verb (ba-row)"),
      ("文語四段-マ行", "Classical quadrigrade verb (ma-row)"),
      ("文語四段-ラ行", "Classical quadrigrade verb (ra-row)"),
      ("文語形容詞-ク", "Classical i-adjective, ク type"),
      ("文語形容詞-シク", "Classical i-adjective, シク type"),
      ("無変化型", "Uninflecting"),
      ("特殊型", "Special")
   ];
}
