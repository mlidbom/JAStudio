using System.Collections.Concurrent;
using System.Collections.Generic;
using Compze.Tessaging.TessageTypes;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// Every tevent that the test harness's JAStudio published, in the order an observer got them, once the unit of work that published each had<br/>
/// committed: what a specification asserts on to see what JAStudio tells those who observe its tevents.
/// </summary>
/// <remarks>Observers run on threads of their own: a specification waits for the engine to have no tevents in flight before reading them.</remarks>
public sealed class TeventsObserved
{
   readonly ConcurrentQueue<ITevent> _observed = new();

   internal void Add(ITevent tevent) => _observed.Enqueue(tevent);

   /// <summary>The tevents observed so far, in the order they were observed.</summary>
   public IReadOnlyList<ITevent> SoFar() => [.. _observed];
}
