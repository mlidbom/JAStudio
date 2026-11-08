using JAStudio.Core.Specifications.Fixtures;
using Xunit;

// As in JAStudio.Core.Specifications: parallel, and the dictionary is shut down once every specification has run.
// The windows of every specification share the one headless UI thread, which runs what each specification sends it in turn.
[assembly: CollectionBehavior(DisableTestParallelization = false)]
[assembly: AssemblyFixture(typeof(PythonServicesCleanupFixture))]
