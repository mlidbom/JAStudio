using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using JAStudio.Core.Reservoir.Sources.AnkiPackages.Media;
using Mp3LoudnessNormalization;

namespace JAStudio.Core.Reservoir.Keeping.Media;

/// <summary>
/// The reservoir's media, kept once each, by content: every file normalized, then named by the SHA-256 of its content (<see cref="MediaStoreFileName"/>),<br/>
/// in a folder by the hash's first two bytes, such as <c>3f/a9/3fa9….mp3</c>. A file never changes once stored, the same content from two sources<br/>
/// is kept once, and a copy is checked by hashing it.
/// </summary>
/// <remarks>
/// Normalizing is lossless, so that it's done once and never again: an MP3's gain is brought to the ReplayGain reference loudness as mp3gain<br/>
/// does, by changing its frames' global gain, never by re-encoding (<see cref="Mp3File"/>); any other file is kept as it is. Several threads<br/>
/// may store at once: a content being stored by one is left to it by the others.
/// </remarks>
public sealed class MediaStore
{
   readonly DirectoryInfo _folder;

   /// <summary>The names of the contents this instance has stored or is storing, so that a content one call is storing is left to it by the others.</summary>
   readonly ConcurrentDictionary<MediaStoreFileName, byte> _namesClaimedForStoring = new();

   public MediaStore(DirectoryInfo folder)
   {
      folder.Create();
      _folder = folder;
   }

   /// <summary>Stores every media file of <paramref name="package"/>, an .apkg file, normalized, several at once, and says how each is kept.</summary>
   public StoredDeckMedia StoreTheMediaOf(FileInfo package)
   {
      var watch = Stopwatch.StartNew();
      var files = AnkiPackageMedia.In(package).AsParallel().AsOrdered().Select(Store).ToList();
      return new StoredDeckMedia(files, watch.Elapsed);
   }

   /// <summary>Stores <paramref name="file"/>'s content, normalized, unless the store has it already, and says how it's kept.</summary>
   StoredMedia Store(PackagedMediaFile file)
   {
      var (content, gainSteps) = file.IsAnMp3 ? NormalizedMp3(file.Content) : (file.Content, 0);
      var name = MediaStoreFileName.Of(content, file.Extension);
      var isNewToTheStore = IsNewAndClaimedForStoring(name);
      if(isNewToTheStore) WriteAtomically(name, content);
      return new StoredMedia(file.Name, name, file.Content.Length, gainSteps, isNewToTheStore);
   }

   /// <summary>
   /// Whether the content named <paramref name="name"/> is new to the store, and this call is the one to store it: false when the store has it<br/>
   /// already, or another call is storing it.
   /// </summary>
   bool IsNewAndClaimedForStoring(MediaStoreFileName name) => !PathOf(name).Exists && _namesClaimedForStoring.TryAdd(name, 0);

   /// <summary>Where the content named <paramref name="name"/> is kept, in its <see cref="FolderOf"/>.</summary>
   FileInfo PathOf(MediaStoreFileName name) => new(Path.Combine(FolderOf(name).FullName, name.ToString()));

   /// <summary>The folder the content named <paramref name="name"/> is kept in: one by its hash's first two bytes, to keep each folder small.</summary>
   DirectoryInfo FolderOf(MediaStoreFileName name) => new(Path.Combine(_folder.FullName, name.Hash[..2], name.Hash[2..4]));

   static (byte[] Content, int GainSteps) NormalizedMp3(byte[] content)
   {
      var mp3 = new Mp3File(content);
      var gainSteps = mp3.MeasureReplayGainOrNone()?.Mp3GainSteps ?? 0;
      return (mp3.WithGainChangedBy(gainSteps).Bytes(), gainSteps);
   }

   /// <summary>Writes <paramref name="content"/> under <paramref name="name"/>, through a temporary file, so that the store never holds a file half written.</summary>
   void WriteAtomically(MediaStoreFileName name, byte[] content)
   {
      FolderOf(name).Create();
      var path = PathOf(name);
      var temporary = path.FullName + ".writing";
      File.WriteAllBytes(temporary, content);
      File.Move(temporary, path.FullName);
   }
}
