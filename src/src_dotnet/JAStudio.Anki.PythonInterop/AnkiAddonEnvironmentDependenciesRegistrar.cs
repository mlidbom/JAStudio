using System;
using Compze.Utilities.DependencyInjection;
using Compze.Utilities.DependencyInjection.Abstractions;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Storage;
using JAStudio.Core.TaskRunners;
using JAStudio.UI.Utils;

namespace JAStudio.Anki.PythonInterop;

class AnkiAddonEnvironmentDependenciesRegistrar(string configJson, Action<string> configUpdateCallback) : IEnvironmentSpecificDependenciesRegistrar
{
   readonly string _configJson = configJson;
   readonly Action<string> _configUpdateCallback = configUpdateCallback;

   public void WireEnvironmentSpecificServices(IComponentRegistrar registrar)
   {
      registrar.Register(
         Singleton.For<IEnvironmentPaths>().Instance(new AnkiEnvironmentPaths()),
         Singleton.For<INoteRepository>().CreatedBy((NoteSerializer serializer, IEnvironmentPaths paths) => (INoteRepository)new FileSystemNoteRepository(serializer, paths)),
         Singleton.For<IBackendNoteCreator>().Instance(new AnkiBackendNoteCreator()),
         Singleton.For<IBackendDataLoader>().Instance(new AnkiBackendDataLoader()),
         Singleton.For<IFatalErrorHandler>().Instance(new AvaloniaFatalErrorHandler()),
         Singleton.For<IUIThreadDispatcher>().Instance(new AvaloniaUIThreadDispatcher()),
         Singleton.For<IConfigDictSource>().Instance(new AnkiConfigDictSource(_configJson, _configUpdateCallback)),
         Singleton.For<IReadingsMappingsSource>().CreatedBy((IEnvironmentPaths paths) => new FileReadingsMappingsSource(paths)),
         Singleton.For<ICardOperations>().CreatedBy((ExternalNoteIdMap idMap) => (ICardOperations)new AnkiCardOperationsImpl(idMap))
      );
   }
}
