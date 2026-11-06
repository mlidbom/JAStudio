using JAStudio.Core.Specifications.Fixtures;
using Xunit;

// As in JAStudio.Core.Specifications: parallel, and the dictionary is shut down once every specification has run.
// The only process-wide state these specifications set is CardServerUrl.BaseUrl, always to the same address.
[assembly: CollectionBehavior(DisableTestParallelization = false)]
[assembly: AssemblyFixture(typeof(PythonServicesCleanupFixture))]
