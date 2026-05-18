using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Compze.Internals.Logging;
using MemoryPack;

namespace JAStudio.Core.Storage;

public class JsonFilesystemObjectRepository<TData> where TData : class, IIdentifiableByGuid
{
   readonly string _directory;
   readonly JsonSerializerOptions _jsonOptions;
   readonly HashSet<string> _ensuredDirectories = new(StringComparer.OrdinalIgnoreCase);

   string SnapshotPath => Path.Combine(_directory, "snapshot.bin");

   public JsonFilesystemObjectRepository(string directory, JsonSerializerOptions jsonOptions)
   {
      ValidateDataTypeIsMemoryPackable();
      _directory = directory;
      _jsonOptions = jsonOptions;
   }

   static void ValidateDataTypeIsMemoryPackable()
   {
      var type = typeof(TData);
      var hasAttribute = type.GetCustomAttributes(typeof(MemoryPackableAttribute), inherit: false).Length > 0;
      if(!hasAttribute)
         throw new InvalidOperationException(
            $"Type {type.FullName} must be decorated with [MemoryPackable] to be used with {nameof(JsonFilesystemObjectRepository<TData>)}");
   }

   public void Save(TData data)
   {
      var path = FilePath(data.Id);
      EnsureDirectoryExists(Path.GetDirectoryName(path)!);
      File.WriteAllText(path, JsonSerializer.Serialize(data, _jsonOptions));
   }

   public void Delete(Guid id)
   {
      var path = FilePath(id);
      if(File.Exists(path))
         File.Delete(path);
   }

   public List<TData> LoadAll()
   {
      if(File.Exists(SnapshotPath))
      {
         try
         {
            return LoadWithSnapshotMerge();
         }
         catch(Exception ex)
         {
            this.Log().Error(ex, $"Failed to load {typeof(TData).Name} snapshot, falling back to full rebuild");
         }
      }

      return LoadAllFromJsonAndSaveSnapshot();
   }

   public List<ScannedFile> ScanFiles()
   {
      if(!Directory.Exists(_directory)) return [];

      return new DirectoryInfo(_directory)
            .EnumerateFiles("*.json", SearchOption.AllDirectories)
            .Select(fi => new ScannedFile(fi.FullName, Guid.Parse(Path.GetFileNameWithoutExtension(fi.FullName)), fi.LastWriteTimeUtc))
            .ToList();
   }

   public void SaveSnapshot(List<TData> data)
   {
      EnsureDirectoryExists(_directory);
      using var stream = File.Create(SnapshotPath);
      MemoryPackSerializer.SerializeAsync(stream, data).GetAwaiter().GetResult();
   }

   public List<TData> LoadSnapshot()
   {
      using var stream = File.OpenRead(SnapshotPath);
      return MemoryPackSerializer.DeserializeAsync<List<TData>>(stream).GetAwaiter().GetResult()
          ?? throw new InvalidOperationException($"{typeof(TData).Name} snapshot deserialization returned null");
   }

   List<TData> LoadWithSnapshotMerge()
   {
      var snapshotTimestamp = File.GetLastWriteTimeUtc(SnapshotPath);
      var snapshot = LoadSnapshot();
      var files = ScanFiles();
      var changes = FindChangesSinceSnapshot(snapshot, files, snapshotTimestamp);

      if(changes.HasChanges)
      {
         snapshot = PatchSnapshotWithChanges(snapshot, changes);
         SaveSnapshot(snapshot);
      }

      return snapshot;
   }

   static SnapshotChanges FindChangesSinceSnapshot(List<TData> snapshot, List<ScannedFile> files, DateTime snapshotTimestamp)
   {
      var currentIds = files.Select(f => f.Id).ToHashSet();
      var changedFiles = files.Where(f => f.LastWriteUtc > snapshotTimestamp).ToList();
      var deletedCount = snapshot.Count(d => !currentIds.Contains(d.Id));

      return new SnapshotChanges(changedFiles, currentIds, deletedCount);
   }

   List<TData> PatchSnapshotWithChanges(List<TData> snapshot, SnapshotChanges changes)
   {
      var map = snapshot.ToDictionary(d => d.Id);

      foreach(var file in changes.ChangedFiles)
      {
         var json = File.ReadAllText(file.Path);
         var data = JsonSerializer.Deserialize<TData>(json, _jsonOptions)
                 ?? throw new JsonException($"Failed to deserialize {typeof(TData).Name} from {file.Path}");
         map[data.Id] = data;
      }

      foreach(var id in map.Keys.Where(id => !changes.CurrentIds.Contains(id)).ToList())
         map.Remove(id);

      return map.Values.ToList();
   }

   List<TData> LoadAllFromJsonAndSaveSnapshot()
   {
      var files = ScanFiles();
      var items = files.Select(f =>
      {
         var json = File.ReadAllText(f.Path);
         return JsonSerializer.Deserialize<TData>(json, _jsonOptions)
             ?? throw new JsonException($"Failed to deserialize {typeof(TData).Name} from {f.Path}");
      }).ToList();

      SaveSnapshot(items);
      return items;
   }

   static string Bucket(Guid id) => id.ToString("N")[..2];

   string FilePath(Guid id) => Path.Combine(_directory, Bucket(id), $"{id}.json");

   void EnsureDirectoryExists(string directory)
   {
      if(_ensuredDirectories.Add(directory))
         Directory.CreateDirectory(directory);
   }

   class SnapshotChanges(List<ScannedFile> changedFiles, HashSet<Guid> currentIds, int deletedCount)
   {
      public List<ScannedFile> ChangedFiles { get; } = changedFiles;
      public HashSet<Guid> CurrentIds { get; } = currentIds;
      int DeletedCount { get; } = deletedCount;
      public bool HasChanges => ChangedFiles.Count + DeletedCount > 0;
   }

   public class ScannedFile(string path, Guid id, DateTime lastWriteUtc)
   {
      public string Path { get; } = path;
      public Guid Id { get; } = id;
      public DateTime LastWriteUtc { get; } = lastWriteUtc;
   }
}
