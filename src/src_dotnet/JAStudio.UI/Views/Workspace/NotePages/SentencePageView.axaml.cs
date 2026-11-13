using System;
using System.Linq;
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
/// shows its note in the tab, and middle-clicking it opens the note in a new tab in the background. Right-clicking gives the sentence's note menu, for<br/>
/// the text right-clicked, and keys pressed on the page run the note view shortcuts.
/// </summary>
/// <remarks>This code-behind only turns pointer and key events into calls on the view model, and opens the menu.</remarks>
public partial class SentencePageView : UserControl
{
   public SentencePageView()
   {
      InitializeComponent();
      KeyDown += (_, keyPressed) => RunTheNoteViewShortcutOf(keyPressed);
      ContextRequested += ShowTheNoteMenu;
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
   /// Opens the sentence's note menu where the page was right-clicked, or at its top left from the menu key, for the text right-clicked: the text<br/>
   /// selected there, or else the whole text, such as a word of the breakdown, as the card selects it.
   /// </summary>
   async void ShowTheNoteMenu(object? sender, ContextRequestedEventArgs requested)
   {
      requested.Handled = true;
      var menuTopLeft = this.PointToScreen(requested.TryGetPosition(this, out var pointer) ? pointer : new Point(0, 0));
      var selection = TextRightClicked(requested.Source);
      var clipboard = await (TopLevel.GetTopLevel(this)?.Clipboard ?? throw new InvalidOperationException("The page is shown in no window with a clipboard.")).TryGetTextAsync() ?? string.Empty;
      PopupMenuWindow.ShowAt(menuTopLeft, Page.NoteContextMenuSpec(selection, clipboard));
   }

   static string TextRightClicked(object? source) =>
      (source as Visual)?.GetSelfAndVisualAncestors().OfType<TextBlock>().FirstOrDefault() switch
      {
         SelectableTextBlock { SelectedText.Length: > 0 } selectable => selectable.SelectedText,
         StyledTextBlock styled => styled.PlainText,
         { } text => text.Text ?? string.Empty,
         null => string.Empty
      };
}
