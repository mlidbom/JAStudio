using System.Collections.Generic;
using Compze.Threading;

namespace JAStudio.Core.Note;

public class Tag
{
   static readonly IMonitor _monitor = IMonitor.New();
   static readonly Dictionary<int, Tag> ById = new();
   static readonly Dictionary<string, Tag> ByName = new();

   public string Name { get; }
   public int Id { get; }
   public long Bit { get; }

   Tag(string name)
   {
      var id = ById.Count;
      Name = name;
      Id = id;
      Bit = 1L << id;
   }

   public static Tag FromName(string name) => _monitor.Locked(() =>
   {
      if(ByName.TryGetValue(name, out var existing)) return existing;
      var created = new Tag(name);
      ById.Add(created.Id, created);
      ByName.Add(name, created);
      return created;
   });

   public static Tag FromId(int id) => _monitor.Locked(() => ById[id]);
}
