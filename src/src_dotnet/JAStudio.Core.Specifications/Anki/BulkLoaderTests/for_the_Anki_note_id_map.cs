using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Anki.BulkLoaderTests;

/// <summary>The test collection has 253 vocab, 685 kanji and 279 sentence notes, each with a corpus note id in its <c>jas_note_id</c> field.</summary>
public class for_the_Anki_note_id_map : SpecificationReadingAnAnkiCollectionFile
{
   public class after_loading_it_from_the_test_collection : for_the_Anki_note_id_map
   {
      readonly Dictionary<long, NoteId> _corpusNoteIdsByAnkiNoteId;

      public after_loading_it_from_the_test_collection() => _corpusNoteIdsByAnkiNoteId = WhatTheCollectionLoadsOfTheStudyNotesIn(AnkiTestDb.Database).IdMappings;

      [XF] public void every_vocab_note_maps_to_a_vocab_id() => _corpusNoteIdsByAnkiNoteId.Values.OfType<VocabId>().Count().Must().Be(253);
      [XF] public void every_kanji_note_maps_to_a_kanji_id() => _corpusNoteIdsByAnkiNoteId.Values.OfType<KanjiId>().Count().Must().Be(685);
      [XF] public void every_sentence_note_maps_to_a_sentence_id() => _corpusNoteIdsByAnkiNoteId.Values.OfType<SentenceId>().Count().Must().Be(279);
      [XF] public void no_other_note_is_mapped() => _corpusNoteIdsByAnkiNoteId.Count.Must().Be(253 + 685 + 279);
   }
}
