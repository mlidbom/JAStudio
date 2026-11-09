using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core;

namespace JAStudio.Mcp.Corpus;

/// <summary>
/// The checkout of the corpus repository a JAStudio loaded its notes from and saves them to, as git sees it: which note files differ from the<br/>
/// checkout's <c>HEAD</c>, which is what a change to the notes, such as a full rebuild, changed, the versions of those files, and moving the<br/>
/// checkout to another commit.
/// </summary>
public class CorpusCheckout
{
   /// <summary>The folders of the corpus repository that hold the notes, one file per note, named by its id.</summary>
   static readonly string[] NoteFolders = ["kanji", "vocab", "sentences"];

   readonly DirectoryInfo _folder;
   readonly GitCommandLine _git;

   public CorpusCheckout(IEnvironmentPaths paths)
   {
      _folder = new DirectoryInfo(paths.DatabaseDir);
      _git = new GitCommandLine(_folder);
   }

   /// <summary>The commit the checkout's <c>HEAD</c> is at, by its hash.</summary>
   public CorpusCommit Head() => new(Git(["rev-parse", "HEAD"]).Trim());

   /// <summary>The commit <paramref name="commit"/> names, by its hash, so that it stays the same commit while a branch moves on. Git refuses a name that names no commit.</summary>
   public CorpusCommit ByItsHash(CorpusCommit commit) => new(Git(["rev-parse", "--verify", "--end-of-options", $"{commit.Name}^{{commit}}"]).Trim());

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

   /// <summary>The note files whose contents in the commit <paramref name="from"/> differ from the checkout's files as they are now, with both versions of each.</summary>
   public IReadOnlyList<NoteFileVersions> NoteFilesThatDifferFromTheWorkingTree(CorpusCommit from)
   {
      var paths = NoteFiles(PathsOfTheFilesThatDifferFromTheWorkingTree(from));
      var contentsBefore = ContentsOfTheFiles(from, paths);
      return [.. paths.Select((path, index) => new NoteFileVersions(path, contentsBefore[index], ContentsInTheWorkingTree(path)))];
   }

   /// <summary>The note files whose contents differ between the commits <paramref name="from"/> and <paramref name="to"/>, with both versions of each.</summary>
   public IReadOnlyList<NoteFileVersions> NoteFilesThatDifferBetween(CorpusCommit from, CorpusCommit to)
   {
      var paths = NoteFiles(PathsOfTheFilesThatDifferBetween(from, to));
      var contentsBefore = ContentsOfTheFiles(from, paths);
      var contentsAfter = ContentsOfTheFiles(to, paths);
      return [.. paths.Select((path, index) => new NoteFileVersions(path, contentsBefore[index], contentsAfter[index]))];
   }

   /// <summary>The ids of the notes whose files are among <paramref name="paths"/>, relative to the checkout: the paths in the note folders that are note files.</summary>
   static IReadOnlyList<Guid> IdsOfTheNotesWithTheFiles(IEnumerable<string> paths) =>
      [.. NoteFiles(paths).Select(it => Guid.Parse(Path.GetFileNameWithoutExtension(it)))];

   /// <summary>The paths among <paramref name="paths"/> in the note folders that are note files, rather than another file kept there, such as a snapshot of the notes.</summary>
   static IReadOnlyList<string> NoteFiles(IEnumerable<string> paths) => [.. paths.Where(it => it.EndsWith(".json", StringComparison.Ordinal))];

   /// <summary>The contents of the files <paramref name="paths"/> in <paramref name="commit"/>, in the same order: null for a file the commit doesn't have.</summary>
   IReadOnlyList<string?> ContentsOfTheFiles(CorpusCommit commit, IReadOnlyList<string> paths) => _git.ContentsOfTheFiles([.. paths.Select(it => $"{commit.Name}:{it}")]);

   /// <summary>The contents of the file <paramref name="path"/> in the checkout as it is now: null when there is no such file, as for a deleted note.</summary>
   string? ContentsInTheWorkingTree(string path)
   {
      var file = new FileInfo(Path.Combine(_folder.FullName, path));
      return file.Exists ? File.ReadAllText(file.FullName) : null;
   }

   /// <summary>The paths, relative to the checkout, of the files in the note folders that git reports as differing from <c>HEAD</c>, untracked ones included.</summary>
   IEnumerable<string> PathsOfTheFilesThatDifferFromHead() =>
      PathsInTheStatusEntries(Git(["status", "--porcelain=v1", "-z", "--untracked-files=all", "--", .. NoteFolders]));

   /// <summary>
   /// The paths, relative to the checkout, of the files in the note folders whose contents in the commit <paramref name="from"/> differ from the<br/>
   /// checkout's files as they are now, or that are in only one of the two: the tracked ones, from <c>git diff</c>, and the untracked ones.
   /// </summary>
   IEnumerable<string> PathsOfTheFilesThatDifferFromTheWorkingTree(CorpusCommit from) =>
      PathsEndedByNul(Git(["diff", "--name-only", "--no-renames", "-z", from.Name, "--", .. NoteFolders]))
        .Concat(PathsEndedByNul(Git(["ls-files", "--others", "--exclude-standard", "-z", "--", .. NoteFolders])));

   /// <summary>
   /// The paths, relative to the checkout, of the files in the note folders that differ between the commits <paramref name="from"/> and<br/>
   /// <paramref name="to"/>.
   /// </summary>
   IEnumerable<string> PathsOfTheFilesThatDifferBetween(CorpusCommit from, CorpusCommit to) =>
      PathsEndedByNul(Git(["diff", "--name-only", "--no-renames", "-z", from.Name, to.Name, "--", .. NoteFolders]));

   /// <summary>The paths in the output of a git command run with <c>-z</c> that lists only paths: each path ended by a NUL.</summary>
   static string[] PathsEndedByNul(string output) => output.Split('\0', StringSplitOptions.RemoveEmptyEntries);

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
   string Git(IReadOnlyList<string> arguments) => _git.Output(arguments);
}
