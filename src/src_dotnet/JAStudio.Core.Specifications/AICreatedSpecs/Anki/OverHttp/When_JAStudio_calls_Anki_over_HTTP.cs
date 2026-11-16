using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki;
using JAStudio.Anki.AnkisFunctions;
using JAStudio.Anki.AnkisFunctions.OverHttp;
using JAStudio.Anki.CollectionDatabase;
using JAStudio.Core.Anki;
using JAStudio.Core.Studying.StudyNotes;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// JAStudio calling Anki over HTTP (<see cref="AnkiOverHttp"/>), as a JAStudio that is a program of its own will. Each specification calls the<br/>
/// Anki add-on's own HTTP server, Python's <c>HttpServerForJAStudio</c>, serving a copy of the test collection that no other specification sees<br/>
/// (<see cref="AnkiServingACopyOfTheTestCollection"/>). So a function whose name, arguments or answer differ between the two sides fails here.
/// </summary>
/// <remarks>
/// What a call did is read back as JAStudio reads the collection Anki has open: through Anki's own connection (<see cref="AnkisOwnConnection"/>),<br/>
/// here over HTTP too.
/// </remarks>
[NCrunch.Framework.RequiresCapability("PYTHON_VENV")]
public class When_JAStudio_calls_Anki_over_HTTP : IDisposable, IAIGeneratedSpec
{
   /// <summary>A vocab, of the note type <c>_Vocab</c>, which has 24 fields. It has two cards, and the tags <see cref="TheTagsTheVocabHasInTheTestCollection"/>.</summary>
   const long AVocabsAnkiNoteId = 215;

   /// <summary>A sentence whose field <c>__parsing_result</c> holds newlines.</summary>
   const long ASentencesAnkiNoteId = 1253683685263;

   /// <summary>The Anki search that finds the test collection's 253 vocab.</summary>
   const string EveryVocab = "note:_Vocab";

   static readonly string[] TheTagsTheVocabHasInTheTestCollection = ["-::vocab::has-no-studying-sentences", "legacy_scheduling", "source::wani::level::04"];

   readonly AnkiServingACopyOfTheTestCollection _anki = AnkisHttpServersForSpecifications.StartOne();
   readonly AnkiOverHttp _ankiOverHttp;

   public When_JAStudio_calls_Anki_over_HTTP() => _ankiOverHttp = new AnkiOverHttp(_anki.AccessFile);

   public virtual void Dispose()
   {
      _ankiOverHttp.Dispose();
      _anki.Dispose();
   }

   IAnkisNotes Notes => _ankiOverHttp.Notes;
   IAnkisTags Tags => _ankiOverHttp.Tags;
   IAnkisMedia Media => _ankiOverHttp.Media;
   IAnkisWindows Windows => _ankiOverHttp.Windows;

   /// <summary>Every study-note in the collection Anki has open, as JAStudio reads them: through Anki's own connection, over HTTP.</summary>
   IReadOnlyList<StudyNoteInTheSRS> TheStudyNotes() => StudyNotesInAnkisDatabase.ReadAll(new AnkisOwnConnection(_ankiOverHttp.Database));

   StudyNoteInTheSRS TheVocabsStudyNote() => TheStudyNotes().Single(it => it.StudyNoteId == AVocabsAnkiNoteId);

   string[] TheVocabsTags() => [.. TheVocabsStudyNote().Tags.Order(StringComparer.Ordinal)];

   bool[] WhetherEachCardOfTheVocabIsSuspended() => [.. TheVocabsStudyNote().CardStudyingStatuses.Select(it => it.IsSuspended)];

   /// <summary>How <paramref name="call"/> fails, which it must, with what Anki answered.</summary>
   static (HttpStatusCode Status, string Error) HowAnkiFails(Action call)
   {
      var failure = Invoking(call).Must().Throw<CallToAnkiFailedException>().Which;
      return (failure.Status, failure.Error);
   }

