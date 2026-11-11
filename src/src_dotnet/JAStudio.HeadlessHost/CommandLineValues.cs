using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace JAStudio.HeadlessHost;

/// <summary>The values a command line gives as <c>--name value</c> pairs, read by name, each checked as it's read.</summary>
sealed class CommandLineValues
{
   readonly IReadOnlyDictionary<string, string> _valuesByName;

   CommandLineValues(IReadOnlyDictionary<string, string> valuesByName) => _valuesByName = valuesByName;

   /// <summary>The values in <paramref name="args"/>, which must be <c>--name value</c> pairs.</summary>
   public static CommandLineValues From(IReadOnlyList<string> args)
   {
      if(args.Count % 2 != 0 || !args.Where((_, index) => index % 2 == 0).All(it => it.StartsWith("--", StringComparison.Ordinal)))
         throw new ArgumentException($"Expected --name value pairs, got: {string.Join(' ', args)}");

      return new CommandLineValues(args.Chunk(2).ToDictionary(it => it[0][2..], it => it[1]));
   }

   /// <summary>The value given as <c>--</c><paramref name="name"/>, which must be given.</summary>
   public string Value(string name) => _valuesByName.TryGetValue(name, out var value) ? value : throw new ArgumentException($"--{name} is required");

   /// <summary>The whole number given as <c>--</c><paramref name="name"/>, such as a port or a count.</summary>
   public int Integer(string name) => int.Parse(Value(name), CultureInfo.InvariantCulture);

   /// <summary>The folder given as <c>--</c><paramref name="name"/>, which must exist.</summary>
   public DirectoryInfo ExistingFolder(string name)
   {
      var folder = new DirectoryInfo(Value(name));
      return folder.Exists ? folder : throw new ArgumentException($"--{name}: no folder {folder.FullName}");
   }

   /// <summary>The file given as <c>--</c><paramref name="name"/>, which must exist.</summary>
   public FileInfo ExistingFile(string name)
   {
      var file = new FileInfo(Value(name));
      return file.Exists ? file : throw new ArgumentException($"--{name}: no file {file.FullName}");
   }

   /// <summary>The file, or else the folder, given as <c>--</c><paramref name="name"/>, which must exist.</summary>
   public FileSystemInfo ExistingFileOrFolder(string name) => File.Exists(Value(name)) ? new FileInfo(Value(name)) : ExistingFolder(name);
}
