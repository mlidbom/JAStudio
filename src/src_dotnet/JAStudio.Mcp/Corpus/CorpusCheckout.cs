using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Teventive.MemoryImageStateStore;
using JAStudio.Core;
using JAStudio.Core.Storage;

namespace JAStudio.Mcp.Corpus;

/// <summary>
/// The checkout of the corpus repository a JAStudio loaded its notes from and saves them to, as git sees it: which note files differ from the<br/>
/// checkout's <c>HEAD</c>, which is what a change to the notes, such as a full rebuild, changed, the versions of those files, and moving the<br/>
/// checkout to another commit.
/// </summary>
public class CorpusCheckout
{
   readonly DirectoryInfo _folder;
   readonly GitCommandLine _git;
   readonly ITaggregateJsonFiles _noteFiles;

   public CorpusCheckout(IEnvironmentPaths paths, ITaggregateJsonFiles noteFiles)
   {
      _folder = new DirectoryInfo(paths.DatabaseDir);
      _git = new GitCommandLine(_folder);
      _noteFiles = noteFiles;
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
   /// Fetches <paramref name="commit"/>, by its hash or the name of a branch at it there, from the corpus checkout <paramref name="otherClone"/> into<br/>
   /// this checkout's repository, without making a branch for it, and gives it by its hash. For a commit made in another clone of the corpus<br/>
   /// repository, such as a linked worktree's <c>src/jas_database</c>: git keeps a clone of its own for each worktree's submodule.
   /// </summary>
   public CorpusCommit FetchedFrom(DirectoryInfo otherClone, CorpusCommit commit)
   {
      Git(["fetch", "--quiet", otherClone.FullName, commit.Name]);
      return new CorpusCommit(Git(["rev-parse", "FETCH_HEAD"]).Trim());
   }

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
      var noteFiles = NoteFiles(PathsOfTheFilesThatDifferFromTheWorkingTree(from));
      var contentsBefore = ContentsOfTheFiles(from, noteFiles);
      return [.. noteFiles.Select((it, index) => new NoteFileVersions(it.Path, it.Note.Class, contentsBefore[index], ContentsInTheWorkingTree(it.Path)))];
   }

   /// <summary>The note files whose contents differ between the commits <paramref name="from"/> and <paramref name="to"/>, with both versions of each.</summary>
   public IReadOnlyList<NoteFileVersions> NoteFilesThatDifferBetween(CorpusCommit from, CorpusCommit to)
   {
      var noteFiles = NoteFiles(PathsOfTheFilesThatDifferBetween(from, to));
      var contentsBefore = ContentsOfTheFiles(from, noteFiles);
      var contentsAfter = ContentsOfTheFiles(to, noteFiles);
      return [.. noteFiles.Select((it, index) => new NoteFileVersions(it.Path, it.Note.Class, contentsBefore[index], contentsAfter[index]))];
   }

   /// <summary>The ids of the notes whose files are among <paramref name="paths"/>, relative to the checkout.</summary>
   IReadOnlyList<Guid> IdsOfTheNotesWithTheFiles(IEnumerable<string> paths) => [.. NoteFiles(paths).Select(it => it.Note.Id.Value)];

   /// <summary>
   /// The paths among <paramref name="paths"/>, relative to the checkout, that are notes' files, each with the note whose file it is, as the memory<br/>
   /// image state store keeps them (<see cref="ITaggregateJsonFiles.TaggregateWhoseFileIs"/>): not another file kept in the note folders.
   /// </summary>
   IReadOnlyList<(string Path, TaggregateOfAFile Note)> NoteFiles(IEnumerable<string> paths)
   {
      List<(string Path, TaggregateOfAFile Note)> noteFiles = [];
      foreach(var path in paths)
         if(_noteFiles.TaggregateWhoseFileIs(Path.Combine(_folder.FullName, path)) is {} note) noteFiles.Add((path, note));
      return noteFiles;
   }

   /// <summary>The contents of <paramref name="noteFiles"/> in <paramref name="commit"/>, in the same order: null for a file the commit doesn't have.</summary>
   IReadOnlyList<string?> ContentsOfTheFiles(CorpusCommit commit, IReadOnlyList<(string Path, TaggregateOfAFile Note)> noteFiles) =>
      _git.ContentsOfTheFiles([.. noteFiles.Select(it => $"{commit.Name}:{it.Path}")]);

   /// <summary>The contents of the file <paramref name="path"/> in the checkout as it is now: null when there is no such file, as for a deleted note.</summary>
   string? ContentsInTheWorkingTree(string path)
   {
      var file = new FileInfo(Path.Combine(_folder.FullName, path));
      return file.Exists ? File.ReadAllText(file.FullName) : null;
   }

   /// <summary>The paths, relative to the checkout, of the files in the note folders that git reports as differing from <c>HEAD</c>, untracked ones included.</summary>
   IEnumerable<string> PathsOfTheFilesThatDifferFromHead() =>
      PathsInTheStatusEntries(Git(["status", "--porcelain=v1", "-z", "--untracked-files=all", "--", .. CorpusNoteFiles.NoteFolders]));

   /// <summary>
   /// The paths, relative to the checkout, of the files in the note folders whose contents in the commit <paramref name="from"/> differ from the<br/>
   /// checkout's files as they are now, or that are in only one of the two: the tracked ones, from <c>git diff</c>, and the untracked ones.
   /// </summary>
   IEnumerable<string> PathsOfTheFilesThatDifferFromTheWorkingTree(CorpusCommit from) =>
      PathsEndedByNul(Git(["diff", "--name-only", "--no-renames", "-z", from.Name, "--", .. CorpusNoteFiles.NoteFolders]))
        .Concat(PathsEndedByNul(Git(["ls-files", "--others", "--exclude-standard", "-z", "--", .. CorpusNoteFiles.NoteFolders])));

   /// <summary>
   /// The paths, relative to the checkout, of the files in the note folders that differ between the commits <paramref name="from"/> and<br/>
   /// <paramref name="to"/>.
   /// </summary>
   IEnumerable<string> PathsOfTheFilesThatDifferBetween(CorpusCommit from, CorpusCommit to) =>
      PathsEndedByNul(Git(["diff", "--name-only", "--no-renames", "-z", from.Name, to.Name, "--", .. CorpusNoteFiles.NoteFolders]));

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
