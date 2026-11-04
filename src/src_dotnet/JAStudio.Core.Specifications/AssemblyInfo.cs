using JAStudio.Core.Specifications.Fixtures;
using Xunit;

// Enable parallel test execution — no shared static state
[assembly: CollectionBehavior(DisableTestParallelization = false)]
[assembly: AssemblyFixture(typeof(PythonServicesCleanupFixture))]
