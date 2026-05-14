using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.SysUtils.Collections.Generic;

namespace JAStudio.Core.Note.Collection;

class KanjiCache : NoteCache<KanjiNote, KanjiSnapshot>
{
   readonly Dictionary<string, HashSet<KanjiNote>> _byRadical = new();
   readonly Dictionary<string, HashSet<KanjiNote>> _byReading = new();

   public KanjiCache(NoteServices noteServices) : base((services, data) => new KanjiNote(services, KanjiData.FromAnkiNoteData(data)), noteServices) {}

   protected override KanjiNote CreateNoteByMergingAnkiData(NoteServices services, KanjiNote existing, NoteData ankiData)
   {
      var mergedData = KanjiNoteConverter.ToCorpusData(existing).MergeAnkiData(ankiData);
      return new KanjiNote(services, mergedData);
   }

   protected override void ClearDerivedIndexes()
   {
      _byRadical.Clear();
      _byReading.Clear();
   }

   protected override NoteId CreateTypedId(Guid value) => new KanjiId(value);

   protected override KanjiSnapshot CreateSnapshot(KanjiNote note) => new(note);

   protected override void InheritorRemoveFromCache(KanjiNote note, KanjiSnapshot snapshot)
   {
      _byRadical.RemoveFromSets(snapshot.Radicals, note);
      _byReading.RemoveFromSets(snapshot.Readings, note);
   }

   protected override void InheritorAddToCache(KanjiNote note, KanjiSnapshot snapshot)
   {
      _byRadical.AddToSets(snapshot.Radicals, note);
      _byReading.AddToSets(snapshot.Readings, note);
   }

   public List<KanjiNote> WithRadical(string radical) =>
      _monitor.Read(() => _byRadical.TryGetValue(radical, out var notes) ? notes.ToList() : []);

   public HashSet<KanjiNote> WithReading(string reading) =>
      _monitor.Read(() => _byReading.TryGetValue(reading, out var notes) ? notes.ToHashSet() : []);
}
