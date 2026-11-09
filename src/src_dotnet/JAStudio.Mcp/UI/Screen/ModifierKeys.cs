using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;

namespace JAStudio.Mcp.UI.Screen;

/// <summary>The modifier keys held while a key is pressed or a control clicked, such as Ctrl, by the names the screen-tools take them by.</summary>
public static class ModifierKeys
{
   static readonly Dictionary<string, RawInputModifiers> ByName = new(StringComparer.OrdinalIgnoreCase)
                                                                  {
                                                                     ["Ctrl"] = RawInputModifiers.Control,
                                                                     ["Control"] = RawInputModifiers.Control,
                                                                     ["Shift"] = RawInputModifiers.Shift,
                                                                     ["Alt"] = RawInputModifiers.Alt,
                                                                     ["Meta"] = RawInputModifiers.Meta,
                                                                     ["Win"] = RawInputModifiers.Meta
                                                                  };

   /// <summary>The modifier keys <paramref name="names"/> names, joined by <c>+</c>, such as <c>Ctrl+Shift</c>.</summary>
   public static RawInputModifiers Named(string names) => Named(names.Split('+', StringSplitOptions.TrimEntries));

   /// <summary>The modifier keys <paramref name="names"/> names, one each.</summary>
   public static RawInputModifiers Named(IEnumerable<string> names) =>
      names.Select(OneNamed).Aggregate(RawInputModifiers.None, (held, modifier) => held | modifier);

   static RawInputModifiers OneNamed(string name) =>
      ByName.TryGetValue(name, out var modifier)
         ? modifier
         : throw new ArgumentException($"\"{name}\" is no modifier key: they're Ctrl, Shift, Alt and Meta.", nameof(name));
}
