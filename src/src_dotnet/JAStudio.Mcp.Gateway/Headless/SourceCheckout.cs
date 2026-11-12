using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.Mcp.Gateway.Headless;

/// <summary>
/// The checkout of JAStudio's source repository the gateway was started for, the folder with <c>JAStudio.slnx</c>: the headless JAStudio is built<br/>
/// from it and runs with its add-on folder, so a code change in the checkout reaches the headless JAStudio the next time it's started.
/// </summary>
class SourceCheckout
{
   SourceCheckout(DirectoryInfo folder) => Folder = folder;

   public DirectoryInfo Folder { get; }

   /// <summary>
   /// The checkout in <paramref name="folder"/>, which the script that starts the gateway, <c>start-the-gateway.ps1</c>, passes as <c>--source-checkout</c>:<br/>
   /// the checkout whose <c>.mcp.json</c> Claude Code started the gateway from.
   /// </summary>
   public static SourceCheckout In(string? folder)
   {
      var checkout = new DirectoryInfo(folder ?? throw new ArgumentException("The gateway needs --source-checkout <folder>: the JAStudio source checkout to work with."));
      return checkout.EnumerateFiles("JAStudio.slnx").Any()
                ? new SourceCheckout(checkout)
                : throw new ArgumentException($"--source-checkout {checkout.FullName} is no JAStudio source checkout: it has no JAStudio.slnx.");
   }

   /// <summary>The <c>src</c> folder, which Anki loads JAStudio from, with the user files.</summary>
   public DirectoryInfo AddonFolder => new(Path.Combine(Folder.FullName, "src"));

   /// <summary>
   /// The corpus repository's checkout in this source checkout, the submodule <c>src/jas_database</c>. In the main checkout it's the one JAStudio<br/>
   /// inside Anki works on; in a linked worktree, the worktree's own, which git keeps a clone of the corpus repository of its own for.
   /// </summary>
   public DirectoryInfo CorpusRepository => new(Path.Combine(AddonFolder.FullName, "jas_database"));

   /// <summary>The folder of the headless JAStudio's project, <see cref="HeadlessHostProject"/>, which its build output is in.</summary>
   public DirectoryInfo HeadlessHostProjectFolder => new(Path.Combine(AddonFolder.FullName, "src_dotnet", "JAStudio.HeadlessHost"));

   /// <summary>The project of the headless JAStudio.</summary>
   public FileInfo HeadlessHostProject => new(Path.Combine(HeadlessHostProjectFolder.FullName, "JAStudio.HeadlessHost.csproj"));

   /// <summary>
   /// The corpus checkout the headless JAStudio works on unless told otherwise. In a linked worktree of the source repository, the worktree's own<br/>
   /// <see cref="CorpusRepository"/>, at the commit its branch records: no JAStudio inside Anki works on it. In the main checkout, whose corpus<br/>
   /// checkout JAStudio inside Anki works on, a git worktree of the corpus repository of its own (<see cref="TheMainCheckoutsHeadlessCorpusWorktreeAsync"/>).
   /// </summary>
   public async Task<DirectoryInfo> DefaultHeadlessCorpusFolderAsync(CancellationToken cancellationToken) =>
      IsTheMainCheckout ? await TheMainCheckoutsHeadlessCorpusWorktreeAsync(cancellationToken) : TheLinkedWorktreesCorpusCheckout();

   /// <summary>
   /// Whether this is the source repository's main checkout, such as <c>C:\Dev\JAStudio</c>, rather than one of its linked worktrees: the main<br/>
   /// checkout's <c>.git</c> is a folder, and a linked worktree's a file naming the folder git keeps for it.
   /// </summary>
   bool IsTheMainCheckout => Directory.Exists(Path.Combine(Folder.FullName, ".git"));

   /// <summary>A linked worktree's <see cref="CorpusRepository"/>, which must be initialized: a submodule never initialized is an empty folder.</summary>
   DirectoryInfo TheLinkedWorktreesCorpusCheckout() =>
      Path.Exists(Path.Combine(CorpusRepository.FullName, ".git"))
         ? CorpusRepository
         : throw new InvalidOperationException($"This worktree's corpus checkout, {CorpusRepository.FullName}, isn't initialized: run 'git submodule update --init src/jas_database' in {Folder.FullName}, or start the headless JAStudio with a corpusFolder.");

   /// <summary>
   /// The main checkout's headless corpus checkout: a git worktree of the <see cref="CorpusRepository"/> of its own, so that the headless JAStudio<br/>
   /// never writes to the checkout JAStudio inside Anki works on. It sits with the source checkout's worktrees: for <c>C:\Dev\JAStudio</c>,<br/>
   /// <c>C:\Dev\JAStudio.worktrees\headless-corpus</c>.
   /// </summary>
   /// <remarks>
   /// It's created the first time it's needed, at the commit the corpus repository has checked out, and after that left as the work leaves it:<br/>
   /// moving it is a git operation like any other.
   /// </remarks>
   async Task<DirectoryInfo> TheMainCheckoutsHeadlessCorpusWorktreeAsync(CancellationToken cancellationToken)
   {
      var worktree = new DirectoryInfo(Path.Combine(FolderOfItsWorktrees.FullName, "headless-corpus"));
      if(!worktree.Exists)
      {
         await ChildProcesses.RunToCompletionAsync(new CommandLine("git", ["-C", CorpusRepository.FullName, "worktree", "add", "--detach", "--quiet", worktree.FullName, "HEAD"]),
                                                   Folder,
                                                   cancellationToken);
      }

      return worktree;
   }

   /// <summary>Where the worktrees of this source checkout are kept, next to it: for <c>C:\Dev\JAStudio</c>, <c>C:\Dev\JAStudio.worktrees</c>.</summary>
   DirectoryInfo FolderOfItsWorktrees => new(Path.Combine(FolderItIsIn.FullName, $"{Folder.Name}.worktrees"));

   DirectoryInfo FolderItIsIn =>
      Folder.Parent ?? throw new InvalidOperationException($"The source checkout {Folder.FullName} is a drive's root, with no folder to keep its worktrees next to it in.");
}
