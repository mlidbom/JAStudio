using System;
using Avalonia;
using Avalonia.Media;

namespace JAStudio.UI.Controls.Markup;

/// <summary>
/// The note pages' colours, from the application's resources (<c>App.axaml</c>), for code that styles text, such as <see cref="StyledTextBlock"/>'s<br/>
/// runs. They're the cards' light colours, as the cards' stylesheets define them (<c>JAStudio.Web/Styles/_010_colors.scss</c>).
/// </summary>
static class NotePageBrushes
{
   public static IBrush Text => Find("NotePage.Text");
   public static IBrush Bold => Find("NotePage.Bold");
   public static IBrush Italic => Find("NotePage.Italic");
   public static IBrush Underline => Find("NotePage.Underline");
   public static IBrush Green => Find("NotePage.Green");
   public static IBrush ReadingsSeparator => Find("NotePage.ReadingsSeparator");
   public static IBrush KanjiBackground => Find("NotePage.KanjiBackground");
   public static IBrush RadicalBackground => Find("NotePage.RadicalBackground");
   public static IBrush VocabularyBackground => Find("NotePage.VocabularyBackground");
   public static IBrush ReadingBackground => Find("NotePage.ReadingBackground");

   static IBrush Find(string key) =>
      Application.Current is {} application && application.TryGetResource(key, null, out var resource) && resource is IBrush brush
         ? brush
         : throw new InvalidOperationException($"The application's resources have no brush {key}.");
}
