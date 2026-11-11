using System;
using System.IO;

namespace JAStudio.Core.Reservoir.Sources.Subtitles;

public static partial class SubtitleCollection
{
   /// <summary>A file in a show's folder, or in a zip there, which may be a subtitle file, or a zip of them.</summary>
   /// <param name="Show">The show whose folder it's in.</param>
   /// <param name="Name">The file's name, without the folders it's in.</param>
   /// <param name="Open">Opens the file to read it.</param>
   sealed record FileInAShowsFolder(string Show, string Name, Func<Stream> Open);
}
