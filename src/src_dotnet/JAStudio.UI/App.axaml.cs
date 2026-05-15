using System;
using System.Threading;
using Avalonia;
using Avalonia.Markup.Xaml;
using JAStudio.Core;
using JAStudio.UI.Views;

namespace JAStudio.UI;

public class UIApp : Application
{
   static readonly ManualResetEventSlim Initialized = new();

   public override void Initialize() => AvaloniaXamlLoader.Load(this);

   public override void OnFrameworkInitializationCompleted()
   {
      base.OnFrameworkInitializationCompleted();
      Initialized.Set();
   }

   public static void WaitForInitialization(TimeSpan timeout)
   {
      if(!Initialized.Wait(timeout))
      {
         throw new TimeoutException(
            $"Avalonia did not initialize within {timeout.TotalSeconds}s.");
      }
   }

   public static void InitializeMainWindow(TemporaryServiceCollection services) =>
      MainWindow.CreateAndRegister(services);
}
