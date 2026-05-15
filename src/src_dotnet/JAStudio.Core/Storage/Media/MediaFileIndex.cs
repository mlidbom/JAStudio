using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Utilities.Functional;
using Compze.Utilities.Logging;
using JAStudio.Core.Note;
using JAStudio.Core.TaskRunners;
using MemoryPack;

namespace JAStudio.Core.Storage.Media;

public class MediaFileInfo(MediaFileId id, string fullPath, string originalFileName, string extension)
{
   public MediaFileId Id { get; } = id;
   public string FullPath { get; } = fullPath;
   public string OriginalFileName { get; } = originalFileName;
   public string Extension { get; } = extension;
}

public class MediaFileIndex
{
   readonly Dictionary<MediaFileId, MediaAttachment> _byId = new();
   readonly Dictionary<string, MediaAttachment> _byOriginalFileName = new(StringComparer.OrdinalIgnoreCase);
   readonly Dictionary<NoteId, List<MediaAttachment>> _byNoteId = new();
   readonly string _mediaRoot;
   readonly TaskRunner _taskRunner;
   readonly BackgroundTaskManager _backgroundTaskManager;
   bool _initialized;

   public MediaFileIndex(IEnvironmentPaths paths, TaskRunner taskRunner, BackgroundTaskManager backgroundTaskManager)
      : this(paths.MediaDir, taskRunner, backgroundTaskManager) {}

   public MediaFileIndex(string mediaRoot, TaskRunner taskRunner, BackgroundTaskManager backgroundTaskManager)
   {
      _mediaRoot = mediaRoot;
      _taskRunner = taskRunner;
      _backgroundTaskManager = backgroundTaskManager;
      _metadataDir = Path.Combine(mediaRoot, "metadata");
   }

   readonly string _metadataDir;
   string SnapshotPath => Path.Combine(_metadataDir, "media-snapshot.bin");

   record ScannedSidecar(string Path, MediaFileId Id, DateTime LastWriteUtc);

   record MediaScanResult(
      List<ScannedSidecar> AudioSidecars,
      List<ScannedSidecar> ImageSidecars,
      Dictionary<string, List<FileInfo>> MediaFilesByDirectory);

   /// <summary>
   /// Builds the index with progress reporting via RunBatch.
   /// Single filesystem enumeration, then parallel sidecar deserialization.
   /// Uses a binary snapshot for fast subsequent loads (only changed sidecars are re-read).
   /// </summary>
   public void Build()
   {
      ClearIndexes();

      if(!Directory.Exists(_mediaRoot))
      {
         _initialized = true;
         return;
      }

      if(File.Exists(SnapshotPath))
      {
         try
         {
            BuildWithSnapshotMerge();
            return;
         }
         catch(Exception ex)
         {
            this.Log().Error(ex, "Failed to load media snapshot, falling back to full rebuild");
         }
      }

      BuildFromSidecarsAndSaveSnapshot();
   }

   void BuildFromSidecarsAndSaveSnapshot()
   {
      using var runner = _taskRunner.Current("Loading media index");

      var scan = runner.RunIndeterminate("Scanning media files", ScanMediaFiles);

      var audioAttachments = runner.RunBatchAsync(scan.AudioSidecars, s => DeserializeSidecar(s.Path, isAudio: true, scan.MediaFilesByDirectory), "Loading audio sidecars", ThreadCount.FractionOfLogicalCores(0.3));
      var imageAttachments = runner.RunBatchAsync(scan.ImageSidecars, s => DeserializeSidecar(s.Path, isAudio: false, scan.MediaFilesByDirectory), "Loading image sidecars", ThreadCount.FractionOfLogicalCores(0.3));

      runner.RunBatch(audioAttachments.Result, IndexAttachment, "Indexing audio attachments");
      runner.RunBatch(imageAttachments.Result, IndexAttachment, "Indexing image attachments");

      _initialized = true;
      this.Log().Info($"Media file index built: {_byId.Count} files indexed under {_mediaRoot}");

      var audio = audioAttachments.Result;
      var images = imageAttachments.Result;
      _backgroundTaskManager.Run(() => SaveSnapshot(MediaSnapshotConverter.ToContainer(audio, images)));
   }

