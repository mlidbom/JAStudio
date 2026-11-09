using System;
using System.Diagnostics;
using System.IO;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Specifications.Corpus;

/// <summary>
/// An <see cref="McpServerSpecification"/> whose corpus folder is a git repository, as the corpus checkout a real JAStudio works on is.<br/>
/// The test environment keeps notes in memory, not in files, so specifications write and commit the note files git is to see themselves.
/// </summary>
public abstract class CorpusRepositorySpecification : McpServerSpecification
{
   protected CorpusRepositorySpecification() => Git("init", "--quiet");

   DirectoryInfo CorpusFolder => new(GetService<IEnvironmentPaths>().DatabaseDir);

   /// <summary>The file of <paramref name="note"/>, where the corpus keeps it: <c>vocab/81/81020a41-d636-4ee9-a158-2443d8750482.json</c> for a vocab.</summary>
   protected FileInfo FileOf(JPNote note) => FileOfTheNoteWithTheId(note.GetId().Value, NoteFolderOf(note));

   protected FileInfo FileOfTheNoteWithTheId(Guid id, string noteFolder)
   {
      var idText = id.ToString();
      return new FileInfo(Path.Combine(CorpusFolder.FullName, noteFolder, idText[..2], $"{idText}.json"));
   }

   protected static void Write(FileInfo file, string content)
   {
      Directory.CreateDirectory(file.DirectoryName ?? throw new ArgumentException($"{file.FullName} is in no folder", nameof(file)));
      File.WriteAllText(file.FullName, content);
   }

   protected void CommitEverything()
   {
      Git("add", "--all");
      Git("-c", "user.name=Specification", "-c", "user.email=specification@localhost", "commit", "--quiet", "--message", "The notes as committed");
   }

   static string NoteFolderOf(JPNote note) => note switch
   {
      VocabNote => "vocab",
      KanjiNote => "kanji",
      SentenceNote => "sentences",
      _ => throw new ArgumentOutOfRangeException(nameof(note), note.GetType(), "A kind of note the corpus doesn't store.")
   };

   void Git(params string[] arguments)
   {
      var startInfo = new ProcessStartInfo("git") { WorkingDirectory = CorpusFolder.FullName, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
      foreach(var argument in arguments) startInfo.ArgumentList.Add(argument);

      using var git = Process.Start(startInfo) ?? throw new InvalidOperationException("git started no process.");
      var errors = git.StandardError.ReadToEndAsync();
      git.StandardOutput.ReadToEnd();
      git.WaitForExit();
      if(git.ExitCode != 0) throw new InvalidOperationException($"`git {string.Join(' ', arguments)}` failed: {errors.Result}");
   }
}
