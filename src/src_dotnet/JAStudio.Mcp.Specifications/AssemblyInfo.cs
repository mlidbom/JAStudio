using Xunit;

// Every specification here starts a JAStudioMcpServer on a collection, and the collection publishes process-wide state:
// the services in TemporaryServiceCollection.Instance.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
