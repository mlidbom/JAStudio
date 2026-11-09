using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// Every specification runs in parallel with every other, as in JAStudio.Core.Specifications, apart from those using the headless UI the
// specifications share (SpecificationsUsingTheSharedHeadlessUI). That took a run from 15 seconds to about 4 (2026-11-09).
[assembly: Parallelization(Mode = ParallelMode.All)]
