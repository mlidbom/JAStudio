using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.Anki.BulkLoaderTests;

public class for_card_studying_statuses : SpecificationReadingAnAnkiCollectionFile
{
   public class after_fetching_all : for_card_studying_statuses
   {
      readonly CardStudyingStatus[] _statuses;

      public after_fetching_all() => _statuses = [.. WhatTheCollectionLoadsOfTheStudyNotesIn(AnkiTestDb.Database).StudyingStatuses];

      [XF] public void the_result_is_not_empty() => _statuses.Must().NotBeEmpty();

      [XF] public void it_includes_vocab_statuses() =>
         _statuses.Select(s => s.NoteTypeName).Distinct().Must().Contain(NoteTypes.Vocab);

      [XF] public void it_includes_kanji_statuses() =>
         _statuses.Select(s => s.NoteTypeName).Distinct().Must().Contain(NoteTypes.Kanji);

      [XF] public void it_includes_sentence_statuses() =>
         _statuses.Select(s => s.NoteTypeName).Distinct().Must().Contain(NoteTypes.Sentence);

      [XF] public void every_status_has_a_positive_external_note_id() =>
         _statuses.Must().Satisfy(it => it.All(s => s.ExternalNoteId > 0));

      [XF] public void every_status_has_a_card_type() =>
         _statuses.Must().Satisfy(it => it.All(s => !string.IsNullOrEmpty(s.CardType)));

      [XF] public void every_status_has_a_note_type_name() =>
         _statuses.Must().Satisfy(it => it.All(s => !string.IsNullOrEmpty(s.NoteTypeName)));
   }
}
