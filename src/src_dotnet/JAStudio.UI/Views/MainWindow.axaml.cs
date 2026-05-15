using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using JAStudio.Core;
using JAStudio.UI.Menus;

namespace JAStudio.UI.Views;

partial class MainWindow : Window
{
   static MainWindow? _instance;

   [Obsolete("For XAML designer/previewer only")]
   public MainWindow() { InitializeComponent(); }

   MainWindow(TemporaryServiceCollection services)
   {
      InitializeComponent();
      var mainMenu = this.FindControl<Menu>("MainMenu")!;
      var menuSpec = new JapaneseMainMenu(services).BuildMenuSpec(GetClipboardText);
      foreach(var item in SpecMenuRenderer.BuildMenuItems(menuSpec))
         mainMenu.Items.Add(item);
   }

   string GetClipboardText() =>
      Task.Run(async () =>
      {
         var data = await (Clipboard?.TryGetDataAsync() ?? Task.FromResult<IAsyncDataTransfer?>(null)).ConfigureAwait(false);
         if(data == null) return "";
         using(data)
         {
            foreach(var item in data.Items)
            {
               var text = await item.TryGetTextAsync().ConfigureAwait(false);
               if(text != null) return text;
            }
         }
         return "";
      }).GetAwaiter().GetResult();

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

   public static void ToggleVisibility(TemporaryServiceCollection services)
   {
      if(_instance == null) CreateAndRegister(services);
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
