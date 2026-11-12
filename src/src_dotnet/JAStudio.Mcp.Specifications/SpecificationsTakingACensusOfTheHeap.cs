using Xunit;

namespace JAStudio.Mcp.Specifications;

/// <summary>
/// The specifications that take a census of the specifications' own process's heap, as describe_what_the_heap_holds does, which xUnit runs one<br/>
/// at a time and never alongside any other specification, while the others run in parallel.
/// </summary>
/// <remarks>
/// Alone, a census of the process takes about a second. While every other specification ran, it took over 100 seconds, past the client's<br/>
/// timeout, as it walked their objects with every core busy (2026-11-12).
/// </remarks>
[CollectionDefinition(DisableParallelization = true)]
public class SpecificationsTakingACensusOfTheHeap;