   void BuildWithSnapshotMerge()
   {
      using var runner = _taskRunner.Current("Loading media index (snapshot merge)");

      var snapshotTimestamp = File.GetLastWriteTimeUtc(SnapshotPath);
      var container = runner.RunIndeterminate("Loading media snapshot", DeserializeSnapshot);
      var scan = runner.RunIndeterminate("Scanning media files", ScanMediaFiles);

      var changes = runner.RunIndeterminate("Finding sidecar changes",
                                            () => FindChanges(container, scan, snapshotTimestamp));

      if(changes.HasChanges)
      {
         container = runner.RunIndeterminate("Patching snapshot with changes",
                                             () => PatchSnapshot(container, changes, scan.MediaFilesByDirectory));

         var snapshotToSave = container;
         _backgroundTaskManager.Run(() => SaveSnapshot(snapshotToSave));
      }

      var audioAttachments = runner.RunBatchAsync(container.Audio, MediaSnapshotConverter.ToAudioAttachment, "Constructing audio attachments");
      var imageAttachments = runner.RunBatchAsync(container.Images, MediaSnapshotConverter.ToImageAttachment, "Constructing image attachments");

      runner.RunBatch(audioAttachments.Result, IndexAttachment, "Indexing audio attachments");
      runner.RunBatch(imageAttachments.Result, IndexAttachment, "Indexing image attachments");

      _initialized = true;
      this.Log().Info($"Media file index built from snapshot: {_byId.Count} files indexed under {_mediaRoot}");
   }

   record SidecarChanges(
      List<ScannedSidecar> ChangedAudio,
      List<ScannedSidecar> ChangedImages,
      HashSet<Guid> CurrentAudioIds,
      HashSet<Guid> CurrentImageIds,
      int DeletedCount)
   {
      public bool HasChanges => ChangedAudio.Count + ChangedImages.Count + DeletedCount > 0;
   }

   static SidecarChanges FindChanges(MediaSnapshotContainer container, MediaScanResult scan, DateTime snapshotTimestamp)
   {
      var currentAudioIds = scan.AudioSidecars.Select(s => s.Id.Value).ToHashSet();
      var currentImageIds = scan.ImageSidecars.Select(s => s.Id.Value).ToHashSet();

      var changedAudio = scan.AudioSidecars.Where(s => s.LastWriteUtc > snapshotTimestamp).ToList();
      var changedImages = scan.ImageSidecars.Where(s => s.LastWriteUtc > snapshotTimestamp).ToList();

      var deletedAudioCount = container.Audio.Count(d => !currentAudioIds.Contains(d.Id));
      var deletedImageCount = container.Images.Count(d => !currentImageIds.Contains(d.Id));

      return new SidecarChanges(changedAudio,
                                changedImages,
                                currentAudioIds,
                                currentImageIds,
                                deletedAudioCount + deletedImageCount);
   }

   static MediaSnapshotContainer PatchSnapshot(MediaSnapshotContainer container, SidecarChanges changes, Dictionary<string, List<FileInfo>> mediaFilesByDirectory)
   {
      var audioMap = container.Audio.ToDictionary(d => d.Id);
      var imageMap = container.Images.ToDictionary(d => d.Id);

      foreach(var sidecar in changes.ChangedAudio)
      {
         var attachment = (AudioAttachment)DeserializeSidecar(sidecar.Path, isAudio: true, mediaFilesByDirectory);
         audioMap[sidecar.Id.Value] = MediaSnapshotConverter.ToSnapshotData(attachment);
      }

      foreach(var sidecar in changes.ChangedImages)
      {
         var attachment = (ImageAttachment)DeserializeSidecar(sidecar.Path, isAudio: false, mediaFilesByDirectory);
         imageMap[sidecar.Id.Value] = MediaSnapshotConverter.ToSnapshotData(attachment);
      }

      foreach(var id in audioMap.Keys.Where(id => !changes.CurrentAudioIds.Contains(id)).ToList())
         audioMap.Remove(id);
      foreach(var id in imageMap.Keys.Where(id => !changes.CurrentImageIds.Contains(id)).ToList())
         imageMap.Remove(id);

      return new MediaSnapshotContainer
             {
                Audio = audioMap.Values.ToList(),
                Images = imageMap.Values.ToList(),
             };
   }

