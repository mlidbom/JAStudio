using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
using JAStudio.UI.Controls.Markup;
using JAStudio.UI.Menus;
using JAStudio.UI.ViewModels.Workspace.NotePages;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

namespace JAStudio.UI.Views.Workspace.NotePages;

/// <summary>
/// A sentence's page in a workspace tab (<see cref="SentencePageViewModel"/>), in Avalonia, as the sentence card's answer side shows it. Clicking a link<br/>
/// shows its note in the tab, and middle-clicking it opens the note in a new tab in the background. Right-clicking an element of the page, such as a<br/>
/// breakdown entry, gives the element's menu (<see cref="INotePageElement"/>), and right-clicking elsewhere the sentence's note menu, for the text<br/>
/// right-clicked. Keys pressed on the page run the note view shortcuts.
/// </summary>
/// <remarks>This code-behind only turns pointer and key events into calls on the view model, and opens the menu.</remarks>
public partial class SentencePageView : UserControl
{
   public SentencePageView()
   {
      InitializeComponent();
      KeyDown += (_, keyPressed) => RunTheNoteViewShortcutOf(keyPressed);
      ContextRequested += ShowTheRightClickMenu;
   }

   SentencePageViewModel Page => DataContext as SentencePageViewModel ?? throw new InvalidOperationException("A sentence's page shows no sentence page view model.");

   void RunTheNoteViewShortcutOf(KeyEventArgs keyPressed)
   {
      if(NoteViewKeyGesture.Of(keyPressed) is {} keyGesture) Page.KeyPressed(keyGesture);
   }

   /// <summary>A click on a link shows its note in the tab; a middle-click opens it in a new tab in the background.</summary>
   void OnNoteLinkReleased(object? sender, PointerReleasedEventArgs released)
   {
      if(sender is not Control { Tag: NoteLinkViewModel link }) return;

      switch(released.InitialPressMouseButton)
      {
         case MouseButton.Left:
            Page.Follow(link);
            break;
         case MouseButton.Middle:
            Page.OpenInABackgroundTab(link);
            break;
         default:
            return;
      }

      released.Handled = true;
   }

   /// <summary>
   /// Opens the menu where the page was right-clicked, or at its top left from the menu key: the menu of the element right-clicked, such as a<br/>
   /// breakdown entry, for the text selected on it, if any; or else the sentence's note menu, for the text right-clicked and the clipboard's text.
   /// </summary>
   async void ShowTheRightClickMenu(object? sender, ContextRequestedEventArgs requested)
   {
      requested.Handled = true;
      var menuTopLeft = this.PointToScreen(requested.TryGetPosition(this, out var pointer) ? pointer : new Point(0, 0));
      var menu = ElementRightClicked(requested.Source) is {} element
                    ? Page.MenuOf(element, TextSelectedIn(requested.Source))
                    : Page.NoteContextMenuSpec(TextRightClicked(requested.Source), await TheClipboardsText());
      PopupMenuWindow.ShowAt(menuTopLeft, menu);
   }

   /// <summary>The element of the page <paramref name="source"/>, the control right-clicked, shows, or the nearest one around it shows, if any.</summary>
   static INotePageElement? ElementRightClicked(object? source) =>
      TheControlRightClickedAndThoseAroundIt(source).Select(it => it.DataContext).OfType<INotePageElement>().FirstOrDefault();

   /// <summary>The text selected in the text right-clicked, or none.</summary>
   static string TextSelectedIn(object? source) => TextBlockRightClicked(source) is SelectableTextBlock selectable ? selectable.SelectedText : string.Empty;

   /// <summary>The text right-clicked: the text selected there, or else the whole text, as the card selects it.</summary>
   static string TextRightClicked(object? source) =>
      TextBlockRightClicked(source) switch
      {
         SelectableTextBlock { SelectedText.Length: > 0 } selectable => selectable.SelectedText,
         StyledTextBlock styled => styled.PlainText,
         { } text => text.Text ?? string.Empty,
         null => string.Empty
      };

   static TextBlock? TextBlockRightClicked(object? source) => TheControlRightClickedAndThoseAroundIt(source).OfType<TextBlock>().FirstOrDefault();

   /// <summary><paramref name="source"/>, the control right-clicked, then each control around it, the nearest first.</summary>
   static IEnumerable<Visual> TheControlRightClickedAndThoseAroundIt(object? source) => (source as Visual)?.GetSelfAndVisualAncestors() ?? [];

   async Task<string> TheClipboardsText() =>
      await (TopLevel.GetTopLevel(this)?.Clipboard ?? throw new InvalidOperationException("The page is shown in no window with a clipboard.")).TryGetTextAsync() ?? string.Empty;
}
