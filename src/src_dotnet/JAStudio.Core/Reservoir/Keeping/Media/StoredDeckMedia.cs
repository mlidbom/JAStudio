using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Reservoir.Keeping.Media;

/// <summary>
/// The media files of a deck's package, each as the <see cref="MediaStore"/> keeps it (<see cref="StoredMedia"/>), and how long storing them took<br/>
/// (<see cref="MediaStore.StoreTheMediaOf"/>).
/// </summary>
public sealed class StoredDeckMedia
{
   readonly IReadOnlyDictionary<string, StoredMedia> _filesByOriginalName;

   internal StoredDeckMedia(IReadOnlyList<StoredMedia> files, TimeSpan timeTaken)
   {
      Files = files;
      TimeTaken = timeTaken;
      _filesByOriginalName = files.ToDictionary(it => it.OriginalName);
   }

   public IReadOnlyList<StoredMedia> Files { get; }

   /// <summary>How long reading, normalizing, hashing and storing the files took.</summary>
   public TimeSpan TimeTaken { get; }

   /// <summary>The files' length as the package holds them, before normalizing, in bytes.</summary>
   public long OriginalLength => Files.Sum(it => (long)it.OriginalLength);

   /// <summary>How many of the files the store didn't have yet, from an earlier deck or the same one.</summary>
   public int FilesNewToTheStore => Files.Count(it => it.WasNewToTheStore);

   /// <summary>The file the deck's notes refer to as <paramref name="originalName"/>, or null when the package lacks it, as a deck's package can.</summary>
   public StoredMedia? FileNamed(string originalName) => _filesByOriginalName.GetValueOrDefault(originalName);
}
