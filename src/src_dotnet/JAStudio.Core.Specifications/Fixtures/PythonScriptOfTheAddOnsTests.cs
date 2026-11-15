using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Compze.Threading;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases;

namespace JAStudio.Core.Specifications.Fixtures;

/// <summary>
/// A Python script among the Anki add-on's tests, in <c>src/tests</c>, through which specifications reach the add-on's own code: it answers<br/>
/// commands, a line each, with a line of JSON each. Its Python process is started when it's first asked, with the Python of the checkout's venv,<br/>
/// or of the venv the environment variable <c>JASTUDIO_VENV_PATH</c> names, and ends with the specifications' process.
/// </summary>
/// <remarks>
/// One process for all the specifications that use the script, since starting Python, and importing Anki's library, take far longer than most<br/>
/// commands do. Each script says what commands it takes.
/// </remarks>
public sealed class PythonScriptOfTheAddOnsTests
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly ConcurrentQueue<string> _whatPythonWroteToStandardError = new();
   readonly FileInfo _script;
   Process? _python;

   /// <param name="pathInTheTestsFolder">The script's path in <c>src/tests</c>, a folder or file name each, such as <c>"jastudio_tests", "anki_http", "serving_copies_of_the_test_collection.py"</c>.</param>
   public PythonScriptOfTheAddOnsTests(params string[] pathInTheTestsFolder) =>
      _script = new FileInfo(Path.Combine([SourceCheckout.Folder.FullName, "src", "tests", .. pathInTheTestsFolder]));

   /// <summary>The script's answer to <paramref name="command"/>, one line. One command at a time: the specifications run in parallel.</summary>
   public JsonElement AnswerTo(string command) => _monitor.Locked(() =>
   {
      var python = _python ??= StartPython();
      python.StandardInput.WriteLine(command);
      python.StandardInput.Flush();
      var answer = python.StandardOutput.ReadLine()
                ?? throw new InvalidOperationException($"Python ended without answering \"{command}\". It wrote to standard error:{Environment.NewLine}{string.Join(Environment.NewLine, _whatPythonWroteToStandardError)}");
      using var json = JsonDocument.Parse(answer);
      return json.RootElement.Clone();
   });

   Process StartPython()
   {
      var python = Process.Start(new ProcessStartInfo(PythonOfTheVenv.FullName)
                   {
                      ArgumentList = { _script.FullName },
                      RedirectStandardInput = true,
                      RedirectStandardOutput = true,
                      RedirectStandardError = true,
                      UseShellExecute = false,
                      CreateNoWindow = true
                   }) ?? throw new InvalidOperationException($"Couldn't start {PythonOfTheVenv.FullName}.");

      python.ErrorDataReceived += (_, written) =>
      {
         if(written.Data != null) _whatPythonWroteToStandardError.Enqueue(written.Data);
      };
      python.BeginErrorReadLine();
      AppDomain.CurrentDomain.ProcessExit += (_, _) => EndPython(python);
      return python;
   }

   /// <summary>A script ends when its standard input does, after cleaning up after itself.</summary>
   static void EndPython(Process python)
   {
      python.StandardInput.Close();
      python.WaitForExit(TimeSpan.FromSeconds(5));
   }

   static FileInfo PythonOfTheVenv
   {
      get
      {
         var venv = Environment.GetEnvironmentVariable("JASTUDIO_VENV_PATH") is { Length: > 0 } namedByTheEnvironment
                       ? namedByTheEnvironment
                       : Path.Combine(SourceCheckout.Folder.FullName, "venv");
         return new FileInfo(OperatingSystem.IsWindows() ? Path.Combine(venv, "Scripts", "python.exe") : Path.Combine(venv, "bin", "python"));
      }
   }
}
