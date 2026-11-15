using System;
using System.IO;

namespace JAStudio.Core;

/// <summary>JAStudio's assemblies as files on disk: where they were loaded from.</summary>
public static class JAStudiosAssemblies
{
   /// <summary>
   /// The folder JAStudio's assemblies were loaded from, all of them from the same one. It also holds the files that the build copies next to<br/>
   /// them, such as the tokenizer's dictionary.
   /// </summary>
   /// <remarks>
   /// It's where to look for such files, never the current directory or <see cref="AppContext.BaseDirectory"/>: when Python hosts .NET, as inside<br/>
   /// Anki, the current directory is the one Anki was started in, and <see cref="AppContext.BaseDirectory"/> is empty.
   /// </remarks>
   /// <remarks>
   /// A .NET host or web application built where Python hosts .NET has to be given it as its content root, for the same reason: left to itself<br/>
   /// it takes the current directory, or, when that is Windows' system directory, <see cref="AppContext.BaseDirectory"/>, and then fails to<br/>
   /// build, with "The path must be absolute". Windows' Start menu starts Anki in the system directory.
   /// </remarks>
   public static DirectoryInfo Folder => new(Path.GetDirectoryName(typeof(JAStudiosAssemblies).Assembly.Location)
                                          ?? throw new InvalidOperationException("JAStudio's assemblies weren't loaded from files."));
}
