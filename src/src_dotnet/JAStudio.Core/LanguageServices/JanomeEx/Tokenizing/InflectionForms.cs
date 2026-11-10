using System.Collections.Generic;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global

#pragma warning disable IDE0051 //unused members

namespace JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

public class InflectionForm(string name, string description)
{
   public string Name { get; } = name;
   public string Description { get; } = description;

   public override string ToString() => $"{Name} - {Description}";

   public override bool Equals(object? obj)
   {
      if(obj is InflectionForm other)
      {
         return Name == other.Name;
      }

      return false;
   }

   public override int GetHashCode() => Name.GetHashCode();
}

public static class InflectionForms
{
   static readonly Dictionary<string, InflectionForm> AllDict = new();

   static InflectionForm AddForm(string name, string description)
   {
      List<InflectionForm> unused =
      [
         Basic.Classical, Continuative.RenyoukeiMasuStem, Misc.GaruConnection, Irrealis.GeneralIrrealisMizenkei,
         Hypothetical.GeneralHypotheticalKateikei, ImperativeMeireikei.E, NounConnection.GeneralNounConnection
      ];

      var form = new InflectionForm(name, description);
      AllDict[name] = form;
      return form;
   }

   public static InflectionForm GetByName(string name) => AllDict[name];

   /// <summary>Whether <paramref name="name"/> is the name of an inflected form the tokenizer gives, such as 連用タ接続.</summary>
   public static bool IsKnown(string name) => AllDict.ContainsKey(name);

   public static readonly InflectionForm Unknown = AddForm("*", "Unknown");

   public static class Basic
   {
      public static readonly InflectionForm DictionaryForm = AddForm("基本形", "Dictionary form");

      public static readonly InflectionForm
         Gemination = AddForm("基本形-促音便", "Dictionary form with consonant doubling");

      public static readonly InflectionForm Euphonic = AddForm("音便基本形", "Dictionary form with sound changes");
      public static readonly InflectionForm Classical = AddForm("文語基本形", "Dictionary form in classical Japanese");
   }

   public static class Continuative
   {
      public static readonly InflectionForm RenyoukeiMasuStem =
         AddForm("連用形", "Continuative/masu-stem verbs/adjective");

      public static readonly InflectionForm TeConnection = AddForm("連用テ接続", "Continuative te-connection");
      public static readonly InflectionForm TaConnection = AddForm("連用タ接続", "Continuative ta-connection");
      public static readonly InflectionForm DeConnection = AddForm("連用デ接続", "Continuative de-connection");
      public static readonly InflectionForm NiConnection = AddForm("連用ニ接続", "Continuative ni-connection");
      public static readonly InflectionForm GozaiConnection = AddForm("連用ゴザイ接続", "Continuative gozai connection");

      public static readonly HashSet<InflectionForm> TeConnectionForms = [TeConnection, DeConnection];
   }

   public static class Misc
   {
      public static readonly InflectionForm GaruConnection = AddForm("ガル接続", "Garu connection");
   }

   public static class Irrealis
   {
      public static readonly InflectionForm GeneralIrrealisMizenkei =
         AddForm("未然形", "Irrealis/a-stem : negatives/auxiliaries");

      public static readonly InflectionForm SpecialIrrealis = AddForm("未然特殊", "Irrealis - Special");

      public static readonly InflectionForm
         UConnection = AddForm("未然ウ接続", "Irrealis u-connection - volitional \"u\"");

      public static readonly InflectionForm NuConnection =
         AddForm("未然ヌ接続", "Irrealis nu-connection - negative \"nu\"");

      public static readonly InflectionForm ReruConnection =
         AddForm("未然レル接続", "Irrealis reru-connection - passive/potential \"reru\"");

      public static readonly HashSet<InflectionForm> AllForms = [GeneralIrrealisMizenkei, SpecialIrrealis, UConnection, NuConnection, ReruConnection];
   }

   public static class Hypothetical
   {
      public static readonly InflectionForm GeneralHypotheticalKateikei =
         AddForm("仮定形", "Hypothetical/potential/e-stem verbs+adjectives.");

      public static readonly InflectionForm Contraction1 = AddForm("仮定縮約１", "Hypothetical contraction version 1");
      public static readonly InflectionForm Contraction2 = AddForm("仮定縮約２", "Hypothetical contraction version 2");
   }

   public static class ImperativeMeireikei
   {
      public static readonly InflectionForm E = AddForm("命令ｅ", "Imperative/command/meireikei -  e");
      public static readonly InflectionForm Ro = AddForm("命令ｒｏ", "Imperative/command/meireikei - ro");
      public static readonly InflectionForm Yo = AddForm("命令ｙｏ", "Imperative/command/meireikei - yo");
      public static readonly InflectionForm I = AddForm("命令ｉ", "Imperative/command/meireikei - i");

      public static readonly HashSet<InflectionForm> GodanForms = [E, I];
      public static readonly HashSet<InflectionForm> IchidanForms = [Ro, Yo];
   }

   public static class NounConnection
   {
      public static readonly InflectionForm GeneralNounConnection = AddForm("体言接続", "Noun connection");
      public static readonly InflectionForm Special1 = AddForm("体言接続特殊", "Special noun connection");
      public static readonly InflectionForm Special2 = AddForm("体言接続特殊２", "Special noun connection 2");
   }
}
