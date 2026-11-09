using JAStudio.Core.Specifications.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// The specification classes run in parallel, and the dictionary is shut down once every specification has run. Not every specification in
// parallel with every other, as in JAStudio.Core.Specifications: the windows of every specification share the one headless UI thread, which runs
// what each specification sends it in turn, so that gained nothing (2026-11-09).
[assembly: Parallelization(Mode = ParallelMode.Collections)]
[assembly: AssemblyFixture(typeof(PythonServicesCleanupFixture))]
