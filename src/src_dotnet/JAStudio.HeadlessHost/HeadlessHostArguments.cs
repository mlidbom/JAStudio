using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace JAStudio.HeadlessHost;

/// <summary>
/// What the headless JAStudio is started with: which source checkout, corpus checkout and Anki collection it works with, where its MCP server<br/>
/// listens, and which process it lives as long as. The MCP gateway starts it with all of them, as <c>--name value</c> pairs.
/// </summary>
/// <remarks>Every argument is read and checked when the headless JAStudio starts, so a missing or wrong one fails it before it loads anything.</remarks>
sealed class HeadlessHostArguments
{
   HeadlessHostArguments(IReadOnlyDictionary<string, string> valuesByName)
   {
      AddonFolder = ExistingFolder("addon-folder");
      CorpusFolder = ExistingFolder("corpus-folder");
      AnkiCollectionFile = ExistingFile("anki-collection-file");
      AnkiProfileFolder = AnkiCollectionFile.Directory ?? throw new ArgumentException($"--anki-collection-file: {AnkiCollectionFile.FullName} is in no folder");
      McpServerPort = int.Parse(Value("mcp-server-port"), CultureInfo.InvariantCulture);
      ProcessToExitWith = Process.GetProcessById(int.Parse(Value("exit-with-process"), CultureInfo.InvariantCulture));
      return;

      DirectoryInfo ExistingFolder(string name)
      {
         var folder = new DirectoryInfo(Value(name));
         return folder.Exists ? folder : throw new ArgumentException($"--{name}: no folder {folder.FullName}");
      }

      FileInfo ExistingFile(string name)
      {
         var file = new FileInfo(Value(name));
         return file.Exists ? file : throw new ArgumentException($"--{name}: no file {file.FullName}");
      }

      string Value(string name) => valuesByName.TryGetValue(name, out var value) ? value : throw new ArgumentException($"--{name} is required");
   }

   public static HeadlessHostArguments From(string[] args)
   {
      if(args.Length % 2 != 0 || !args.Where((_, index) => index % 2 == 0).All(it => it.StartsWith("--", StringComparison.Ordinal)))
         throw new ArgumentException($"Expected --name value pairs, got: {string.Join(' ', args)}");

      return new HeadlessHostArguments(args.Chunk(2).ToDictionary(it => it[0][2..], it => it[1]));
   }

   /// <summary>The source checkout's <c>src</c> folder: the folder Anki loads JAStudio from, with the add-on's configuration and the user files.</summary>
   public DirectoryInfo AddonFolder { get; }

   /// <summary>The checkout of the corpus repository to load the notes from, and save them to.</summary>
   public DirectoryInfo CorpusFolder { get; }

   /// <summary>The Anki profile's collection database, <c>collection.anki2</c>, which the notes' studying statuses are read from.</summary>
   public FileInfo AnkiCollectionFile { get; }

   /// <summary>The folder of the Anki profile whose collection the headless JAStudio reads: it holds <see cref="AnkiCollectionFile"/> and the profile's media.</summary>
   public DirectoryInfo AnkiProfileFolder { get; }

   /// <summary>The port of this computer's loopback address that the MCP server listens on, or 0 for a free port the system picks.</summary>
   public int McpServerPort { get; }

   /// <summary>The process the headless JAStudio exits with, the MCP gateway that started it, so that it never outlives it.</summary>
   public Process ProcessToExitWith { get; }
}
