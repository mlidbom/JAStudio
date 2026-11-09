using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// Every specification here starts a CardServer, and a CardServer publishes its address to the whole process: CardServerUrl.BaseUrl.
[assembly: Parallelization(Mode = ParallelMode.None)]
