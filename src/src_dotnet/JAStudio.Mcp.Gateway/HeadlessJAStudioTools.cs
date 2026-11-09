using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Gateway;

/// <summary>The gateway's own tools, which start and stop the headless JAStudio.</summary>
[McpServerToolType]
class HeadlessJAStudioTools
{
   readonly HeadlessJAStudio _headlessJAStudio;

   public HeadlessJAStudioTools(HeadlessJAStudio headlessJAStudio) => _headlessJAStudio = headlessJAStudio;

   [McpServerTool(Name = "start_headless_jastudio", Destructive = false)]
   [Description("""
                Builds the headless JAStudio, JAStudio without Anki or a UI, from this source checkout and starts it, stopping the one running first,
                so that it runs the checkout's code as it is: call it again after changing JAStudio's code. Once it has loaded the collection, its
                tools, such as search_notes, are offered here too, and they work on the corpus checkout it was started on. A build error fails it
                with the build's output. It takes about 10 seconds, and about 20 the first time on a corpus checkout, which builds its snapshots.
                """)]
   public Task<string> StartHeadlessJAStudio(
      [Description("""
                   The corpus checkout to work on. By default the headless JAStudio's own git worktree of the corpus repository, which is created
                   at the corpus repository's checked out commit the first time. Never the checkout JAStudio inside Anki works on, src/jas_database,
                   while Anki runs.
                   """)] string? corpusFolder = null,
      [Description("The Anki profile whose collection the notes' studying statuses are read from. By default the profile used last.")] string? ankiProfile = null,
      CancellationToken cancellationToken = default) =>
      _headlessJAStudio.StartAsync(corpusFolder, ankiProfile, cancellationToken);

   [McpServerTool(Name = "stop_headless_jastudio", Destructive = false, Idempotent = true)]
   [Description("Stops the headless JAStudio, if it runs, and withdraws its tools.")]
   public Task<string> StopHeadlessJAStudio() => _headlessJAStudio.StopAsync();
}
