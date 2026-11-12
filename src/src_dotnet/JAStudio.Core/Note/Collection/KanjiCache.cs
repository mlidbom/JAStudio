using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Collection;

class KanjiCache : NoteCache<KanjiNote, KanjiSnapshot>
{
   readonly NoteIndex<string, KanjiNote, KanjiSnapshot> _byRadical;
   readonly NoteIndex<string, KanjiNote, KanjiSnapshot> _byReading;

   public KanjiCache(NoteServices noteServices) : base(noteServices)
   {
      _byRadical = NewIndex(snapshot => snapshot.Radicals);
      _byReading = NewIndex(snapshot => snapshot.Readings);
   }

   protected override KanjiSnapshot CreateSnapshot(KanjiNote note) => new(note);

   public List<KanjiNote> WithRadical(string radical) => _byRadical.NotesWith(radical);

   public HashSet<KanjiNote> WithReading(string reading) => _byReading.NotesWith(reading).ToHashSet();
}