   [XF] public void a_search_in_Ankis_search_syntax_finds_the_ids_of_the_notes_it_finds_in_Anki() =>
      Notes.Find(EveryVocab).Must().Satisfy(found => found.Count == 253 && found.Contains(AVocabsAnkiNoteId));

   [XF] public void the_notes_with_a_tag_or_a_tag_below_it_are_counted() => Tags.NumberOfNotesWithATagIn("source::wani").Must().Be(938);

   [XF] public void a_function_that_fails_in_Anki_fails_the_call_with_Pythons_traceback() =>
      HowAnkiFails(() => Notes.WriteTheFields(AVocabsAnkiNoteId, new Dictionary<string, string> { ["a field the note type has not"] = "x" }))
        .Must().Satisfy(it => it.Status == HttpStatusCode.InternalServerError && it.Error.Contains("KeyError"));

   public class reading_the_study_notes_through_Ankis_own_connection : When_JAStudio_calls_Anki_over_HTTP
   {
      readonly IReadOnlyList<StudyNoteInTheSRS> _studyNotes;

      public reading_the_study_notes_through_Ankis_own_connection() => _studyNotes = TheStudyNotes();

      [XF] public void every_study_note_is_read() => _studyNotes.Count.Must().Be(1217);
      [XF] public void every_card_has_a_studying_status() => _studyNotes.SelectMany(it => it.CardStudyingStatuses).Count().Must().Be(1749);
      [XF] public void a_fields_newlines_are_kept() => _studyNotes.Single(it => it.StudyNoteId == ASentencesAnkiNoteId).Fields["__parsing_result"].Must().Contain("\n");
      [XF] public void a_study_notes_tags_are_each_tag_it_has_in_Anki() =>
         _studyNotes.Single(it => it.StudyNoteId == AVocabsAnkiNoteId).Tags.Must().SequenceEqual(TheTagsTheVocabHasInTheTestCollection);
   }

   public class after_giving_a_note_a_tag : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_giving_a_note_a_tag() => Notes.AddTag([AVocabsAnkiNoteId], "jastudio::imported");

