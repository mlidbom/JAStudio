namespace JAStudio.Core.LanguageServices;

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable once UnusedType.Global
public static class KatakanaChart
{
   public const int KIndex = 1;
   public const int SIndex = 2;
   public const int TIndex = 3;
   public const int NIndex = 4;
   public const int HIndex = 5;
   public const int MIndex = 6;
   public const int YIndex = 7;
   public const int RIndex = 8;
   public const int WIndex = 9;

   public static readonly string[] ARow1 = ["ア", "カ", "サ", "タ", "ナ", "ハ", "マ", "ヤ", "ラ", "ワ"];
   public static readonly string[] ARow2 = ["　", "ガ", "ザ", "ダ", "　", "バ", "　", "　", "　", "　"];
   public static readonly string[] ARow3 = ["　", "　", "　", "　", "　", "パ", "　", "　", "　", "　"];

   public static readonly string[] IRow1 = ["イ", "キ", "シ", "チ", "ニ", "ヒ", "ミ", "　", "リ", "　"];
   public static readonly string[] IRow2 = ["　", "ギ", "ジ", "ヂ", "　", "ビ", "　", "　", "　", "　"];
   public static readonly string[] IRow3 = ["　", "　", "　", "　", "　", "ピ", "　", "　", "　", "　"];

   public static readonly string[] URow1 = ["ウ", "ク", "ス", "ツ", "ヌ", "フ", "ム", "ユ", "ル", "　"];
   public static readonly string[] URow2 = ["　", "グ", "ズ", "ヅ", "　", "ブ", "　", "　", "　", "　"];
   public static readonly string[] URow3 = ["　", "　", "　", "　", "　", "プ", "　", "　", "　", "　"];

   public static readonly string[] ERow1 = ["エ", "ケ", "セ", "テ", "ネ", "ヘ", "メ", "　", "レ", "　"];
   public static readonly string[] ERow2 = ["　", "ゲ", "ゼ", "デ", "　", "ベ", "　", "　", "　", "　"];
   public static readonly string[] ERow3 = ["　", "　", "　", "　", "　", "ペ", "　", "　", "　", "　"];

   public static readonly string[] ORow1 = ["オ", "コ", "ソ", "ト", "ノ", "ホ", "モ", "ヨ", "ロ", "ヲ"];
   public static readonly string[] ORow2 = ["　", "ゴ", "ゾ", "ド", "　", "ボ", "　", "　", "　", "　"];
   public static readonly string[] ORow3 = ["　", "　", "　", "　", "　", "ポ", "　", "　", "　", "　"];
}
