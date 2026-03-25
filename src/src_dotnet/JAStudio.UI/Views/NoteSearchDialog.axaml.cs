using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using JAStudio.UI.Utils;
using JAStudio.UI.ViewModels;

namespace JAStudio.UI.Views;

partial class NoteSearchDialog : Window
{
   static NoteSearchDialog? _instance;

   [Obsolete("For XAML designer/previewer only")]
   public NoteSearchDialog() {}

   static NoteSearchDialog GetInstance(Core.TemporaryServiceCollection services)
   {
      _instance ??= new NoteSearchDialog(services);
      return _instance;
   }

   public NoteSearchDialog(Core.TemporaryServiceCollection services)
   {
      InitializeComponent();
      DataContext = new NoteSearchDialogViewModel(services);

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
      var viewModel = DataContext as NoteSearchDialogViewModel;
      viewModel?.OpenSelectedNote();
   }

   /// <summary>
   /// Toggle the visibility of the dialog (show if hidden, hide if shown).
   /// </summary>
   public static void ToggleVisibility(Core.TemporaryServiceCollection services)
   {
      var instance = GetInstance(services);
      if(instance.IsVisible)
      {
         instance.Hide();
      } else
      {
         WindowPositioner.RepositionNearCursor(instance);
         instance.Show();
         instance.Activate();
         var searchInput = instance.FindControl<TextBox>("SearchInput");
         searchInput?.Focus();
      }
   }
}
