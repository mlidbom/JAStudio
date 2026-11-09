using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JAStudio.Core;

namespace JAStudio.Mcp.Corpus;

/// <summary>
/// The checkout of the corpus repository a JAStudio loaded its notes from and saves them to, as git sees it: which note files differ from the<br/>
/// checkout's <c>HEAD</c>, which is what a change to the notes, such as a full rebuild, changed, and moving the checkout to another commit.
/// </summary>
public class CorpusCheckout
{
   /// <summary>The folders of the corpus repository that hold the notes, one file per note, named by its id.</summary>
   static readonly string[] NoteFolders = ["kanji", "vocab", "sentences"];

   readonly DirectoryInfo _folder;

   public CorpusCheckout(IEnvironmentPaths paths) => _folder = new DirectoryInfo(paths.DatabaseDir);

   /// <summary>The commit the checkout's <c>HEAD</c> is at, by its hash.</summary>
   public CorpusCommit Head() => new(Git(["rev-parse", "HEAD"]).Trim());

   /// <summary>
   /// Moves the checkout, its branch and its files, forward to <paramref name="commit"/>, as <c>git merge --ff-only</c> does. Git refuses, and changes<br/>
   /// nothing, when <paramref name="commit"/> isn't ahead of <c>HEAD</c>, or when moving would overwrite changes not committed.
   /// </summary>
   public void FastForwardTo(CorpusCommit commit) => Git(["merge", "--ff-only", "--quiet", commit.Name]);

   /// <summary>
   /// The ids of the notes whose files differ from the checkout's <c>HEAD</c>: changed, added or deleted, staged or not. A note file is named by<br/>
   /// the note's id, such as <c>vocab/81/81020a41-d636-4ee9-a158-2443d8750482.json</c>.
   /// </summary>
   public IReadOnlyList<Guid> IdsOfTheNotesWhoseFilesDifferFromHead() => IdsOfTheNotesWithTheFiles(PathsOfTheFilesThatDifferFromHead());

   /// <summary>The ids of the notes whose files differ between the commits <paramref name="from"/> and <paramref name="to"/>: changed, added or deleted.</summary>
   public IReadOnlyList<Guid> IdsOfTheNotesWhoseFilesDifferBetween(CorpusCommit from, CorpusCommit to) =>
      IdsOfTheNotesWithTheFiles(PathsOfTheFilesThatDifferBetween(from, to));

   /// <summary>The ids of the notes whose files are among <paramref name="paths"/>, relative to the checkout: the paths in the note folders that are note files.</summary>
   static IReadOnlyList<Guid> IdsOfTheNotesWithTheFiles(IEnumerable<string> paths) =>
      [.. paths.Where(it => it.EndsWith(".json", StringComparison.Ordinal)).Select(it => Guid.Parse(Path.GetFileNameWithoutExtension(it)))];

   /// <summary>The paths, relative to the checkout, of the files in the note folders that git reports as differing from <c>HEAD</c>, untracked ones included.</summary>
   IEnumerable<string> PathsOfTheFilesThatDifferFromHead() =>
      PathsInTheStatusEntries(Git(["status", "--porcelain=v1", "-z", "--untracked-files=all", "--", .. NoteFolders]));

   /// <summary>
   /// The paths, relative to the checkout, of the files in the note folders that differ between the commits <paramref name="from"/> and<br/>
   /// <paramref name="to"/>: <c>git diff --name-only -z</c> lists each path ended by a NUL.
   /// </summary>
   string[] PathsOfTheFilesThatDifferBetween(CorpusCommit from, CorpusCommit to) =>
      Git(["diff", "--name-only", "--no-renames", "-z", from.Name, to.Name, "--", .. NoteFolders]).Split('\0', StringSplitOptions.RemoveEmptyEntries);

   /// <summary>
   /// The paths in the output of <c>git status --porcelain=v1 -z</c>: each entry is two status letters, a space and the path, ended by a NUL.<br/>
   /// A renamed or copied file's entry, status <c>R</c> or <c>C</c>, is followed by its old path, ended by a NUL too, which is no longer the note's file.
   /// </summary>
   static IEnumerable<string> PathsInTheStatusEntries(string status)
   {
      var fields = status.Split('\0', StringSplitOptions.RemoveEmptyEntries);
      for(var index = 0; index < fields.Length; index++)
      {
         var entry = fields[index];
         yield return entry[3..];
         if(entry[0] is 'R' or 'C') index++;
      }
   }

   /// <summary>Runs git with <paramref name="arguments"/> in the corpus checkout, and returns what it writes to its standard output.</summary>
   /// <remarks>
   /// Git runs with no console window. Inside Anki, which has no console, git would otherwise get a console of its own, which Windows can show<br/>
   /// as a terminal window for every git command. In the first fast-forward from inside Anki (2026-11-09), one git command was ended with<br/>
   /// <c>STATUS_CONTROL_C_EXIT</c>, the exit code of a process whose console closed, though the same command run by hand succeeded.
   /// </remarks>
   string Git(IReadOnlyList<string> arguments)
   {
      var startInfo = new ProcessStartInfo("git")
      {
         WorkingDirectory = _folder.FullName,
         UseShellExecute = false,
         CreateNoWindow = true,
         RedirectStandardInput = true,
         RedirectStandardOutput = true,
         RedirectStandardError = true,
         StandardOutputEncoding = Encoding.UTF8,
         StandardErrorEncoding = Encoding.UTF8
      };
      foreach(var argument in arguments) startInfo.ArgumentList.Add(argument);

      using var git = Process.Start(startInfo) ?? throw new InvalidOperationException("git started no process.");
      git.StandardInput.Close();
      var standardError = git.StandardError.ReadToEndAsync();
      var output = git.StandardOutput.ReadToEnd();
      git.WaitForExit();

      return git.ExitCode == 0
                ? output
                : throw new InvalidOperationException($"`git {string.Join(' ', arguments)}` in {_folder.FullName} failed with exit code {git.ExitCode}: {standardError.Result}");
   }
}
