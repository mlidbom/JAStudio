#pragma warning disable IDE0051 //unused members

using System.Collections.Generic;
using Compze.Utilities.Logging;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public class InflectionType
{
   public string Base { get; }
   public string Name { get; }
   public string Description { get; }

   public InflectionType(string name, string description, string baseValue)
   {
      Name = name;
      Description = description;
      Base = baseValue;
   }

   public override string ToString() => $"{Name} - {Description}";

   public override bool Equals(object? obj)
   {
      if(obj is InflectionType other)
      {
         return Name == other.Name;
      }

      return false;
   }

   public override int GetHashCode() => Name.GetHashCode();
}

public static class InflectionTypes
{
   static InflectionTypes()
   {
      List<InflectionType> unused =
      [
         Godan.Su, Ichidan.Regular, Adjective.IEnding, Sahen.Suru, Kahen.KuruKana, Bungo.Beshi, Yodan.BaEnding,
         Ruhen.RaHen, Nidan.LowerDa, Other.Indeclinable, Special.Masu
      ];
   }

   static readonly Dictionary<string, InflectionType> AllDict = new();

   static InflectionType AddForm(string name, string description)
   {
      var parts = name.Split('・');
      var baseValue = parts[0];
      var form = new InflectionType(name, description, baseValue);
      AllDict[name] = form;
      return form;
   }

   public static InflectionType GetByName(string name) => AllDict[name];

   public static readonly InflectionType Unknown = AddForm("*", "Unknown");

   public static class Godan
   {
      public const string Base = "五段";
      public static readonly InflectionType Su = AddForm("五段・サ行", "Godan verb with 'su' ending");
      public static readonly InflectionType Mu = AddForm("五段・マ行", "Godan verb with 'mu' ending");
      public static readonly InflectionType Bu = AddForm("五段・バ行", "Godan verb with 'bu' ending");
      public static readonly InflectionType GuEnding = AddForm("五段・ガ行", "Godan verb with 'gu' ending");
      public static readonly InflectionType Tsu = AddForm("五段・タ行", "Godan verb with 'tsu' ending");
      public static readonly InflectionType Nu = AddForm("五段・ナ行", "Godan verb with 'nu' ending");
      public static readonly InflectionType Ru = AddForm("五段・ラ行", "Godan verb with 'ru' ending");

      public static readonly InflectionType RuSpecial =
         AddForm("五段・ラ行特殊", "Godan verb with 'ru' ending - Irregular conjugation");

      public static readonly InflectionType RuEndingAru = AddForm("五段・ラ行アル", "Godan verb 'aru'");

      public static readonly InflectionType UGemination =
         AddForm("五段・ワ行促音便", "Godan verb with 'u' ending and 'っ' consonant assimilation");

      public static readonly InflectionType UUSound = AddForm("五段・ワ行ウ音便",
                                                              "Godan verb with 'u' ending and 'u' sound change");

      public static readonly InflectionType KuGeminationYuku = AddForm("五段・カ行促音便ユク", "Godan verb 'yuku'");

      public static readonly InflectionType KuGemination =
         AddForm("五段・カ行促音便", "Godan verb with 'ku' ending and consonant assimilation");

      public static readonly InflectionType KuISound = AddForm("五段・カ行イ音便",
                                                               "Godan verb with 'ku' ending and 'i' sound change");
   }

   public static class Ichidan
   {
      public const string Base = "一段";
      public static readonly InflectionType Regular = AddForm("一段", "Ichidan verb");
      public static readonly InflectionType Eru = AddForm("一段・得ル", "Ichidan verb 'eru'");
      public static readonly InflectionType Kureru = AddForm("一段・クレル", "Ichidan verb 'kureru'");
   }

   public static class Adjective
   {
      public const string Base = "形容詞";
      public static readonly InflectionType IEnding = AddForm("形容詞・イ段", "I-adjective");
      public static readonly InflectionType AuoEnding = AddForm("形容詞・アウオ段", "Adjective with 'a', 'u', 'o' row");
      public static readonly InflectionType Ii = AddForm("形容詞・イイ", "Adjective 'いい'");
   }

