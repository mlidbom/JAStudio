using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

public partial class EnglishWordSearchDialog : Window
{
   static EnglishWordSearchDialog? _instance;

   /// <summary>
   /// Get the singleton instance of the dialog.
   /// </summary>
   public static EnglishWordSearchDialog Instance
   {
      get
      {
         if(_instance == null)
         {
            _instance = new EnglishWordSearchDialog();
         }

         return _instance;
      }
   }

   public EnglishWordSearchDialog()
   {
      InitializeComponent();
      DataContext = new EnglishWordSearchDialogViewModel();

      // Focus search input when dialog is shown
      Opened += (_, _) =>
      {
         var searchInput = this.FindControl<TextBox>("SearchInput");
         searchInput?.Focus();
      };
   }

   protected override void OnClosing(WindowClosingEventArgs e)
   {
      base.OnClosing(e);
      e.Cancel = true;
      Hide();
   }

   void InitializeComponent()
   {
      AvaloniaXamlLoader.Load(this);
   }

   void OnResultDoubleClick(object? sender, TappedEventArgs e)
   {
      var viewModel = DataContext as EnglishWordSearchDialogViewModel;

      // Check for modifier keys
      var modifiers = e.KeyModifiers;

      if(modifiers.HasFlag(KeyModifiers.Control))
      {
         viewModel?.OpenInMerriamWebster();
      } else if(modifiers.HasFlag(KeyModifiers.Shift))
      {
         viewModel?.OpenInGoogle();
      } else
      {
         viewModel?.OpenInOED();
      }
   }

   /// <summary>
   /// Toggle the visibility of the dialog (show if hidden, hide if shown).
   /// </summary>
   public static void ToggleVisibility()
   {
      if(Instance.IsVisible)
      {
         Instance.Hide();
      } else
      {
         WindowPositioner.RepositionNearCursor(Instance);
         Instance.Show();
         Instance.Activate();
         var searchInput = Instance.FindControl<TextBox>("SearchInput");
         searchInput?.Focus();
      }
   }
}
