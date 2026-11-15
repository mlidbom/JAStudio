using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Specifications.Anki;
using JAStudio.Core.Specifications.Anki.BulkLoaderTests;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki;

/// <summary>
/// The test collection, read from its file (<see cref="AnkiCollectionFile"/>) with the SELECTs JAStudio inside Anki reads the collection Anki has<br/>
/// open with. It has 1,217 study-notes, 253 vocab, 685 kanji and 279 sentences, every one with tags, and 1,749 cards, none suspended: a vocab's<br/>
/// are of its note type's templates Reading and Listening, a kanji's of Reading only, a sentence's of Listening and Reading. Its one other note<br/>
/// type, <c>_Basic</c>, has no notes.
/// </summary>
public class When_reading_the_test_collection_from_its_file : SpecificationReadingAnAnkiCollectionFile, IAIGeneratedSpec
{
   /// <summary>A vocab, 215, of the note type <c>_Vocab</c>, which has 24 fields.</summary>
   const long AVocabsAnkiNoteId = 215;

   /// <summary>A sentence whose field <c>__parsing_result</c> holds newlines.</summary>
   const long ASentencesAnkiNoteId = 1253683685263;

   public class the_study_notes : When_reading_the_test_collection_from_its_file
   {
      readonly IReadOnlyList<StudyNoteInTheSRS> _studyNotes;

      public the_study_notes() => _studyNotes = TheStudyNotesIn(AnkiTestDb.Database);

      StudyNoteInTheSRS TheStudyNoteWith(long ankiNoteId) => _studyNotes.Single(it => it.StudyNoteId == ankiNoteId);

      IEnumerable<CardStudyingStatus> EveryCardStudyingStatus => _studyNotes.SelectMany(it => it.CardStudyingStatuses);

      IEnumerable<string> CardTypesOfTheStudyNotesOf(string noteType) => EveryCardStudyingStatus.Where(it => it.NoteTypeName == noteType).Select(it => it.CardType).Distinct().Order();

      [XF] public void every_study_note_is_read() => _studyNotes.Count.Must().Be(1217);
      [XF] public void every_study_note_holds_the_id_of_its_corpus_note() => _studyNotes.Count(it => it.NoteId != null).Must().Be(1217);
      [XF] public void a_vocabs_study_note_holds_a_vocabs_id() => TheStudyNoteWith(AVocabsAnkiNoteId).NoteId.Must().Satisfy(it => it is VocabId);
      [XF] public void a_study_note_has_every_field_of_its_note_type() => TheStudyNoteWith(AVocabsAnkiNoteId).Fields.Count.Must().Be(24);
      [XF] public void a_fields_newlines_are_kept() => TheStudyNoteWith(ASentencesAnkiNoteId).Fields["__parsing_result"].Must().Contain("\n");
      [XF] public void a_study_notes_tags_are_each_tag_it_has_in_Anki() =>
         TheStudyNoteWith(AVocabsAnkiNoteId).Tags.Must().SequenceEqual(["-::vocab::has-no-studying-sentences", "legacy_scheduling", "source::wani::level::04"]);
      [XF] public void every_card_has_a_studying_status() => EveryCardStudyingStatus.Count().Must().Be(1749);
      [XF] public void a_cards_studying_status_has_the_id_of_its_study_note() =>
         TheStudyNoteWith(AVocabsAnkiNoteId).CardStudyingStatuses.Select(it => it.StudyNoteId).Distinct().Must().SequenceEqual([AVocabsAnkiNoteId]);
      [XF] public void no_card_is_suspended() => EveryCardStudyingStatus.Count(it => it.IsSuspended).Must().Be(0);
      [XF] public void the_vocabs_cards_are_of_the_templates_Listening_and_Reading() => CardTypesOfTheStudyNotesOf(NoteTypes.Vocab).Must().SequenceEqual([CardTypes.Listening, CardTypes.Reading]);
      [XF] public void the_kanjis_cards_are_of_the_template_Reading() => CardTypesOfTheStudyNotesOf(NoteTypes.Kanji).Must().SequenceEqual([CardTypes.Reading]);
      [XF] public void the_sentences_cards_are_of_the_templates_Listening_and_Reading() => CardTypesOfTheStudyNotesOf(NoteTypes.Sentence).Must().SequenceEqual([CardTypes.Listening, CardTypes.Reading]);
   }

   public class the_note_types_for_raw_notes : When_reading_the_test_collection_from_its_file
   {
      readonly IReadOnlyList<AnkiNoteType> _noteTypes = NoteTypesInAnki.ReadFrom(AnkiTestDb.Database).ForRawNotes();

      [XF] public void are_all_note_types_but_JAStudios_own() => _noteTypes.Select(it => it.Name).Must().SequenceEqual(["_Basic"]);
      [XF] public void have_their_fields_in_order() => _noteTypes.Single().FieldNames.Must().SequenceEqual(["Q", "A", "Add Reverse", "URL", "Audio"]);
   }
}
