using System;
using System.Collections.Concurrent;
using System.IO;
using System.Security.Cryptography;
using JAStudio.Core.Reservoir.Sources.AnkiPackages.Media;
using Mp3LoudnessNormalization;

namespace JAStudio.Core.Reservoir.Keeping;

/// <summary>
/// The reservoir's media, kept once each, by content: every file normalized, then named by the SHA-256 of its content, such as<br/>
/// <c>3f/a9/3fa9….mp3</c>. A file never changes once stored, the same content from two sources is kept once, and a copy is checked by hashing it.
/// </summary>
/// <remarks>
/// Normalizing is lossless, so that it's done once and never again: an MP3's gain is brought to the ReplayGain reference loudness as mp3gain<br/>
/// does, by changing its frames' global gain, never by re-encoding (<see cref="Mp3File"/>); any other file is kept as it is. Several threads<br/>
/// may store at once: a content being stored by one is left to it by the others.
/// </remarks>
public sealed class MediaStore
{
   readonly DirectoryInfo _folder;
   readonly ConcurrentDictionary<string, byte> _hashesStoredByThisInstance = new();

   public MediaStore(DirectoryInfo folder)
   {
      folder.Create();
      _folder = folder;
   }

   /// <summary>Stores <paramref name="file"/>'s content, normalized, unless the store has it already, and says how it's kept.</summary>
   public StoredMedia Store(PackagedMediaFile file)
   {
      var extension = Path.GetExtension(file.Name).ToLowerInvariant();
      var (content, gainSteps) = IsAnMp3(extension) ? NormalizedMp3(file.Content) : (file.Content, 0);
      var hash = Convert.ToHexStringLower(SHA256.HashData(content));
      var path = PathOf(hash, extension);
      var isNew = !path.Exists && _hashesStoredByThisInstance.TryAdd(hash + extension, 0);
      if(isNew) WriteAtomically(FolderOf(hash), path, content);
      return new StoredMedia(file.Name, hash, extension, file.Content.Length, gainSteps, isNew);
   }

   /// <summary>Where the content with <paramref name="hash"/> and <paramref name="extension"/> is kept, in its <see cref="FolderOf"/>.</summary>
   public FileInfo PathOf(string hash, string extension) => new(Path.Combine(FolderOf(hash).FullName, hash + extension));

   /// <summary>The folder the content with <paramref name="hash"/> is kept in: one by its hash's first two bytes, to keep each folder small.</summary>
   DirectoryInfo FolderOf(string hash) => new(Path.Combine(_folder.FullName, hash[..2], hash[2..4]));

   static bool IsAnMp3(string extension) => extension == ".mp3";

   static (byte[] Content, int GainSteps) NormalizedMp3(byte[] content)
   {
      //bug: Mp3File can't measure some very short clips, which subs2srs decks hold for very short lines, and throws: 9 of about 30,000 MP3s
      //   in the five trial decks (2026-11-11). Some its decoder refuses, "No registered and working codec factory found for decoding format
      //   'mp3'" (NotSupportedException), such as a 131 ms MPEG-2 clip of 5 frames at varying bitrates; some are shorter than ReplayGain's
      //   50 ms window, "No window was counted" (InvalidOperationException). mp3gain leaves a file it can't measure as it is; Mp3File should
      //   say it can't be measured instead of throwing, and this keep such a clip as it is. ImportRun normalizes the same way, and would
      //   fail on such a clip too.
      var mp3 = new Mp3File(content);
      var gainSteps = mp3.MeasureReplayGain().Mp3GainSteps;
      return (mp3.WithGainChangedBy(gainSteps).Bytes(), gainSteps);
   }

   /// <summary>Writes <paramref name="content"/> to <paramref name="path"/>, in <paramref name="folder"/>, through a temporary file, so that the store never holds a file half written.</summary>
   static void WriteAtomically(DirectoryInfo folder, FileInfo path, byte[] content)
   {
      folder.Create();
      var temporary = path.FullName + ".writing";
      File.WriteAllBytes(temporary, content);
      File.Move(temporary, path.FullName);
   }
}
