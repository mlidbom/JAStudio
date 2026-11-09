using JAStudio.Core.Specifications.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// Every specification runs in parallel with every other, those of one class and the rows of one theory too: no specification sets process-wide
// state, and the parsing cases (Each_parsing_case) only read the collection they share, as the threads of a full rebuild do. That took a run
// from 11.1 seconds to 6.2 (2026-11-09).
[assembly: Parallelization(Mode = ParallelMode.All)]
[assembly: AssemblyFixture(typeof(PythonServicesCleanupFixture))]
