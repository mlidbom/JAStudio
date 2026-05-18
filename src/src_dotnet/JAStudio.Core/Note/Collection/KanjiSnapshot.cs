using System.Linq;

namespace JAStudio.Core.Note.Collection;

class KanjiSnapshot : CachedNote
{
   public string[] Radicals { get; }
   public string[] Readings { get; }

   public KanjiSnapshot(KanjiNote note) : base(note)
   {
      Radicals = note.Radicals.Distinct().ToArray();
      Readings = note.ReadingsClean.Distinct().ToArray();
   }
}
