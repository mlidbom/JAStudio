using JAStudio.Core.Specifications.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// Enable parallel test execution — no shared static state
[assembly: Parallelization(Mode = ParallelMode.All)]
[assembly: AssemblyFixture(typeof(PythonServicesCleanupFixture))]
