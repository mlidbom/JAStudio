using System;

namespace JAStudio.Core.UI.Web;

/// <summary>
/// Provides the base URL for the CardServer (Kestrel/Blazor).
/// Set by JAStudio.Web.CardServer on startup.
/// Used by renderers to inject iframe references.
/// </summary>
public static class CardServerUrl
{
   /// <summary>
   /// The base URL of the running CardServer, e.g. "http://127.0.0.1:54321".
   /// Null if the server has not been started.
   /// </summary>
   public static string? BaseUrl { get; set; }

   public static string MediaUrl(string fileName) => $"/media?name={Uri.EscapeDataString(fileName)}"; 
}
