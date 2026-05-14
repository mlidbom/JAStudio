using System;

namespace JAStudio.UI.Utils;

/// <summary>
/// Provides keyboard accelerator formatting for menu items.
/// Ported from jastudio/ui/menus/menu_utils/shortcutfinger.py
/// 
/// In Avalonia/WPF, the underscore character '_' is used to denote access keys.
/// This class mirrors the Python implementation which uses specific keyboard positions
/// for consistent muscle-memory navigation.
/// </summary>
public static class ShortcutFinger
{
   static string FormatFinger(char finger, string text) => $"_{finger} {text}";

   // Home row keys (most accessible)
   public static string Home1(string text) => FormatFinger('u', text);
   public static string Home2(string text) => FormatFinger('e', text);
   public static string Home3(string text) => FormatFinger('o', text);
   public static string Home4(string text) => FormatFinger('a', text);
   public static string Home5(string text) => FormatFinger('i', text);

   // Upper row keys
   public static string Up1(string text) => FormatFinger('p', text);
   public static string Up2(string text) => FormatFinger('ö', text);
   public static string Up3(string text) => FormatFinger('ä', text);
   public static string Up4(string text) => FormatFinger('å', text);
   public static string Up5(string text) => FormatFinger('y', text);

   // Lower row keys
   public static string Down1(string text) => FormatFinger('k', text);
   public static string Down2(string text) => FormatFinger('j', text);

   public static string Down3(string text) => FormatFinger('q', text);

   // down4 is commented out in Python (period key)
   public static string Down5(string text) => FormatFinger('x', text);
   public static string Down6(string text) => FormatFinger('b', text);

   /// <summary>
   /// Returns text without any accelerator (for items that don't need shortcuts).
   /// </summary>
   public static string None(string text) => text;

   // Numpad functions (0-9 mapped to specific fingers)
   static readonly Func<string, string>[] NumpadFunctions =
   [
      Up4, Up3, Up2, Up1, Up5, Down3, Down2, Down1, Down5, Down6
   ];

   public static string Numpad(int index, string text)
   {
      if(index < NumpadFunctions.Length)
         return NumpadFunctions[index]($"{index + 1} {text}");
      return None(text);
   }

   // Priority order for automatic assignment
   static readonly Func<string, string>[] FingersByPriorityOrder =
   [
      Home1, Home2, Home3, Home4, Home5,
      Up1, Up2, Up3, Up4, Up5,
      Down1, Down2, Down3, Down5
   ];

   public static string FingerByPriorityOrder(int index, string text)
   {
      if(index < FingersByPriorityOrder.Length)
         return FingersByPriorityOrder[index](text);
      return None(text);
   }

   /// <summary>
   /// Remove the shortcut prefix from a formatted string.
   /// Example: "_u Config" -> "Config"
   /// </summary>
   public static string RemoveShortcutText(string text)
   {
      var parts = text.Split(' ', 2);
      return parts.Length > 1 ? parts[1] : text;
   }
}