   void SaveSnapshot(MediaSnapshotContainer container)
   {
      Directory.CreateDirectory(Path.GetDirectoryName(SnapshotPath)!);
      using var stream = File.Create(SnapshotPath);
      MemoryPackSerializer.SerializeAsync(stream, container).GetAwaiter().GetResult();
   }

   MediaSnapshotContainer DeserializeSnapshot()
   {
      using var stream = File.OpenRead(SnapshotPath);
      return MemoryPackSerializer.DeserializeAsync<MediaSnapshotContainer>(stream).GetAwaiter().GetResult()
          ?? throw new InvalidOperationException("Media snapshot deserialization returned null");
   }

   void ClearIndexes()
   {
      _byId.Clear();
      _byOriginalFileName.Clear();
      _byNoteId.Clear();
   }

   /// <summary>
   /// Single enumeration of the media directory tree. Separates files into
   /// audio sidecars, image sidecars (with IDs and timestamps), and media files grouped by directory.
   /// </summary>
   MediaScanResult ScanMediaFiles()
   {
      var audioSidecars = new List<ScannedSidecar>();
      var imageSidecars = new List<ScannedSidecar>();
      var mediaFilesByDirectory = new Dictionary<string, List<FileInfo>>(StringComparer.OrdinalIgnoreCase);

      var metadataPrefix = _metadataDir + Path.DirectorySeparatorChar;

      foreach(var fi in new DirectoryInfo(_mediaRoot).EnumerateFiles("*", SearchOption.AllDirectories))
      {
         var fullName = fi.FullName;
         if(fullName.StartsWith(metadataPrefix, StringComparison.OrdinalIgnoreCase))
            continue;

         if(fullName.EndsWith(SidecarSerializer.AudioSidecarExtension, StringComparison.OrdinalIgnoreCase))
         {
            var id = MediaFileId.Parse(fi.Name[..^SidecarSerializer.AudioSidecarExtension.Length]);
            audioSidecars.Add(new ScannedSidecar(fullName, id, fi.LastWriteTimeUtc));
         } else if(fullName.EndsWith(SidecarSerializer.ImageSidecarExtension, StringComparison.OrdinalIgnoreCase))
         {
            var id = MediaFileId.Parse(fi.Name[..^SidecarSerializer.ImageSidecarExtension.Length]);
            imageSidecars.Add(new ScannedSidecar(fullName, id, fi.LastWriteTimeUtc));
         } else
         {
            var dir = fi.DirectoryName ?? string.Empty;
            if(!mediaFilesByDirectory.TryGetValue(dir, out var list))
            {
               list = [];
               mediaFilesByDirectory[dir] = list;
            }

            list.Add(fi);
         }
      }

      return new MediaScanResult(audioSidecars, imageSidecars, mediaFilesByDirectory);
   }

   static MediaAttachment DeserializeSidecar(string sidecarPath, bool isAudio, Dictionary<string, List<FileInfo>> mediaFilesByDirectory)
   {
      MediaAttachment attachment = isAudio
                                      ? SidecarSerializer.ReadAudioSidecar(sidecarPath)
                                      : SidecarSerializer.ReadImageSidecar(sidecarPath);

      var dir = Path.GetDirectoryName(sidecarPath) ?? string.Empty;
      attachment.FilePath = FindMediaFile(dir, attachment.Id, mediaFilesByDirectory) ?? string.Empty;
      return attachment;
   }

