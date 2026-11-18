using System;
using System.Diagnostics;
using System.IO;
using Compze.Teventive.MemoryImageStateStore;
using JAStudio.Core;
using JAStudio.Core.TestUtils;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;

namespace JAStudio.Mcp.Specifications;

/// <summary>
/// An <see cref="McpServerSpecification"/> whose corpus folder is a git repository, as the corpus checkout a real JAStudio works on is, and where the<br/>
/// state store keeps the notes in their files, as a real JAStudio does.
/// </summary>
public abstract class CorpusRepositorySpecification : McpServerSpecification
{
   protected CorpusRepositorySpecification() : base(NotesKept.InTheirFiles) => Git("init", "--quiet");

   DirectoryInfo CorpusFolder => new(GetService<IEnvironmentPaths>().DatabaseDir);

   /// <summary>The file of <paramref name="note"/>, where the corpus keeps it: <c>vocab/81/81020a41-d636-4ee9-a158-2443d8750482.json</c> for a vocab.</summary>
   protected FileInfo FileOf(JPNote note) => new(GetService<ITaggregateJsonFiles>().PathOf(note));

   /// <summary>The file a vocab with the id <paramref name="id"/> would have, whether the collection has such a vocab or not.</summary>
   protected FileInfo FileOfTheVocabWithTheId(Guid id) => new(GetService<ITaggregateJsonFiles>().PathOf<VocabNote>(new VocabId(id)));

   /// <summary>Writes the files of <paramref name="notes"/>, each as the corpus stores the note as it is now.</summary>
   protected void WriteTheFilesOf(params JPNote[] notes)
   {
      foreach(var note in notes) Write(FileOf(note), GetService<NoteSerializer>().Serialize(note));
   }

   protected static void Write(FileInfo file, string content)
   {
      Directory.CreateDirectory(file.DirectoryName ?? throw new ArgumentException($"{file.FullName} is in no folder", nameof(file)));
      File.WriteAllText(file.FullName, content);
   }

   /// <summary>Commits every file in the corpus folder, on the branch checked out, and gives the new commit's hash.</summary>
   protected string CommitEverything(string message = "The notes as committed")
   {
      Git("add", "--all");
      Git("-c", "user.name=Specification", "-c", "user.email=specification@localhost", "commit", "--quiet", "--message", message);
      return HeadCommit();
   }

   protected string HeadCommit() => Git("rev-parse", "HEAD").Trim();

   /// <summary>Runs git with <paramref name="arguments"/> in the corpus folder, and gives what it wrote to standard output.</summary>
   protected string Git(params string[] arguments)
   {
      var startInfo = new ProcessStartInfo("git") { WorkingDirectory = CorpusFolder.FullName, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
      foreach(var argument in arguments) startInfo.ArgumentList.Add(argument);

      using var git = Process.Start(startInfo) ?? throw new InvalidOperationException("git started no process.");
      var errors = git.StandardError.ReadToEndAsync();
      var output = git.StandardOutput.ReadToEnd();
      git.WaitForExit();
      return git.ExitCode == 0 ? output : throw new InvalidOperationException($"`git {string.Join(' ', arguments)}` failed: {errors.Result}");
   }
}
