using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace JAStudio.Mcp.Corpus;

/// <summary>Git, run as a command in one repository's checkout: how <see cref="CorpusCheckout"/> asks git about the corpus checkout, and changes it.</summary>
public class GitCommandLine
{
   static readonly UTF8Encoding Utf8WithoutByteOrderMark = new(encoderShouldEmitUTF8Identifier: false);

   readonly DirectoryInfo _checkoutFolder;

   public GitCommandLine(DirectoryInfo checkoutFolder) => _checkoutFolder = checkoutFolder;

   /// <summary>Runs git with <paramref name="arguments"/>, and returns what it writes to its standard output.</summary>
   public string Output(IReadOnlyList<string> arguments) => Utf8WithoutByteOrderMark.GetString(Run(arguments, standardInput: ""));

   /// <summary>
   /// The contents of the files <paramref name="filesOfCommits"/>, each named as git names a file of a commit, <c>commit:path</c>, read with a single<br/>
   /// run of git, <c>git cat-file --batch</c>, however many there are. Null for a file the commit doesn't have.
   /// </summary>
   /// <remarks>
   /// <c>git cat-file --batch</c> reads one name a line, and writes for each a header line, <c>hash type size</c>, followed by the contents, <c>size</c><br/>
   /// bytes, and a line break; or only the line <c>name missing</c>, when there is no such file.
   /// </remarks>
   public IReadOnlyList<string?> ContentsOfTheFiles(IReadOnlyList<string> filesOfCommits)
   {
      if(filesOfCommits.Count == 0) return [];

      var output = Run(["cat-file", "--batch"], string.Concat(filesOfCommits.Select(it => it + "\n")));
      var contents = new List<string?>(filesOfCommits.Count);
      var position = 0;
      foreach(var _ in filesOfCommits)
      {
         var header = LineAt(position);
         position += header.Length + 1;
         if(header.EndsWith(" missing", StringComparison.Ordinal))
         {
            contents.Add(null);
            continue;
         }

         var size = int.Parse(header[(header.LastIndexOf(' ') + 1)..], CultureInfo.InvariantCulture);
         contents.Add(Utf8WithoutByteOrderMark.GetString(output, position, size));
         position += size + 1;
      }

      return contents;

      string LineAt(int start) => Encoding.ASCII.GetString(output, start, Array.IndexOf(output, (byte)'\n', start) - start);
   }

   /// <summary>Runs git with <paramref name="arguments"/>, writing <paramref name="standardInput"/> to its standard input, and returns what it writes to its standard output.</summary>
   /// <remarks>
   /// Git runs with no console window. Inside Anki, which has no console, git would otherwise get a console of its own, which Windows can show<br/>
   /// as a terminal window for every git command. In the first fast-forward from inside Anki (2026-11-09), one git command was ended with<br/>
   /// <c>STATUS_CONTROL_C_EXIT</c>, the exit code of a process whose console closed, though the same command run by hand succeeded.
   /// </remarks>
   byte[] Run(IReadOnlyList<string> arguments, string standardInput)
   {
      var startInfo = new ProcessStartInfo("git")
      {
         WorkingDirectory = _checkoutFolder.FullName,
         UseShellExecute = false,
         CreateNoWindow = true,
         RedirectStandardInput = true,
         RedirectStandardOutput = true,
         RedirectStandardError = true,
         StandardInputEncoding = Utf8WithoutByteOrderMark,
         StandardErrorEncoding = Utf8WithoutByteOrderMark
      };
      foreach(var argument in arguments) startInfo.ArgumentList.Add(argument);

      using var git = Process.Start(startInfo) ?? throw new InvalidOperationException("git started no process.");
      using var output = new MemoryStream();
      var readingTheOutput = git.StandardOutput.BaseStream.CopyToAsync(output);
      var readingTheErrors = git.StandardError.ReadToEndAsync();
      git.StandardInput.Write(standardInput);
      git.StandardInput.Close();
      readingTheOutput.GetAwaiter().GetResult();
      git.WaitForExit();

      return git.ExitCode == 0
                ? output.ToArray()
                : throw new InvalidOperationException($"`git {string.Join(' ', arguments)}` in {_checkoutFolder.FullName} failed with exit code {git.ExitCode}: {readingTheErrors.Result}");
   }
}
