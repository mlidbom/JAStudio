using System;
using JAStudio.Core.TaskRunners;

namespace JAStudio.HeadlessHost;

/// <summary>
/// Ends the headless JAStudio on an exception nothing else handled, such as one thrown by a background task: with no UI to show it in, the<br/>
/// exception reaches the MCP gateway's log of the process, and the gateway reports that the headless JAStudio exited.
/// </summary>
class HeadlessFatalErrorHandler : IFatalErrorHandler
{
   public void Handle(Exception exception) => throw new InvalidOperationException("An exception nothing in the headless JAStudio handled.", exception);
}
