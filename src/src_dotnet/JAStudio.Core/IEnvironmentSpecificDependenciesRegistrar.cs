using Compze.DependencyInjection.Abstractions;
using Compze.Tessaging.TessageBus;
using Compze.Teventive.MemoryImageStateStore;
using JAStudio.Core.Storage;

namespace JAStudio.Core;

/// <summary>
/// Registers environment-specific services into the DI container.
/// Implemented by the Anki addon runtime and by the test harness.
/// </summary>
public interface IEnvironmentSpecificDependenciesRegistrar
{
   void WireEnvironmentSpecificServices(IComponentRegistrar registrar);

   /// <summary>
   /// Declares, through <paramref name="observe"/>, the observers of tevents that this environment adds to the app's own, such as the test harness's,<br/>
   /// which records every tevent for the specifications to assert on. An observer gets a tevent once the unit of work that published it has committed.
   /// </summary>
   void ObserveTevents(TeventObservationRegistrar observe) {}

   /// <summary>
   /// Declares where <paramref name="store"/>, the memory image state store that keeps the notes, keeps them: unless the environment says<br/>
   /// otherwise, in their files<br/>
   /// in the corpus checkout (<see cref="CorpusNoteFiles"/>).
   /// </summary>
   void DeclareWhereTheNotesAreKept(MemoryImageStateStoreBuilder store) => CorpusNoteFiles.DeclareIn(store);
}