      [XF] public void the_note_has_the_tag_beside_those_it_had() =>
         TheVocabsTags().Must().SequenceEqual(["-::vocab::has-no-studying-sentences", "jastudio::imported", "legacy_scheduling", "source::wani::level::04"]);
   }

   public class after_writing_one_field_of_a_note : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_writing_one_field_of_a_note() => Notes.WriteTheFields(AVocabsAnkiNoteId, new Dictionary<string, string> { [AnkiFieldNames.Question] = "勉強する" });

      [XF] public void the_field_holds_what_was_written_with_its_Japanese_whole() => TheVocabsStudyNote().Fields[AnkiFieldNames.Question].Must().Be("勉強する");
      [XF] public void the_note_has_every_field_of_its_note_type_still() => TheVocabsStudyNote().Fields.Count.Must().Be(24);
      [XF] public void the_notes_tags_are_as_they_were() => TheVocabsTags().Must().SequenceEqual(TheTagsTheVocabHasInTheTestCollection);
   }

   public class after_suspending_every_card_of_a_note : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_suspending_every_card_of_a_note() => Notes.SuspendAllCardsOf(AVocabsAnkiNoteId);

      [XF] public void both_cards_of_the_note_are_suspended() => WhetherEachCardOfTheVocabIsSuspended().Must().SequenceEqual([true, true]);
      [XF] public void no_other_card_is_suspended() => TheStudyNotes().SelectMany(it => it.CardStudyingStatuses).Count(it => it.IsSuspended).Must().Be(2);

      public class and_unsuspending_them : after_suspending_every_card_of_a_note
      {
         public and_unsuspending_them() => Notes.UnsuspendAllCardsOf(AVocabsAnkiNoteId);

         [XF] public void neither_card_of_the_note_is_suspended() => WhetherEachCardOfTheVocabIsSuspended().Must().SequenceEqual([false, false]);
      }
   }

   public class after_deleting_a_note : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_deleting_a_note() => Notes.Delete(AVocabsAnkiNoteId);

      [XF] public void a_search_finds_every_note_it_found_but_that_one() => Notes.Find(EveryVocab).Must().Satisfy(found => found.Count == 252 && !found.Contains(AVocabsAnkiNoteId));
   }

   public class after_deleting_two_notes_in_one_call : When_JAStudio_calls_Anki_over_HTTP
   {
      readonly List<long> _theTwoDeleted;

      public after_deleting_two_notes_in_one_call()
      {
         _theTwoDeleted = [.. Notes.Find(EveryVocab).Take(2)];
         Notes.Delete(_theTwoDeleted);
      }

      [XF] public void a_search_finds_every_note_it_found_but_those_two() => Notes.Find(EveryVocab).Must().Satisfy(found => found.Count == 251 && !found.Intersect(_theTwoDeleted).Any());
   }

   public class after_adding_a_tag_to_a_note_as_an_Anki_operation : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_adding_a_tag_to_a_note_as_an_Anki_operation() => Tags.AddToNotes([AVocabsAnkiNoteId], "flagged");

      [XF] public void the_note_has_the_tag_beside_those_it_had() =>
         TheVocabsTags().Must().SequenceEqual(["-::vocab::has-no-studying-sentences", "flagged", "legacy_scheduling", "source::wani::level::04"]);
   }

   public class after_removing_a_tag_from_a_note : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_removing_a_tag_from_a_note() => Tags.RemoveFromNotes([AVocabsAnkiNoteId], "legacy_scheduling");

      [XF] public void the_note_has_its_other_tags_only() => TheVocabsTags().Must().SequenceEqual(["-::vocab::has-no-studying-sentences", "source::wani::level::04"]);
   }

   public class after_removing_from_a_note_a_tag_of_which_it_only_has_a_tag_below : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_removing_from_a_note_a_tag_of_which_it_only_has_a_tag_below() => Tags.RemoveFromNotes([AVocabsAnkiNoteId], "source::wani");

      [XF] public void the_note_keeps_the_tag_below_it() => TheVocabsTags().Must().SequenceEqual(TheTagsTheVocabHasInTheTestCollection);
   }

   public class after_renaming_a_tag : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_renaming_a_tag() => Tags.Rename("source::wani", "source::wanikani");

      [XF] public void a_note_with_a_tag_below_it_has_that_tag_below_the_new_name() =>
         TheVocabsTags().Must().SequenceEqual(["-::vocab::has-no-studying-sentences", "legacy_scheduling", "source::wanikani::level::04"]);

      [XF] public void no_note_has_a_tag_in_the_old_name() => Tags.NumberOfNotesWithATagIn("source::wani").Must().Be(0);
      [XF] public void every_note_that_had_a_tag_in_the_old_name_has_one_in_the_new() => Tags.NumberOfNotesWithATagIn("source::wanikani").Must().Be(938);
   }

   public class after_deleting_a_tag : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_deleting_a_tag() => Tags.Delete("source::wani");

      [XF] public void no_note_has_it_or_a_tag_below_it() => Tags.NumberOfNotesWithATagIn("source::wani").Must().Be(0);
      [XF] public void a_note_that_had_a_tag_below_it_has_its_other_tags_only() => TheVocabsTags().Must().SequenceEqual(["-::vocab::has-no-studying-sentences", "legacy_scheduling"]);
   }

   public class after_adding_a_file_to_Ankis_media : When_JAStudio_calls_Anki_over_HTTP
   {
      static readonly byte[] WhatTheFileHolds = "ID3 a recording"u8.ToArray();
      readonly string _itsNameInAnki;

      public after_adding_a_file_to_Ankis_media()
      {
         var file = Path.Combine(_anki.Folder.FullName, "recording.mp3");
         File.WriteAllBytes(file, WhatTheFileHolds);
         _itsNameInAnki = Media.AddFile(file);
      }

      [XF] public void Anki_keeps_it_under_the_files_own_name() => _itsNameInAnki.Must().Be("recording.mp3");
      [XF] public void Ankis_media_folder_holds_a_copy_of_the_file() => File.ReadAllBytes(Path.Combine(_anki.Folder.FullName, Media.Folder, _itsNameInAnki)).Must().SequenceEqual(WhatTheFileHolds);
   }

   public class after_asking_Anki_to_open_its_browser_on_a_search : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_asking_Anki_to_open_its_browser_on_a_search() => Windows.OpenTheBrowserOn("nid:215");

      [XF] public void Ankis_windows_were_asked_to_open_the_browser_on_the_search() => _anki.WhatItsWindowsWereAskedFor().Must().SequenceEqual(["open the browser on: nid:215"]);
   }

   public class after_asking_Anki_to_open_its_browser_and_its_previewer_on_a_search : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_asking_Anki_to_open_its_browser_and_its_previewer_on_a_search() => Windows.OpenTheBrowserAndThePreviewerOn("nid:215");

      [XF] public void Ankis_windows_were_asked_to_open_the_browser_and_the_previewer_on_the_search() =>
         _anki.WhatItsWindowsWereAskedFor().Must().SequenceEqual(["open the browser and the previewer on: nid:215"]);
   }

   public class after_asking_Anki_to_show_a_tooltip_for_five_seconds : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_asking_Anki_to_show_a_tooltip_for_five_seconds() => Windows.ShowATooltip("Reparsed 3 sentences: 勉強", TimeSpan.FromSeconds(5));

      [XF] public void Ankis_windows_were_asked_to_show_the_message_for_5000_milliseconds_with_its_Japanese_whole() =>
         _anki.WhatItsWindowsWereAskedFor().Must().SequenceEqual(["show a tooltip for 5000 ms: Reparsed 3 sentences: 勉強"]);
   }

   public class after_asking_Anki_to_refresh_its_windows : When_JAStudio_calls_Anki_over_HTTP
   {
      public after_asking_Anki_to_refresh_its_windows() => Windows.Refresh();

      [XF] public void Ankis_windows_were_asked_to_refresh() => _anki.WhatItsWindowsWereAskedFor().Must().SequenceEqual(["refresh"]);
   }

   public class while_Ankis_own_sync_is_under_way : When_JAStudio_calls_Anki_over_HTTP
   {
      public while_Ankis_own_sync_is_under_way() => _anki.ItsOwnSyncStarts();

      [XF] public void a_call_is_refused_saying_that_Anki_is_syncing() =>
         Invoking(() => Notes.Find(EveryVocab)).Must().Throw<AnkiRefusedTheCallException>().Which.Why.Must().Be("Anki is syncing.");

      public class and_once_it_has_finished : while_Ankis_own_sync_is_under_way
      {
         public and_once_it_has_finished() => _anki.ItsOwnSyncFinishes();

         [XF] public void a_call_is_answered_again() => Notes.Find(EveryVocab).Count.Must().Be(253);
      }
   }

   public class with_another_token_than_the_servers : When_JAStudio_calls_Anki_over_HTTP
   {
      readonly AnAccessFileASpecificationWrites _namingAnotherToken = new();
      readonly AnkiOverHttp _withAnotherToken;

      public with_another_token_than_the_servers()
      {
         _namingAnotherToken.Holds(_anki.Access with { Token = "another token" });
         _withAnotherToken = new AnkiOverHttp(_namingAnotherToken.AccessFile);
      }

      public override void Dispose()
      {
         _withAnotherToken.Dispose();
         _namingAnotherToken.Dispose();
         base.Dispose();
      }

      [XF] public void the_first_call_fails_as_unauthorized() => HowAnkiFails(() => _withAnotherToken.Notes.Find(EveryVocab)).Status.Must().Be(HttpStatusCode.Unauthorized);
   }
}
