using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// Every specification here starts a JAStudioMcpServer on a collection, and the collection publishes process-wide state:
// the services in TemporaryServiceCollection.Instance.
[assembly: Parallelization(Mode = ParallelMode.None)]
