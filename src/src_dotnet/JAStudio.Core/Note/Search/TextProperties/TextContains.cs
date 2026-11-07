using System;

namespace JAStudio.Core.Note.Search.TextProperties;

/// <summary>A text matches when it contains <see cref="Text"/>, ignoring case.</summary>
public sealed record TextContains : TextMatch
{
   internal TextContains(string text) => Text = text;

   /// <summary>The text the property text must contain.</summary>
   public string Text { get; }

   internal override bool IsMatchedBy(string text) => text.Contains(Text, StringComparison.OrdinalIgnoreCase);

   internal override string Description => $"contains \"{Text}\"";
}
