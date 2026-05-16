using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core;
using JAStudio.UI.Menus;

namespace JAStudio.UI.Views;

partial class MainWindow : Window
{
   static MainWindow? _instance;

   [Obsolete("For XAML designer/previewer only")]
   public MainWindow()
   {
      InitializeComponent();
   }

   MainWindow(TemporaryServiceCollection services)
   {
      var services1 = services;
      InitializeComponent();
      KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+Shift+O"), Command = new RelayCommand(Hide) });
      KeyBindings.Add(new KeyBinding { Gesture = KeyGesture.Parse("Ctrl+O"), Command = new RelayCommand(() => NoteSearchDialog.ToggleVisibility(services1)) });
      var mainMenu = this.FindControl<Menu>("MainMenu")!;
      var menuSpec = new JapaneseMainMenu(services).BuildMenuSpec(GetClipboardText);
      foreach(var item in SpecMenuRenderer.BuildMenuItems(menuSpec))
         mainMenu.Items.Add(item);
   }

   string GetClipboardText() => Clipboard?.TryGetTextAsync().Result ?? string.Empty;

   void InitializeComponent() => AvaloniaXamlLoader.Load(this);

   protected override void OnClosing(WindowClosingEventArgs e)
   {
      base.OnClosing(e);
      e.Cancel = true;
      Hide();
   }

   internal static void CreateAndRegister(TemporaryServiceCollection services)
   {
      _instance = new MainWindow(services);
      if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
         desktop.MainWindow = _instance;
   }

   public static void ToggleVisibility()
   {
      if(_instance!.IsVisible)
      {
         _instance.Hide();
      } else
      {
         _instance.Show();
         _instance.Activate();
      }
   }
}
