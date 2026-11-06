using Xunit;

// Every specification here starts a CardServer, and CardServers publish process-wide state:
// CardServerUrl.BaseUrl and the services in TemporaryServiceCollection.Instance.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
