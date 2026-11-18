using System;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core;

public class CoreApp : IDisposable
{
   /// <summary>This JAStudio's hold on its corpus checkout, which keeps every other JAStudio off it for as long as this one runs.</summary>
   readonly CorpusCheckoutLock _corpusCheckoutLock;
   readonly BackgroundTaskManager _backgroundTasks;

   public TemporaryServiceCollection Services { get; }
   public IEnvironmentPaths Paths { get; }

   internal CoreApp(TemporaryServiceCollection services, IEnvironmentPaths paths, BackgroundTaskManager backgroundTasks)
   {
      Services = services;
      Paths = paths;
      _backgroundTasks = backgroundTasks;
      _corpusCheckoutLock = CorpusCheckoutLock.On(paths.DatabaseDir);
   }

   public static bool IsTesting => TestEnvDetector.IsTesting;

   /// <summary>
   /// Lets go of the corpus checkout, once the background tasks have finished: one still running, such as a reload of the collection, needs the<br/>
   /// corpus checkout and the services the container disposes after this.
   /// </summary>
   public void Dispose()
   {
      _backgroundTasks.WaitUntilEveryTaskHasFinished();
      _corpusCheckoutLock.Dispose();
      (Paths as IDisposable)?.Dispose();
   }

   public JapaneseConfig Config => Services.ConfigurationStore.Config();
   public JPCollection Collection => Services.Resolve<JPCollection>();
}
