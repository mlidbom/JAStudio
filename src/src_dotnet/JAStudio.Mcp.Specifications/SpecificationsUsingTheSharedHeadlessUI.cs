using Xunit;

namespace JAStudio.Mcp.Specifications;

/// <summary>
/// The specifications using the headless UI the specifications share (<see cref="HeadlessUIOfTheSpecifications"/>), which xUnit runs one at a time<br/>
/// and never alongside any other specification, while the others run in parallel.
/// </summary>
/// <remarks>
/// They read what the shared headless UI recorded last, such as the last web page it was asked for, which another of them running alongside could<br/>
/// have changed.
/// </remarks>
[CollectionDefinition(DisableParallelization = true)]
public class SpecificationsUsingTheSharedHeadlessUI;
