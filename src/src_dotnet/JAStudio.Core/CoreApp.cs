using System;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core;

public class CoreApp : IDisposable
{
   public TemporaryServiceCollection Services { get; }
   public IEnvironmentPaths Paths { get; }

   internal CoreApp(TemporaryServiceCollection services, IEnvironmentPaths paths)
   {
      Services = services;
      Paths = paths;
   }

   public static bool IsTesting => TestEnvDetector.IsTesting;

   public void Dispose() => (Paths as IDisposable)?.Dispose();

   public JapaneseConfig Config => Services.ConfigurationStore.Config();
   public JPCollection Collection => Services.Resolve<JPCollection>();
}
