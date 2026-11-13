namespace JAStudio.Mcp.Discovery;

/// <summary>What a running JAStudio runs in: Anki, which loads it as an add-on, or one of JAStudio's own hosts, which run it without Anki.</summary>
public enum JAStudioHost
{
   /// <summary>Anki, which loads JAStudio as its add-on.</summary>
   Anki,

   /// <summary>The standalone JAStudio, <c>JAStudio.StandaloneHost</c>: JAStudio with its workspace window, as inside Anki, but with Anki out of the loop.</summary>
   Standalone,

   /// <summary>The headless JAStudio, <c>JAStudio.HeadlessHost</c>, which Claude's MCP gateway starts: JAStudio without Anki or a screen.</summary>
   Headless
}
