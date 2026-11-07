using System;
using System.Globalization;

namespace JAStudio.UI.Utils;

/// <summary>
/// A text that sorts with the numbers in it compared as numbers, so <c>episode_2</c> comes before <c>episode_11</c>, ignoring case.<br/>
/// For texts such as a sentence's external id or its source tag, where the numbers are episodes, seasons and times.
/// </summary>
/// <remarks>
/// A grid column bound to it sorts it this way, since the grid compares values that aren't strings as <see cref="IComparable"/>.<br/>
/// It shows as its <see cref="Text"/>.
/// </remarks>
sealed record TextSortedWithNumbersAsNumbers(string Text) : IComparable<TextSortedWithNumbersAsNumbers>, IComparable
{
   static readonly StringComparer NumbersAsNumbers = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.NumericOrdering | CompareOptions.IgnoreCase);

   public int CompareTo(TextSortedWithNumbersAsNumbers? other) => other is null ? 1 : NumbersAsNumbers.Compare(Text, other.Text);

   public int CompareTo(object? other) => other switch
   {
      null => 1,
      TextSortedWithNumbersAsNumbers text => CompareTo(text),
      _ => throw new ArgumentException($"A {nameof(TextSortedWithNumbersAsNumbers)} can't be compared with a {other.GetType().Name}.", nameof(other))
   };

   public override string ToString() => Text;
}
