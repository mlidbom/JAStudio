using System;
using System.Threading;

namespace JAStudio.Core.Specifications.Fixtures;

/// <summary>
/// Sizes the thread pool for every specification running in parallel with every other, before any of them runs: a minimum of
/// <see cref="ThreadsPerCore"/> threads per core.
/// </summary>
/// <remarks>
/// Many specifications block a pool thread while they wait for work they started on other pool threads, such as loading the collection, whose parts<br/>
/// run in parallel (<see cref="JAStudio.Core.Note.Collection.JPCollection.ReloadFromBackend"/>). The pool starts with as many threads as the machine has cores,<br/>
/// and adds more only slowly, about one a second, so a run starved: it took 20 seconds rather than 7, and a specification that waits 10 seconds for<br/>
/// a run in the background failed (2026-11-10).
/// </remarks>
public sealed class ThreadPoolSizedForParallelSpecifications
{
   const int ThreadsPerCore = 8;

   public ThreadPoolSizedForParallelSpecifications()
   {
      var minimum = Environment.ProcessorCount * ThreadsPerCore;
      ThreadPool.SetMinThreads(minimum, minimum);
   }
}