   void IndexAttachment(MediaAttachment attachment)
   {
      if(!_byId.TryAdd(attachment.Id, attachment))
      {
         this.Log().Warning($"Duplicate media file ID {attachment.Id}");
         return;
      }

      if(attachment.OriginalFileName != null)
      {
         _byOriginalFileName.TryAdd(attachment.OriginalFileName, attachment);
      }

      foreach(var noteId in attachment.NoteIds)
      {
         if(!_byNoteId.TryGetValue(noteId, out var list))
         {
            list = [];
            _byNoteId[noteId] = list;
         }

         list.Add(attachment);
      }
   }

   static string? FindMediaFile(string directory, MediaFileId id, Dictionary<string, List<FileInfo>> mediaFilesByDirectory)
   {
      if(!mediaFilesByDirectory.TryGetValue(directory, out var files)) return null;

      var idStr = id.ToString();
      foreach(var fi in files)
      {
         if(Path.GetFileNameWithoutExtension(fi.Name) == idStr)
            return fi.FullName;
      }

      return null;
   }

   unit EnsureInitialized() => unit.From(() =>
   {
      if(!_initialized) Build();
   });

   public string? TryResolve(MediaFileId id) => EnsureInitialized()
     .then(() => _byId.TryGetValue(id, out var attachment) ? attachment.FilePath : null);

   public MediaAttachment? TryGetAttachment(MediaFileId id) => EnsureInitialized()
     .then(() => _byId.GetValueOrDefault(id));

   public MediaFileInfo? TryGetInfo(MediaFileId id) => EnsureInitialized()
     .then(() => _byId.TryGetValue(id, out var attachment)
                    ? new MediaFileInfo(attachment.Id, attachment.FilePath, attachment.OriginalFileName ?? string.Empty, Path.GetExtension(attachment.FilePath))
                    : null);

   public NoteMedia GetNoteMedia(NoteId noteId) => EnsureInitialized()
     .then(() => _byNoteId.TryGetValue(noteId, out var attachments)
                    ? new NoteMedia(attachments.OfType<AudioAttachment>().ToList(), attachments.OfType<ImageAttachment>().ToList())
                    : NoteMedia.Empty);

   public bool Contains(MediaFileId id) => EnsureInitialized()
     .then(() => _byId.ContainsKey(id));

   public int Count => EnsureInitialized()
     .then(() => _byId.Count);

   public IReadOnlyCollection<MediaAttachment> All => EnsureInitialized()
     .then(() => _byId.Values);

   public bool ContainsByOriginalFileName(string originalFileName) => EnsureInitialized()
     .then(() => _byOriginalFileName.ContainsKey(originalFileName));

   public MediaAttachment? TryGetByOriginalFileName(string originalFileName) => EnsureInitialized()
     .then(() => _byOriginalFileName.GetValueOrDefault(originalFileName));

   public void ResaveAllSidecars()
   {
      foreach(var attachment in All)
      {
         if(string.IsNullOrEmpty(attachment.FilePath)) continue;

         if(attachment is AudioAttachment audio)
            SidecarSerializer.WriteAudioSidecar(SidecarSerializer.BuildAudioSidecarPath(attachment.FilePath), audio);
         else if(attachment is ImageAttachment image)
            SidecarSerializer.WriteImageSidecar(SidecarSerializer.BuildImageSidecarPath(attachment.FilePath), image);
      }
   }

   public void Register(MediaAttachment attachment)
   {
      _byId[attachment.Id] = attachment;

      if(attachment.OriginalFileName != null)
      {
         _byOriginalFileName.TryAdd(attachment.OriginalFileName, attachment);
      }

      foreach(var noteId in attachment.NoteIds)
      {
         if(!_byNoteId.TryGetValue(noteId, out var list))
         {
            list = [];
            _byNoteId[noteId] = list;
         }

         list.Add(attachment);
      }
   }
}
