using System;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.Mcp.Gateway;

/// <summary>
/// Runs asynchronous operations one at a time, each after the one before it has finished, in the order they were asked for: starting and stopping<br/>
/// the headless JAStudio, or connecting to the live one and ending the connection, which Claude Code and Anki's exit may ask for at the same time,<br/>
/// and which take long enough, a build and a load or a connection, to overlap if run at once.
/// </summary>
/// <remarks>It holds no lock: each operation waits for the task of the one asked for before it, which it swaps itself in for atomically.</remarks>
class OneAtATime
{
   Task _theOperationAskedForLast = Task.CompletedTask;

   public async Task RunAsync(Func<Task> operation) => await RunAsync(async () =>
   {
      await operation();
      return true;
   });

   public async Task<TResult> RunAsync<TResult>(Func<Task<TResult>> operation)
   {
      var thisOperationIsDone = new TaskCompletionSource();
      var theOperationBefore = Interlocked.Exchange(ref _theOperationAskedForLast, thisOperationIsDone.Task);
      try
      {
         await theOperationBefore;
         return await operation();
      }
      finally
      {
         thisOperationIsDone.SetResult();
      }
   }
}
