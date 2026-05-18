using Compze.DependencyInjection.Abstractions;

namespace JAStudio.Core;

/// <summary>
/// Registers environment-specific services into the DI container.
/// Implemented by the Anki addon runtime and by the test harness.
/// </summary>
public interface IEnvironmentSpecificDependenciesRegistrar
{
   void WireEnvironmentSpecificServices(IComponentRegistrar registrar);
}
