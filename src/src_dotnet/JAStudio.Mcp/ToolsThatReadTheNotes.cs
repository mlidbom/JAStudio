using System.Linq;
using JAStudio.Core.Note.Collection;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp;

/// <summary>
/// Keeps the tools that read the collection's notes from answering while it has none to use yet: they answer with why instead<br/>
/// (<see cref="JPCollection.WhyItHasNoNotesToUseYet"/>), that JAStudio hasn't loaded the collection yet, or that its load failed and how. A tool<br/>
/// marked <see cref="WorksWithoutTheNotesAttribute"/> runs anyway.
/// </summary>
/// <remarks>
/// A live JAStudio's server starts before its collection has loaded, so that the tools that diagnose a load that fails are there<br/>
/// (<see cref="Live.LiveJAStudioMcpServer"/>). The others would otherwise answer from a collection still loading, or from none, as if it had no notes.
/// </remarks>
static class ToolsThatReadTheNotes
{
   public static McpRequestFilter<CallToolRequestParams, CallToolResult> AnswerOnceThereAreNotesToUseIn(JPCollection collection) =>
      callTheTool => async (request, cancellationToken) =>
         collection.WhyItHasNoNotesToUseYet is {} why && !WorksWithoutTheNotes(request.MatchedPrimitive)
            ? new CallToolResult { IsError = true, Content = [new TextContentBlock { Text = why }] }
            : await callTheTool(request, cancellationToken);

   static bool WorksWithoutTheNotes(IMcpServerPrimitive? tool) => tool != null && tool.Metadata.OfType<WorksWithoutTheNotesAttribute>().Any();
}
