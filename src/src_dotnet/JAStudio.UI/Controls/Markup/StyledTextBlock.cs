using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using JAStudio.Core.UI.Markup;

namespace JAStudio.UI.Controls.Markup;

/// <summary>
/// Shows a note's text, with its markup, as styled text (<see cref="StyledText"/>) in the colours of the cards' styles: <c>&lt;b&gt;</c> in the cards'<br/>
/// bold colour, a <c>&lt;kanji&gt;</c> on the kanji's background, and so on. The text can be selected and copied, as on a card.
/// </summary>
/// <remarks>The colours are the note pages' brushes in the application's resources, such as <c>NotePage.Bold</c>.</remarks>
public class StyledTextBlock : SelectableTextBlock
{
   public static readonly StyledProperty<string?> MarkupProperty = AvaloniaProperty.Register<StyledTextBlock, string?>(nameof(Markup));

   public static readonly StyledProperty<bool> ShowsBoldAsColourOnlyProperty = AvaloniaProperty.Register<StyledTextBlock, bool>(nameof(ShowsBoldAsColourOnly));

   /// <summary>The note's text, with its markup, such as <c>やっと&lt;b&gt;会えた&lt;/b&gt;。</c>.</summary>
   public string? Markup
   {
      get => GetValue(MarkupProperty);
      set => SetValue(MarkupProperty, value);
   }

   /// <summary>
   /// Whether <c>&lt;b&gt;</c> is shown in the bold colour only, at the text's own weight, as a sentence's question shows the word it's about, whose<br/>
   /// large text bold would make heavy.
   /// </summary>
   public bool ShowsBoldAsColourOnly
   {
      get => GetValue(ShowsBoldAsColourOnlyProperty);
      set => SetValue(ShowsBoldAsColourOnlyProperty, value);
   }

   /// <summary>The text shown, without its styles, such as for the menu of the text right-clicked.</summary>
   public string PlainText => Markup is {} markup ? StyledText.FromMarkup(markup).ToString() : string.Empty;

   /// <summary>Styled as a <see cref="SelectableTextBlock"/> is, by the application's theme.</summary>
   protected override Type StyleKeyOverride => typeof(SelectableTextBlock);

   protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
   {
      base.OnPropertyChanged(change);
      if(change.Property == MarkupProperty || change.Property == ShowsBoldAsColourOnlyProperty)
         ShowTheMarkup();
   }

   void ShowTheMarkup()
   {
      var inlines = new InlineCollection();
      if(Markup is {} markup)
         foreach(var run in StyledText.FromMarkup(markup).Runs)
            inlines.Add(InlineShowing(run));
      Inlines = inlines;
   }

   Run InlineShowing(StyledRun styled)
   {
      var style = styled.Style;
      var run = new Run(styled.Text);
      if(ForegroundOf(style) is {} foreground) run.Foreground = foreground;
      if(BackgroundOf(style) is {} background) run.Background = background;
      if(style.HasFlag(TextStyle.Bold) && !ShowsBoldAsColourOnly) run.FontWeight = FontWeight.Bold;
      if(style.HasFlag(TextStyle.Italic)) run.FontStyle = FontStyle.Italic;
      if(DecorationsOf(style) is {} decorations) run.TextDecorations = decorations;
      return run;
   }

   /// <summary>The colour of the text, by the style that colours it, as the cards' styles pick it; null for the text's own colour.</summary>
   static IBrush? ForegroundOf(TextStyle style) =>
      style.HasFlag(TextStyle.Bold) ? NotePageBrushes.Bold
      : style.HasFlag(TextStyle.Italic) ? NotePageBrushes.Italic
      : style.HasFlag(TextStyle.Underlined) ? NotePageBrushes.Underline
      : style.HasFlag(TextStyle.Japanese) || style.HasFlag(TextStyle.Label) ? NotePageBrushes.Green
      : style.HasFlag(TextStyle.ReadingsSeparator) ? NotePageBrushes.ReadingsSeparator
      : HasAHighlightBackground(style) ? NotePageBrushes.Text
      : null;

   static IBrush? BackgroundOf(TextStyle style) =>
      style.HasFlag(TextStyle.Kanji) ? NotePageBrushes.KanjiBackground
      : style.HasFlag(TextStyle.Radical) ? NotePageBrushes.RadicalBackground
      : style.HasFlag(TextStyle.Vocabulary) ? NotePageBrushes.VocabularyBackground
      : style.HasFlag(TextStyle.Reading) ? NotePageBrushes.ReadingBackground
      : null;

   static bool HasAHighlightBackground(TextStyle style) => BackgroundOf(style) is not null;

   static TextDecorationCollection? DecorationsOf(TextStyle style) =>
      style.HasFlag(TextStyle.StruckThrough) ? Avalonia.Media.TextDecorations.Strikethrough
      : style.HasFlag(TextStyle.Underlined) || style.HasFlag(TextStyle.Primary) ? Avalonia.Media.TextDecorations.Underline
      : null;
}
