using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.TestUtils;
using JAStudio.UI.Menus;
using JAStudio.UI.ViewModels.Studying;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Studying;

/// <summary>
/// The Studying window, opened as from the workspace's main menu, in a collection loaded as at startup. The sentences 家に帰る and 駅に行く each<br/>
/// have a study-note in the pretend Anki: that of 家に帰る has another question there than JAStudio writes. An Anki note of one of JAStudio's<br/>
/// note types, with the question 孤児, holds no corpus note's id. The specification renders the window.
/// </summary>
public class When_opening_the_studying_window : SpecificationOpeningWindows, IAIGeneratedSpec
{
   protected readonly PretendAnki Anki;
   readonly Window _window;

   public When_opening_the_studying_window()
   {
      var sentenceAboutHome = WithAStudyNote(CreateTestSentence("家に帰る", "Go home"));
      WithAStudyNote(CreateTestSentence("駅に行く", "Go to the station"));
      Anki = GetService<PretendAnki>();
      Anki.ChangeTheFieldOfTheStudyNoteOf(sentenceAboutHome, AnkiFieldNames.Question, "家");
      Anki.AddAStudyNoteThatHoldsNoCorpusNoteId(new Dictionary<string, string> { [AnkiFieldNames.Question] = "孤児", [AnkiFieldNames.JasNoteId] = "" });
      GetService<JPCollection>().ReloadFromBackend();

      _window = Open(() => MainMenuWindows.Studying.Open(Services));
      Render(_window, "StudyingWindow");
   }

   StudyingWindowViewModel ViewModel => ViewModelOf<StudyingWindowViewModel>(_window);

   void PressSync() => OnTheUIThread(() => ViewModel.SyncCommand.Execute(null));

   [XF] public void it_says_that_no_sync_has_been_made() => OnTheUIThread(() => ViewModel.WhatTheLastSyncDid).Must().SequenceEqual(["No sync has been made since JAStudio started."]);
   [XF] public void it_lists_no_study_notes_of_no_note() => OnTheUIThread(() => ViewModel.StudyNotesOfNoNote.Count).Must().Be(0);

   public class then_pressing_Sync : When_opening_the_studying_window
   {
      public then_pressing_Sync()
      {
         PressSync();
         WaitUntil(() => ViewModel is { IsSyncing: false, WhatTheLastSyncDid.Count: > 1 });
         Render(_window, "StudyingWindowAfterASync");
      }

      /// <summary>What the last sync did, without its first line, which says when it finished.</summary>
      List<string> WhatTheSyncDid => OnTheUIThread(() => ViewModel.WhatTheLastSyncDid.Skip(1).ToList());

      [XF] public void it_says_what_the_sync_did() =>
         WhatTheSyncDid.Must().SequenceEqual([
            "Study-notes read from Anki: 3",
            "Study-notes written, since they differed from what JAStudio writes: 1",
            "Notes whose study-note had changed in Anki, in a card's studying status, in its tags, or in being new: 0",
            "Notes whose study-note is gone from Anki: 0",
            "Study-notes in Anki that hold no corpus note's id: 1",
            "Study-notes in Anki that hold the id of a note JAStudio hasn't: 0"
         ]);

      [XF] public void it_lists_the_Anki_note_that_holds_no_corpus_note_id_saying_so() =>
         OnTheUIThread(() => ViewModel.StudyNotesOfNoNote.Select(it => (it.Question, it.WhyItBelongsToNoNote)).ToList()).Must().SequenceEqual([("孤児", "It holds no corpus note's id.")]);

      [XF] public void the_question_of_the_sentence_about_home_is_in_Anki_as_JAStudio_writes_it() =>
         Anki.ReadAll().Select(it => it.Fields[AnkiFieldNames.Question]).Order().Must().SequenceEqual(new[] { "家に帰る", "駅に行く", "孤児" }.Order());
   }

   public class then_pressing_Sync_while_Ankis_own_sync_is_under_way : When_opening_the_studying_window
   {
      public then_pressing_Sync_while_Ankis_own_sync_is_under_way()
      {
         Anki.StartItsOwnSync();
         PressSync();
      }

      [XF] public void it_says_that_the_sync_cannot_run_since_Anki_is_syncing() => OnTheUIThread(() => ViewModel.WhyTheSyncDidNotRun).Must().Be("The sync can't run now: Anki is syncing.");
      [XF] public void it_still_says_that_no_sync_has_been_made() => OnTheUIThread(() => ViewModel.WhatTheLastSyncDid).Must().SequenceEqual(["No sync has been made since JAStudio started."]);
   }
}
