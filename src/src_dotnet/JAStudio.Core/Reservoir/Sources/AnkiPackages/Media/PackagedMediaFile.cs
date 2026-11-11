using System.IO;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages.Media;

/// <summary>A media file an Anki package holds, such as a sentence's audio or a scene's screenshot: its name, as the deck's notes refer to it, and its content.</summary>
/// <param name="Name">The file's name, such as <c>show_01_0.01.mp3</c>, as a note's <c>[sound:…]</c> or <c>&lt;img src="…"&gt;</c> refers to it.</param>
/// <param name="Content">The file's bytes, uncompressed.</param>
public sealed record PackagedMediaFile(string Name, byte[] Content)
{
   /// <summary>The extension of the file's name, lowercase, such as <c>.mp3</c>.</summary>
   public string Extension => Path.GetExtension(Name).ToLowerInvariant();

   /// <summary>Whether the file is an MP3, by its name's extension.</summary>
   public bool IsAnMp3 => Extension == ".mp3";
}
