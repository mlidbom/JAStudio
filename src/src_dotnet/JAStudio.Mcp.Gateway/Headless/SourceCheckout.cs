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

   /// <summary>The <c>src</c> folder, which Anki loads JAStudio from, with JAStudio's configuration (<c>meta.json</c>) and the user files.</summary>
   public DirectoryInfo AddonFolder => new(Path.Combine(Folder.FullName, "src"));

   /// <summary>The corpus repository's checkout in this source checkout, the submodule <c>src/jas_database</c>, which JAStudio inside Anki works on.</summary>
   public DirectoryInfo CorpusRepository => new(Path.Combine(AddonFolder.FullName, "jas_database"));

   /// <summary>The folder of the headless JAStudio's project, <see cref="HeadlessHostProject"/>, which its build output is in.</summary>
   public DirectoryInfo HeadlessHostProjectFolder => new(Path.Combine(AddonFolder.FullName, "src_dotnet", "JAStudio.HeadlessHost"));

   /// <summary>The project of the headless JAStudio.</summary>
   public FileInfo HeadlessHostProject => new(Path.Combine(HeadlessHostProjectFolder.FullName, "JAStudio.HeadlessHost.csproj"));

   /// <summary>
   /// The corpus checkout the headless JAStudio works on unless told otherwise: a git worktree of the <see cref="CorpusRepository"/> of its own, so<br/>
   /// that it never writes to the checkout JAStudio inside Anki works on. It sits with the source checkout's worktrees: for <c>C:\Dev\JAStudio</c>,<br/>
   /// <c>C:\Dev\JAStudio.worktrees\headless-corpus</c>.
   /// </summary>
   /// <remarks>
   /// It's created the first time it's needed, at the commit the corpus repository has checked out, and after that left as the work leaves it:<br/>
   /// moving it is a git operation like any other.
   /// </remarks>
   public async Task<DirectoryInfo> DefaultHeadlessCorpusWorktreeAsync(CancellationToken cancellationToken)
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
