using System;
using System.Linq;
using System.Threading;
using Compze.Internals.SystemCE.ReactiveCE;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.TestUtils;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki;

/// <summary>
/// What JAStudio does as Anki tells it that a profile opened or is closing, and that Anki's own sync is starting or has finished<br/>
/// (<see cref="FollowingAnkisLifecycle"/>): it keeps track of whether it may use Anki's collection, and it reads the profile that opened. Inside<br/>
/// Anki nothing else loads JAStudio's collection. The sentence 家に帰る has a study-note in Anki, untagged, and JAStudio has just started: its<br/>
/// collection isn't loaded.
/// </summary>
public class When_following_Ankis_lifecycle : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static readonly TimeSpan TimeToWaitForTheRead = TimeSpan.FromSeconds(30);

   /// <summary>Longer than a profile that opened waits before it's read: what hasn't been read by then isn't going to be.</summary>
   static readonly TimeSpan LongerThanAProfileThatOpenedWaitsToBeRead = TimeSpan.FromSeconds(1.5);

   protected readonly JPCollection Collection;
   protected readonly PretendAnki Anki;
   protected readonly AnkisAvailability Availability = new();
   readonly FollowingAnkisLifecycle _followingAnkisLifecycle;
   readonly NoteId _idOfTheSentence;
   int _timesTheCollectionLoaded;

   public When_following_Ankis_lifecycle()
   {
      _idOfTheSentence = WithAStudyNote(CreateTestSentence("家に帰る", "Go home")).GetId();
      Collection = GetService<JPCollection>();
      Anki = GetService<PretendAnki>();
      Collection.ReloadedFromBackend.Subscribe(_ => Interlocked.Increment(ref _timesTheCollectionLoaded));
      _followingAnkisLifecycle = new FollowingAnkisLifecycle(Collection, Availability, GetService<BackgroundTaskManager>());
   }

   protected int TimesTheCollectionLoaded => Volatile.Read(ref _timesTheCollectionLoaded);
   protected SentenceNote TheSentence => Collection.Sentences.WithIdOrNone(_idOfTheSentence) ?? throw new InvalidOperationException("The sentence is gone.");

   protected void AnkiTellsJAStudioOf(AnkiLifecycleEvent lifecycleEvent) => _followingAnkisLifecycle.Handle(lifecycleEvent);
   protected void WaitUntilTheCollectionHasLoaded() => WaitUntil(() => Collection.IsInitialized);
   protected static void WaitLongerThanAProfileThatOpenedWaitsToBeRead() => Thread.Sleep(LongerThanAProfileThatOpenedWaitsToBeRead);

   protected static void WaitUntil(Func<bool> condition) => SpinWait.SpinUntil(condition, TimeToWaitForTheRead).Must().BeTrue();

   [XF] public void before_a_profile_has_opened_JAStudio_may_not_use_Ankis_collection() => Availability.WhyItCannotBeUsedNow.Must().Be("No Anki profile is open.");
   [XF] public void before_a_profile_has_opened_the_collection_is_not_loaded() => Collection.IsInitialized.Must().BeFalse();

   public class after_a_profile_opened : When_following_Ankis_lifecycle
   {
      public after_a_profile_opened()
      {
         AnkiTellsJAStudioOf(AnkiLifecycleEvent.ProfileOpened);
         WaitUntilTheCollectionHasLoaded();
      }

      [XF] public void JAStudio_may_use_Ankis_collection() => Availability.WhyItCannotBeUsedNow.Must().BeNull();
      [XF] public void the_collection_is_loaded_with_the_sentence() => Collection.Sentences.All().Select(it => it.GetQuestion()).Must().SequenceEqual(["家に帰る"]);
      [XF] public void the_sentence_has_its_study_note() => TheSentence.HasAStudyNote().Must().BeTrue();

      public class and_it_then_closed : after_a_profile_opened
      {
         public and_it_then_closed() => AnkiTellsJAStudioOf(AnkiLifecycleEvent.ProfileClosing);

         [XF] public void JAStudio_may_no_longer_use_Ankis_collection() => Availability.WhyItCannotBeUsedNow.Must().Be("No Anki profile is open.");
         [XF] public void the_collection_stays_loaded() => Collection.IsInitialized.Must().BeTrue();

         /// <summary>Another profile's study-notes are its own: here the same study-note, tagged meanwhile, stands in for them.</summary>
         public class and_a_profile_in_which_the_sentences_study_note_is_tagged_then_opened : and_it_then_closed
         {
            public and_a_profile_in_which_the_sentences_study_note_is_tagged_then_opened()
            {
               Anki.Tag(TheSentence, "leech");
               AnkiTellsJAStudioOf(AnkiLifecycleEvent.ProfileOpened);
               WaitUntil(() => TheSentence.AnkiTags.Any());
            }

            [XF] public void the_sentence_has_the_tag_of_its_study_note_in_that_profile() => TheSentence.AnkiTags.Select(it => it.Name).Must().SequenceEqual(["anki::leech"]);
            [XF] public void the_collection_is_not_loaded_a_second_time() => TimesTheCollectionLoaded.Must().Be(1);
         }
      }

      public class and_Ankis_own_sync_then_started : after_a_profile_opened
      {
         public and_Ankis_own_sync_then_started() => AnkiTellsJAStudioOf(AnkiLifecycleEvent.SyncStarting);

         [XF] public void JAStudio_may_not_use_Ankis_collection_since_Anki_is_syncing() => Availability.WhyItCannotBeUsedNow.Must().Be("Anki is syncing.");

         /// <summary>What the sync brought from AnkiWeb, JAStudio reads at the next sync a person starts in JAStudio, not as Anki's sync finishes.</summary>
         public class and_tagged_the_sentences_study_note_before_it_finished : and_Ankis_own_sync_then_started
         {
            public and_tagged_the_sentences_study_note_before_it_finished()
            {
               Anki.Tag(TheSentence, "leech");
               AnkiTellsJAStudioOf(AnkiLifecycleEvent.SyncCompleted);
               WaitLongerThanAProfileThatOpenedWaitsToBeRead();
            }

            [XF] public void JAStudio_may_use_Ankis_collection_again() => Availability.WhyItCannotBeUsedNow.Must().BeNull();
            [XF] public void the_study_notes_are_not_read_again_so_the_sentence_has_no_Anki_tag_yet() => TheSentence.AnkiTags.Select(it => it.Name).Must().BeEmpty();
            [XF] public void the_collection_is_not_loaded_a_second_time() => TimesTheCollectionLoaded.Must().Be(1);
         }
      }
   }

   /// <summary>Anki syncs as a profile opens, when set to: it tells JAStudio of the sync right after the profile.</summary>
   public class after_a_profile_opened_and_Ankis_own_sync_started_at_once : When_following_Ankis_lifecycle
   {
      public after_a_profile_opened_and_Ankis_own_sync_started_at_once()
      {
         AnkiTellsJAStudioOf(AnkiLifecycleEvent.ProfileOpened);
         AnkiTellsJAStudioOf(AnkiLifecycleEvent.SyncStarting);
      }

      public class and_the_sync_is_still_under_way_when_the_profile_would_have_been_read : after_a_profile_opened_and_Ankis_own_sync_started_at_once
      {
         public and_the_sync_is_still_under_way_when_the_profile_would_have_been_read() => WaitLongerThanAProfileThatOpenedWaitsToBeRead();

         [XF] public void the_collection_is_not_loaded() => Collection.IsInitialized.Must().BeFalse();
      }

      public class and_the_sync_then_finished : after_a_profile_opened_and_Ankis_own_sync_started_at_once
      {
         public and_the_sync_then_finished()
         {
            AnkiTellsJAStudioOf(AnkiLifecycleEvent.SyncCompleted);
            WaitUntilTheCollectionHasLoaded();
         }

         [XF] public void the_collection_is_loaded_with_the_sentence() => Collection.Sentences.All().Select(it => it.GetQuestion()).Must().SequenceEqual(["家に帰る"]);
         [XF] public void JAStudio_may_use_Ankis_collection() => Availability.WhyItCannotBeUsedNow.Must().BeNull();

         public class and_the_time_a_profile_that_opened_waits_to_be_read_has_passed : and_the_sync_then_finished
         {
            public and_the_time_a_profile_that_opened_waits_to_be_read_has_passed() => WaitLongerThanAProfileThatOpenedWaitsToBeRead();

            [XF] public void the_collection_is_not_loaded_a_second_time() => TimesTheCollectionLoaded.Must().Be(1);
         }
      }
   }
}
