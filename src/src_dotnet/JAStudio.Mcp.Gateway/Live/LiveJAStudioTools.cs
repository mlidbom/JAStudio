using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway.Live;

/// <summary>The gateway's own tool when it serves the live JAStudio, which connects to it.</summary>
[McpServerToolType]
class LiveJAStudioTools
{
   readonly LiveJAStudio _liveJAStudio;

   public LiveJAStudioTools(LiveJAStudio liveJAStudio) => _liveJAStudio = liveJAStudio;

   [McpServerTool(Name = "connect_to_live_jastudio", Destructive = false, Idempotent = true)]
   [Description("""
                Connects to the live JAStudio, the one inside Anki, and offers its tools here: the same tools as the headless JAStudio's, such as
                search_notes and fast_forward_the_corpus_checkout, working on the collection Anki has open and its corpus checkout. Its tools are
                withdrawn when Anki exits: call this again once Anki has restarted. Fails when JAStudio inside Anki doesn't run, or hasn't loaded its
                collection yet.
                """)]
   public Task<string> ConnectToLiveJAStudio(CancellationToken cancellationToken = default) => _liveJAStudio.ConnectAsync(cancellationToken);
}
