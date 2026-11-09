using System;
using Avalonia.Input;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.UI.Screen;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Screen;

/// <summary>The keys <c>press_keys</c> takes, each a <see cref="KeyChord"/>.</summary>
public class When_parsing_a_key_chord
{
   public class with_modifiers_and_a_letter
   {
      readonly KeyChord _chord = KeyChord.Parse("Ctrl+Shift+S");

      [XF] public void the_key_is_the_letter() => _chord.Key.Must().Be(Key.S);
      [XF] public void the_modifiers_are_held() => _chord.ModifiersHeld.Must().Be(RawInputModifiers.Control | RawInputModifiers.Shift);
   }

   public class named_as_Avalonia_names_a_key
   {
      [XF] public void Down_is_the_down_arrow() => KeyChord.Parse("Down").PhysicalKey.Must().Be(PhysicalKey.ArrowDown);
      [XF] public void Enter_is_the_enter_key() => KeyChord.Parse("Enter").PhysicalKey.Must().Be(PhysicalKey.Enter);
      [XF] public void Back_is_the_backspace() => KeyChord.Parse("Back").PhysicalKey.Must().Be(PhysicalKey.Backspace);
      [XF] public void a_lowercase_letter_is_the_letter() => KeyChord.Parse("o").Key.Must().Be(Key.O);
   }

   public class named_by_where_the_key_is_on_the_keyboard
   {
      [XF] public void Backspace_is_the_backspace() => KeyChord.Parse("Backspace").PhysicalKey.Must().Be(PhysicalKey.Backspace);
   }

   public class a_digit
   {
      [XF] public void is_the_digits_key_not_the_key_with_that_number() => KeyChord.Parse("1").PhysicalKey.Must().Be(PhysicalKey.Digit1);
   }

   public class a_sequence
   {
      [XF] public void is_its_chords_in_order() => KeyChord.SequenceIn("Down Down Enter").Must().SequenceEqual([KeyChord.Parse("Down"), KeyChord.Parse("Down"), KeyChord.Parse("Enter")]);
   }

   public class a_number_of_more_than_one_digit
   {
      [XF] public void names_no_key() => Invoking(() => KeyChord.Parse("12")).Must().Throw<ArgumentException>();
   }

   public class an_unknown_modifier
   {
      [XF] public void is_no_modifier_key() => Invoking(() => KeyChord.Parse("Hyper+S")).Must().Throw<ArgumentException>();
   }
}
