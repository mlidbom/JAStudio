using System;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core;

public class CoreApp : IDisposable
{
   /// <summary>This JAStudio's hold on its corpus checkout, which keeps every other JAStudio off it for as long as this one runs.</summary>
   readonly CorpusCheckoutLock _corpusCheckoutLock;

   public TemporaryServiceCollection Services { get; }
   public IEnvironmentPaths Paths { get; }

   internal CoreApp(TemporaryServiceCollection services, IEnvironmentPaths paths)
   {
      Services = services;
      Paths = paths;
      _corpusCheckoutLock = CorpusCheckoutLock.On(paths.DatabaseDir);
   }

   public static bool IsTesting => TestEnvDetector.IsTesting;

   public void Dispose()
   {
      _corpusCheckoutLock.Dispose();
      (Paths as IDisposable)?.Dispose();
   }

   public JapaneseConfig Config => Services.ConfigurationStore.Config();
   public JPCollection Collection => Services.Resolve<JPCollection>();
}
