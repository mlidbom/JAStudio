using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Compze.Threading;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// The Anki add-on's HTTP servers for JAStudio that the specifications call: each serves a copy of the test collection of its own, opened with<br/>
/// Anki's own library, without Anki's window (<see cref="AnkiServingACopyOfTheTestCollection"/>). One Python process runs them all, started<br/>
/// when the first specification asks for a server, with the Python of the checkout's venv, or of the venv the environment variable<br/>
/// <c>JASTUDIO_VENV_PATH</c> names. It ends with the specifications' process.
/// </summary>
/// <remarks>
/// One process, since starting Python and importing Anki's library takes far longer than a copy of the collection does. The process is<br/>
/// <c>src/tests/jastudio_tests/anki_http/serving_copies_of_the_test_collection.py</c>, which says what commands it takes.
/// </remarks>
static class AnkisHttpServersForSpecifications
{
   static readonly IMonitor Monitor = IMonitor.New();
   static readonly ConcurrentQueue<string> WhatPythonWroteToStandardError = new();
   static Process? _pythonServingTheCopies;

   /// <summary>Starts a server on a new copy of the test collection, which serves until it's disposed.</summary>
   public static AnkiServingACopyOfTheTestCollection StartOne() => new(AnswerTo("start"));

   /// <summary>Python's answer to <paramref name="command"/>, one of those <c>serving_copies_of_the_test_collection.py</c> takes. One command at a time: the specifications run in parallel.</summary>
   internal static JsonElement AnswerTo(string command) => Monitor.Locked(() =>
   {
      var python = _pythonServingTheCopies ??= StartPython();
      python.StandardInput.WriteLine(command);
      python.StandardInput.Flush();
      var answer = python.StandardOutput.ReadLine()
                ?? throw new InvalidOperationException($"Python ended without answering \"{command}\". It wrote to standard error:{Environment.NewLine}{string.Join(Environment.NewLine, WhatPythonWroteToStandardError)}");
      using var json = JsonDocument.Parse(answer);
      return json.RootElement.Clone();
   });

   static Process StartPython()
   {
      var python = Process.Start(new ProcessStartInfo(PythonOfTheVenv.FullName)
                   {
                      ArgumentList = { TheScriptServingTheCopies.FullName },
                      RedirectStandardInput = true,
                      RedirectStandardOutput = true,
                      RedirectStandardError = true,
                      UseShellExecute = false,
                      CreateNoWindow = true
                   }) ?? throw new InvalidOperationException($"Couldn't start {PythonOfTheVenv.FullName}.");

      python.ErrorDataReceived += (_, written) =>
      {
         if(written.Data != null) WhatPythonWroteToStandardError.Enqueue(written.Data);
      };
      python.BeginErrorReadLine();
      AppDomain.CurrentDomain.ProcessExit += (_, _) => EndPython(python);
      return python;
   }

   /// <summary>Python ends when its standard input does, after stopping the servers still running and removing their copies of the collection.</summary>
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

   static FileInfo TheScriptServingTheCopies =>
      new(Path.Combine(SourceCheckout.Folder.FullName, "src", "tests", "jastudio_tests", "anki_http", "serving_copies_of_the_test_collection.py"));
}