   public static class Sahen
   {
      public const string Base = "サ変";
      public static readonly InflectionType Suru = AddForm("サ変・スル", "Suru verb");
      public static readonly InflectionType SuruCompound = AddForm("サ変・－スル", "Suru compound verb");
      public static readonly InflectionType Zuru = AddForm("サ変・－ズル", "Zuru verb - Classical variation of suru");
   }

   public static class Kahen
   {
      public const string Base = "カ変";
      public static readonly InflectionType KuruKanji = AddForm("カ変・来ル", "Kuru verb in kanji");
      public static readonly InflectionType KuruKana = AddForm("カ変・クル", "Kuru verb in kana");
   }

   public static class Bungo
   {
      public const string Base = "文語";
      public static readonly InflectionType Nari = AddForm("文語・ナリ", "Classical 'nari' - Classical Japanese copula");
      public static readonly InflectionType Ru = AddForm("文語・ル", "Classical 'ru' ending - Classical verb ending");

      public static readonly InflectionType Ki = AddForm("文語・キ",
                                                         "Classical 'ki' ending - Classical past tense marker");

      public static readonly InflectionType Gotoshi = AddForm("文語・ゴトシ",
                                                              "Classical 'gotoshi' - Classical expression meaning 'like/as if'");

      public static readonly InflectionType Keri = AddForm("文語・ケリ",
                                                           "Classical 'keri' - Classical perfect aspect marker");

      public static readonly InflectionType Ri = AddForm("文語・リ", "Classical 'ri' - Classical verb ending");

      public static readonly InflectionType Beshi = AddForm("文語・ベシ",
                                                            "Classical 'beshi' - Classical expression of obligation/probability");
   }

   public static class Special
   {
      public const string Base = "特殊";
      public static readonly InflectionType Masu = AddForm("特殊・マス", "Special 'masu'");
      public static readonly InflectionType Ya = AddForm("特殊・ヤ", "Special 'ya' - Dialectal copula/question marker");
      public static readonly InflectionType Ja = AddForm("特殊・ジャ", "Special 'ja' - Dialectal copula");
      public static readonly InflectionType Ta = AddForm("特殊・タ", "Special 'ta' - Past tense marker");
      public static readonly InflectionType Nai = AddForm("特殊・ナイ", "Special 'nai' - Negative form");
      public static readonly InflectionType Nu = AddForm("特殊・ヌ", "Special 'nu' - Classical negative");
      public static readonly InflectionType Da = AddForm("特殊・ダ", "Special 'da' - Copula (is/am/are)");
      public static readonly InflectionType Tai = AddForm("特殊・タイ", "Special 'tai' - Desire form (-want to do)");
      public static readonly InflectionType Desu = AddForm("特殊・デス", "Special 'desu' - Polite copula");
   }

   public static class Yodan
   {
      public const string Base = "四段";
      public static readonly InflectionType HaEnding = AddForm("四段・ハ行", "Classical yodan verb with 'ha' ending");
      public static readonly InflectionType BaEnding = AddForm("四段・バ行", "Classical yodan verb with 'ba' ending");
   }

   public static class Ruhen
   {
      public const string Base = "ラ変";
      public static readonly InflectionType RaHen = AddForm("ラ変", "Classical ra-hen irregular verb");
   }

   public static class Nidan
   {
      public const string Base = "下二";
      public static readonly InflectionType LowerDa = AddForm("下二・ダ行", "Lower bigrade with 'da' ending");
      public static readonly InflectionType LowerTa = AddForm("下二・タ行", "Lower bigrade with 'ta' ending");
   }

   public static class Other
   {
      public static readonly InflectionType Indeclinable =
         AddForm("不変化型", "Indeclinable type - Words that don't conjugate");
   }
}
