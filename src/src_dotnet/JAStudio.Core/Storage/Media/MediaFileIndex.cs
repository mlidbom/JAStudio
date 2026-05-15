using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compze.Utilities.Functional;
using Compze.Utilities.Logging;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.Storage.Media;

public class MediaFileIndex
{
   readonly Dictionary<string, MediaAttachment> _byOriginalFileName = new(StringComparer.OrdinalIgnoreCase);
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
   }

   static readonly HashSet<string> ImageExtensions =
      new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };

   static readonly HashSet<string> MetadataExtensions =
      new(StringComparer.OrdinalIgnoreCase) { ".json", ".txt" };

   /// <summary>
   /// Builds the index by scanning all media files in the media root directory.
   /// Files are indexed by their filename.
   /// </summary>
   public void Build()
   {
      ClearIndexes();

      if(!Directory.Exists(_mediaRoot))
      {
         _initialized = true;
         return;
      }

      BuildFromFiles();
   }

   void BuildFromFiles()
   {
      using var runner = _taskRunner.Current("Loading media index");
      var files = runner.RunIndeterminate("Scanning media files", CollectMediaFiles);
      runner.RunBatch(files, IndexFile, "Indexing media files");
      _initialized = true;
      this.Log().Info($"Media file index built: {_byOriginalFileName.Count} files indexed under {_mediaRoot}");
   }

   List<FileInfo> CollectMediaFiles()
   {
      var result = new List<FileInfo>();
      foreach(var fi in new DirectoryInfo(_mediaRoot).EnumerateFiles("*", SearchOption.AllDirectories))
      {
         if(!MetadataExtensions.Contains(fi.Extension))
            result.Add(fi);
      }
      return result;
   }

   void IndexFile(FileInfo fi)
   {
      MediaAttachment attachment = ImageExtensions.Contains(fi.Extension)
         ? new ImageAttachment { OriginalFileName = fi.Name, FilePath = fi.FullName }
         : new AudioAttachment { OriginalFileName = fi.Name, FilePath = fi.FullName };

      if(!_byOriginalFileName.TryAdd(fi.Name, attachment))
         this.Log().Warning($"Duplicate filename '{fi.Name}' — keeping first encountered");
   }

   void ClearIndexes() => _byOriginalFileName.Clear();

   unit EnsureInitialized() => unit.From(() =>
   {
      if(!_initialized) Build();
   });

   public IReadOnlyList<AudioAttachment> GetAudioAttachments(string rawValue) => EnsureInitialized()
     .then(() => MediaFieldParsing.ParseAudioReferences(rawValue)
                                  .Select(r => ResolveAudio(r.FileName))
                                  .OfType<AudioAttachment>()
                                  .ToList());

   public IReadOnlyList<ImageAttachment> GetImageAttachments(string rawValue) => EnsureInitialized()
     .then(() => MediaFieldParsing.ParseImageReferences(rawValue)
                                  .Select(r => ResolveImage(r.FileName))
                                  .OfType<ImageAttachment>()
                                  .ToList());

   AudioAttachment? ResolveAudio(string fileName)
   {
      var stored = _byOriginalFileName.GetValueOrDefault(fileName);
      if(stored == null) return null;
      return stored as AudioAttachment
             ?? new AudioAttachment { OriginalFileName = stored.OriginalFileName, FilePath = stored.FilePath };
   }

   ImageAttachment? ResolveImage(string fileName)
   {
      var stored = _byOriginalFileName.GetValueOrDefault(fileName);
      if(stored == null) return null;
      return stored as ImageAttachment
             ?? new ImageAttachment { OriginalFileName = stored.OriginalFileName, FilePath = stored.FilePath };
   }

   public int Count => EnsureInitialized()
     .then(() => _byOriginalFileName.Count);

   public IReadOnlyCollection<MediaAttachment> All => EnsureInitialized()
     .then(() => _byOriginalFileName.Values);

   public bool ContainsByOriginalFileName(string originalFileName) => EnsureInitialized()
     .then(() => _byOriginalFileName.ContainsKey(originalFileName));

   public MediaAttachment? TryGetByOriginalFileName(string originalFileName) => EnsureInitialized()
     .then(() => _byOriginalFileName.GetValueOrDefault(originalFileName));

   public void Register(MediaAttachment attachment)
   {
      if(attachment.OriginalFileName != null)
         _byOriginalFileName.TryAdd(attachment.OriginalFileName, attachment);
   }
}

