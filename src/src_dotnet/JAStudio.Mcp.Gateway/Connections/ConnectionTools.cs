using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Connections;

/// <summary>The gateway's own tools that list the running JAStudios, and connect to and disconnect from them (<see cref="ConnectedJAStudios"/>).</summary>
[McpServerToolType]
class ConnectionTools
{
   readonly ConnectedJAStudios _connectedJAStudios;

   public ConnectionTools(ConnectedJAStudios connectedJAStudios) => _connectedJAStudios = connectedJAStudios;

   [McpServerTool(Name = "list_the_running_jastudios", ReadOnly = true)]
   [Description("""
                Lists the JAStudios running on this computer, each by the name to connect to it by, such as anki-main: JAStudio inside Anki, the
                standalone JAStudio, and headless JAStudios, built from any source checkout. A name says what hosts the JAStudio and the branch of
                the source checkout it was started on. Says which are connected, and which connected ones don't run now.
                """)]
   public Task<string> ListTheRunningJAStudios(CancellationToken cancellationToken = default) => _connectedJAStudios.ReportOfTheRunningJAStudiosAsync(cancellationToken);

   [McpServerTool(Name = "connect_to_jastudio", Destructive = false, Idempotent = true)]
   [Description("""
                Connects to a running JAStudio and offers its tools here, under the connection's name: <name>__<tool>, such as anki-main__search_notes.
                Several can be connected at once, such as to compare how two branches parse the same sentences. A connection lasts through the
                JAStudio's restarts: while it doesn't run, its tools are withdrawn, and when it runs again, such as when Anki has restarted, they're
                offered again. A JAStudio's MCP server starts once its collection has loaded, and only then does the JAStudio run, as listed.
                """)]
   public Task<string> ConnectToJAStudio(
      [Description("""
                   The name of the JAStudio, as list_the_running_jastudios gives it. By default the JAStudio working on this source checkout's
                   corpus checkout, src/jas_database: in the main checkout, JAStudio inside Anki or the standalone JAStudio; in a linked worktree,
                   the worktree's standalone JAStudio, or the headless JAStudio started there.
                   """)] string? name = null,
      CancellationToken cancellationToken = default) =>
      _connectedJAStudios.ConnectAsync(name, cancellationToken);

   [McpServerTool(Name = "disconnect_from_jastudio", Destructive = false, Idempotent = false)]
   [Description("Ends a connection to a JAStudio, withdrawing its tools. The JAStudio runs on.")]
   public Task<string> DisconnectFromJAStudio([Description("The connection's name, such as anki-main.")] string name) => _connectedJAStudios.DisconnectAsync(name);
}
