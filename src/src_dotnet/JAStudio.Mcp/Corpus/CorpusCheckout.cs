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
/// checkout's <c>HEAD</c>, which is what a change to the notes, such as a full rebuild, changed.
/// </summary>
public class CorpusCheckout
{
   /// <summary>The folders of the corpus repository that hold the notes, one file per note, named by its id.</summary>
   static readonly string[] NoteFolders = ["kanji", "vocab", "sentences"];

   readonly DirectoryInfo _folder;

   public CorpusCheckout(IEnvironmentPaths paths) => _folder = new DirectoryInfo(paths.DatabaseDir);

   /// <summary>
   /// The ids of the notes whose files differ from the checkout's <c>HEAD</c>: changed, added or deleted, staged or not. A note file is named by<br/>
   /// the note's id, such as <c>vocab/81/81020a41-d636-4ee9-a158-2443d8750482.json</c>.
   /// </summary>
   public IReadOnlyList<Guid> IdsOfTheNotesWhoseFilesDifferFromHead() =>
      [.. PathsOfTheFilesThatDifferFromHead().Select(it => Guid.Parse(Path.GetFileNameWithoutExtension(it)))];

   /// <summary>The paths, relative to the checkout, of the note files git reports as differing from <c>HEAD</c>, untracked ones included.</summary>
   IEnumerable<string> PathsOfTheFilesThatDifferFromHead() =>
      PathsInTheStatusEntries(Git(["status", "--porcelain=v1", "-z", "--untracked-files=all", "--", .. NoteFolders]))
        .Where(it => it.EndsWith(".json", StringComparison.Ordinal));

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

   string Git(IReadOnlyList<string> arguments)
   {
      var startInfo = new ProcessStartInfo("git")
      {
         WorkingDirectory = _folder.FullName,
         UseShellExecute = false,
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
