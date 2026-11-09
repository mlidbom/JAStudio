using System.Collections.Generic;

namespace JAStudio.Mcp.Gateway;

/// <summary>A command the gateway runs as a child process, such as <c>git</c> or <c>dotnet</c>, with its arguments.</summary>
record CommandLine(string Command, IReadOnlyList<string> Arguments)
{
   /// <summary>The command line as it would be typed, for reporting it.</summary>
   public override string ToString() => $"{Command} {string.Join(' ', Arguments)}";
}
