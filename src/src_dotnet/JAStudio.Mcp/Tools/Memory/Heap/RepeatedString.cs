namespace JAStudio.Mcp.Tools.Memory.Heap;

/// <summary>A string value the managed heap holds more than one string of.</summary>
/// <param name="Value">The value, or its first 100 characters if it's longer.</param>
/// <param name="Copies">How many strings of the value the heap holds.</param>
/// <param name="MegabytesInCopies">The megabytes all of them but one take, which sharing one string would save.</param>
public record RepeatedString(string Value, long Copies, double MegabytesInCopies)
{
   const int LongestValueShown = 100;

   /// <summary><paramref name="value"/> as <see cref="Value"/> shows it.</summary>
   public static string Shown(string value) => value.Length <= LongestValueShown ? value : value[..LongestValueShown] + "…";
}
