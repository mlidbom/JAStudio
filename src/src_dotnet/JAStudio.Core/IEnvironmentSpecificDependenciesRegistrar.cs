using Compze.DependencyInjection.Abstractions;
using Compze.Tessaging.TessageBus;

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
}
