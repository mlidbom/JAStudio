using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// Every specification here starts a CardServer, and CardServers publish process-wide state:
// CardServerUrl.BaseUrl and the services in TemporaryServiceCollection.Instance.
[assembly: Parallelization(Mode = ParallelMode.None)]
