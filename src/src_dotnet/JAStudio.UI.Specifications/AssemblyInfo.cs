using JAStudio.Core.Specifications.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// As in JAStudio.Core.Specifications: every specification in parallel with every other, and the dictionary is shut down once every specification
// has run. The only process-wide state these specifications set is CardServerUrl.BaseUrl, always to the same address.
[assembly: Parallelization(Mode = ParallelMode.All)]
[assembly: AssemblyFixture(typeof(PythonServicesCleanupFixture))]
