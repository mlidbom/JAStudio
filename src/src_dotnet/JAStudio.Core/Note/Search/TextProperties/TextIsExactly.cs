using System;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>A text matches when the whole of it is <see cref="Text"/>, ignoring case.</summary>
public sealed record TextIsExactly : TextMatch
{
   internal TextIsExactly(string text) => Text = text;

   /// <summary>The text the whole property text must be.</summary>
   public string Text { get; }

   internal override bool IsMatchedBy(string text) => text.Equals(Text, StringComparison.OrdinalIgnoreCase);

   internal override string Description => $"is \"{Text}\"";
}
