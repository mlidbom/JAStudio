using System;
using System.Net;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// A call JAStudio made to Anki over HTTP failed: the function failed in Anki, in which case <see cref="Error"/> is Python's traceback, or the<br/>
/// request wasn't one the Anki add-on serves, such as a function it has none of, arguments that don't fit, or a request without its token.
/// </summary>
public sealed class CallToAnkiFailedException(string whatWasCalled, HttpStatusCode status, string error)
   : Exception($"{whatWasCalled} failed in Anki, which answered {(int)status} {status}:{Environment.NewLine}{error}")
{
   /// <summary>The HTTP status Anki answered with: 500 when the function failed, 400, 401 or 404 when the request wasn't one it serves.</summary>
   public HttpStatusCode Status { get; } = status;

   /// <summary>What Anki said went wrong.</summary>
   public string Error { get; } = error;
}
