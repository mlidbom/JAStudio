namespace JAStudio.Core.UI.Markup;

/// <summary>A run of a <see cref="StyledText"/>: text, which may hold line breaks, all of it with the same <see cref="TextStyle"/>.</summary>
public sealed record StyledRun(string Text, TextStyle Style);
