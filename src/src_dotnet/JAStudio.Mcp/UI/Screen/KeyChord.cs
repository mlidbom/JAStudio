using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>
/// A key pressed while modifier keys are held, such as Ctrl+Shift+S, or a key alone, such as Enter: what <c>press_keys</c> presses, given as<br/>
/// the key's name after the modifiers', joined by <c>+</c>. The key is a key of a US (QWERTY) keyboard, where it is on the keyboard, as<br/>
/// Avalonia's headless platform types it.
/// </summary>
/// <param name="PhysicalKey">The key pressed, by where it is on the keyboard.</param>
/// <param name="ModifiersHeld">The <see cref="ModifierKeys"/> held while it's pressed, such as Ctrl.</param>
public sealed record KeyChord(PhysicalKey PhysicalKey, RawInputModifiers ModifiersHeld)
{
   /// <summary>The key the chord presses, as Avalonia names it in key events: what key bindings, such as JAStudio's shortcuts, match.</summary>
   public Key Key => PhysicalKey.ToQwertyKey();

   /// <summary>What the key types, such as <c>S</c> with Shift held, which key events carry along with the key.</summary>
   public string? KeySymbol => PhysicalKey.ToQwertyKeySymbol(ModifiersHeld.HasFlag(RawInputModifiers.Shift));

   /// <summary>
   /// The text pressing the chord types, as a real keyboard types it along with the key's events, such as <c>s</c>, or null when it types none:<br/>
   /// a shortcut, with Ctrl, Alt or Meta held, types nothing, nor does a key such as Enter or the arrows.
   /// </summary>
   public string? TextTyped => !IsAShortcut && KeySymbol is { Length: > 0 } symbol && !symbol.Any(char.IsControl) ? symbol : null;

   bool IsAShortcut => (ModifiersHeld & (RawInputModifiers.Control | RawInputModifiers.Alt | RawInputModifiers.Meta)) != RawInputModifiers.None;

   /// <summary>The chords in <paramref name="keys"/>, separated by spaces, such as <c>Down Down Enter</c>, in the order they're pressed.</summary>
   public static IReadOnlyList<KeyChord> SequenceIn(string keys)
   {
      var chords = keys.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      return chords.Length > 0 ? [.. chords.Select(Parse)] : throw new ArgumentException("No keys were given to press.", nameof(keys));
   }

   /// <summary>The chord <paramref name="chord"/> names, such as <c>Ctrl+Shift+S</c>: the modifiers' names, then the key's, joined by <c>+</c>.</summary>
   public static KeyChord Parse(string chord)
   {
      var names = chord.Split('+', StringSplitOptions.TrimEntries);
      return new KeyChord(PhysicalKeyNamed(names[^1]), ModifierKeys.Named(names[..^1]));

      PhysicalKey PhysicalKeyNamed(string name) =>
         name.Length == 1 && char.IsAsciiDigit(name[0]) ? Enum.Parse<PhysicalKey>($"Digit{name}")
         : IsANameOfAKey(name, out Key key) && PhysicalKeyOf(key) is {} physicalKey ? physicalKey
         : IsANameOfAKey(name, out PhysicalKey namedPhysicalKey) ? namedPhysicalKey
         : throw new ArgumentException($"""
                                        "{name}" in "{chord}" names no key. A key is named as Avalonia names it, such as S, Enter, Down, Escape, Tab, Space, Back, Delete,
                                        F5 or PageDown, or by where it is on the keyboard, such as Backspace, Digit1 or ArrowDown.
                                        """, nameof(chord));
   }

   /// <summary>
   /// Whether <paramref name="name"/> is the name of a <typeparamref name="TKey"/>, which <paramref name="key"/> then is. A number isn't, though<br/>
   /// <see cref="Enum.TryParse{TEnum}(string,bool,out TEnum)"/> reads it as the key with that value.
   /// </summary>
   static bool IsANameOfAKey<TKey>(string name, out TKey key) where TKey : struct, Enum
   {
      var isANumber = name.All(char.IsAsciiDigit);
      return Enum.TryParse(name, ignoreCase: true, out key) && !isANumber && Enum.IsDefined(key);
   }

   /// <summary>The key on a US keyboard that types <paramref name="key"/>, or null when none does.</summary>
   static PhysicalKey? PhysicalKeyOf(Key key) =>
      Enum.GetValues<PhysicalKey>().Where(it => it != PhysicalKey.None && it.ToQwertyKey() == key).Cast<PhysicalKey?>().FirstOrDefault();
}
