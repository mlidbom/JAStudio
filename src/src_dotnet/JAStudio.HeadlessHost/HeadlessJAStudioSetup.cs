using System;
using System.IO;

namespace JAStudio.HeadlessHost;

/// <summary>
/// What the headless JAStudio loads: the source checkout's add-on folder, the corpus checkout and the Anki collection. The MCP gateway has a<br/>
/// <c>HeadlessJAStudioSetup</c> of its own, which it starts the headless JAStudio with, and which this one is read back from.
/// </summary>
/// <remarks>Every folder and file is checked to exist when it's read, so a wrong one fails the headless JAStudio before it loads anything.</remarks>
sealed class HeadlessJAStudioSetup
{
   HeadlessJAStudioSetup(CommandLineValues values)
   {
      AddonFolder = values.ExistingFolder("addon-folder");
      CorpusFolder = values.ExistingFolder("corpus-folder");
      AnkiCollectionFile = values.ExistingFile("anki-collection-file");
      AnkiProfileFolder = AnkiCollectionFile.Directory ?? throw new ArgumentException($"--anki-collection-file: {AnkiCollectionFile.FullName} is in no folder");
   }

   /// <summary>The setup given as <c>--addon-folder</c>, <c>--corpus-folder</c> and <c>--anki-collection-file</c> in <paramref name="values"/>.</summary>
   public static HeadlessJAStudioSetup From(CommandLineValues values) => new(values);

   /// <summary>The source checkout's <c>src</c> folder: the folder Anki loads JAStudio from, with the add-on's configuration and the user files.</summary>
   public DirectoryInfo AddonFolder { get; }

   /// <summary>The checkout of the corpus repository to load the notes from, and save them to.</summary>
   public DirectoryInfo CorpusFolder { get; }

   /// <summary>The Anki profile's collection database, <c>collection.anki2</c>, which the notes' studying statuses are read from.</summary>
   public FileInfo AnkiCollectionFile { get; }

   /// <summary>The folder of the Anki profile whose collection the headless JAStudio reads: it holds <see cref="AnkiCollectionFile"/> and the profile's media.</summary>
   public DirectoryInfo AnkiProfileFolder { get; }
}
