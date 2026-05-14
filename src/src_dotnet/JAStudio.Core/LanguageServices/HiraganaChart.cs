// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace JAStudio.Core.LanguageServices;

// ReSharper disable once UnusedType.Global
public static class HiraganaChart
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

   public static readonly string[] ARow1 = ["あ", "か", "さ", "た", "な", "は", "ま", "や", "ら", "わ"];
   public static readonly string[] ARow2 = ["　", "が", "ざ", "だ", "　", "ば", "　", "　", "　", "　"];
   public static readonly string[] ARow3 = ["　", "　", "　", "　", "　", "ぱ", "　", "　", "　", "　"];

   public static readonly string[] IRow1 = ["い", "き", "し", "ち", "に", "ひ", "み", "　", "り", "　"];
   public static readonly string[] IRow2 = ["　", "ぎ", "じ", "ぢ", "　", "び", "　", "　", "　", "　"];
   public static readonly string[] IRow3 = ["　", "　", "　", "　", "　", "ぴ", "　", "　", "　", "　"];

   public static readonly string[] URow1 = ["う", "く", "す", "つ", "ぬ", "ふ", "む", "ゆ", "る", "　"];
   public static readonly string[] URow2 = ["　", "ぐ", "ず", "づ", "　", "ぶ", "　", "　", "　", "　"];
   public static readonly string[] URow3 = ["　", "　", "　", "　", "　", "ぷ", "　", "　", "　", "　"];

   public static readonly string[] ERow1 = ["え", "け", "せ", "て", "ね", "へ", "め", "　", "れ", "　"];
   public static readonly string[] ERow2 = ["　", "げ", "ぜ", "で", "　", "べ", "　", "　", "　", "　"];
   public static readonly string[] ERow3 = ["　", "　", "　", "　", "　", "ぺ", "　", "　", "　", "　"];

   public static readonly string[] ORow1 = ["お", "こ", "そ", "と", "の", "ほ", "も", "よ", "ろ", "を"];
   public static readonly string[] ORow2 = ["　", "ご", "ぞ", "ど", "　", "ぼ", "　", "　", "　", "　"];
   public static readonly string[] ORow3 = ["　", "　", "　", "　", "　", "ぽ", "　", "　", "　", "　"];
}
