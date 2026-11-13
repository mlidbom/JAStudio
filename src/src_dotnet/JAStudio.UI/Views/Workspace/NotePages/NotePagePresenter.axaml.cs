using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using JAStudio.UI.ViewModels.Workspace;

namespace JAStudio.UI.Views.Workspace.NotePages;

/// <summary>
/// Where a workspace tab's view (<see cref="NoteTabView"/>, <see cref="NoteTabImageView"/>) shows the shown note's page in Avalonia<br/>
/// (<see cref="NoteTabViewModel.NotePage"/>), scrolled: a sentence's, for now. The view for each kind of page is the application's data template for it.
/// </summary>
/// <remarks>
/// A page is made anew when its note or a setting changes. When the page had the keyboard, the new one gets it, so the keys pressed on the page, such<br/>
/// as the note view shortcuts, go on working. A page of another note starts at the top.
/// </remarks>
/// <remarks>The keys scroll the page as they scroll a browser's: the arrows up and down, Home and End, besides the scroll viewer's own Page Up and Page Down.</remarks>
public partial class NotePagePresenter : UserControl
{
   bool _thePageHadTheKeyboard;

   public NotePagePresenter()
   {
      InitializeComponent();
      Scroller.KeyDown += (_, keyPressed) => ScrollAsABrowserDoes(keyPressed);
   }

   /// <summary>Follows <paramref name="tab"/>'s pages: scrolls a page of another note to the top, and keeps the keyboard on a page made anew.</summary>
   internal void Present(NoteTabViewModel tab)
   {
      tab.ShowsAnotherNote += () => Scroller.Offset = default;
      tab.PropertyChanging += (_, changing) =>
      {
         if(changing.PropertyName == nameof(NoteTabViewModel.NotePage)) _thePageHadTheKeyboard = HasTheKeyboard();
      };
      tab.PropertyChanged += (_, changed) =>
      {
         if(changed.PropertyName == nameof(NoteTabViewModel.NotePage) && _thePageHadTheKeyboard) FocusThePage();
      };
   }

   /// <summary>Gives the keyboard to the page, posted, so that a page just made is in place first.</summary>
   public void FocusThePage() => Dispatcher.UIThread.Post(() => Page.Presenter?.Child?.Focus(), DispatcherPriority.Background);

   void ScrollAsABrowserDoes(KeyEventArgs keyPressed)
   {
      if(keyPressed.Handled || keyPressed.KeyModifiers != KeyModifiers.None) return;

      switch(keyPressed.Key)
      {
         case Key.Up:
            Scroller.LineUp();
            break;
         case Key.Down:
            Scroller.LineDown();
            break;
         case Key.Home:
            Scroller.ScrollToHome();
            break;
         case Key.End:
            Scroller.ScrollToEnd();
            break;
         default:
            return;
      }

      keyPressed.Handled = true;
   }

   bool HasTheKeyboard() =>
      TopLevel.GetTopLevel(this)?.FocusManager.GetFocusedElement() is Visual focused && (focused == this || this.IsVisualAncestorOf(focused));
}
