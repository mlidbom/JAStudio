using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Anki.AnkisFunctions.OverHttp;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Anki.OverHttp;

/// <summary>
/// Anki is a program beside JAStudio, which starts, ends and opens another profile on its own. So JAStudio (<see cref="AnkiOverHttp"/>) looks for<br/>
/// the Anki that serves it at each call, in the access file, and works with one of Anki's collections at a time: the one Anki had open at its<br/>
/// first call, until it takes the one Anki has open instead.
/// </summary>
/// <remarks>
/// The access file here is one the specification writes (<see cref="AnAccessFileASpecificationWrites"/>), as the user's own is written by each<br/>
/// Anki that starts serving. An Anki is the add-on's own serving of a copy of the test collection (<see cref="AnkiServingACopyOfTheTestCollection"/>),<br/>
/// whose 253 vocab the Anki search <c>note:_Vocab</c> finds.
/// </remarks>
[NCrunch.Framework.RequiresCapability("PYTHON_VENV")]
public class When_JAStudio_looks_for_the_Anki_that_serves_it : IDisposable, IAIGeneratedSpec
{
   const string EveryVocab = "note:_Vocab";

   readonly AnAccessFileASpecificationWrites _theAccessFile = new();
   readonly AnkiServingACopyOfTheTestCollection _anki = AnkisHttpServersForSpecifications.StartOne();
   readonly AnkiOverHttp _ankiOverHttp;

   public When_JAStudio_looks_for_the_Anki_that_serves_it() => _ankiOverHttp = new AnkiOverHttp(_theAccessFile.AccessFile);

   public virtual void Dispose()
   {
      _ankiOverHttp.Dispose();
      _anki.Dispose();
      _theAccessFile.Dispose();
   }

   int TheNumberOfVocabACallFinds() => _ankiOverHttp.Notes.Find(EveryVocab).Count;

   string WhyACallFailsForWantOfAnAnki() => Invoking(() => _ankiOverHttp.Notes.Find(EveryVocab)).Must().Throw<NoAnkiServesJAStudioException>().Which.Why;

   public class before_any_Anki_has_started_serving : When_JAStudio_looks_for_the_Anki_that_serves_it
   {
      [XF] public void a_call_fails_saying_that_Anki_is_not_running_or_has_no_profile_open() => WhyACallFailsForWantOfAnAnki().Must().Be("Anki isn't running, or has no profile open.");
      [XF] public void JAStudio_is_told_beforehand_why_a_call_would_fail() => _ankiOverHttp.WhyItCannotBeCalledNow.Must().Be("Anki isn't running, or has no profile open.");
      [XF] public void JAStudio_works_with_no_collection_yet() => _ankiOverHttp.CollectionJAStudioWorksWith.Must().BeNull();
   }

   public class once_an_Anki_serves : When_JAStudio_looks_for_the_Anki_that_serves_it
   {
      public once_an_Anki_serves() => _theAccessFile.Names(_anki);

      [XF] public void JAStudio_is_told_that_a_call_would_be_answered() => _ankiOverHttp.WhyItCannotBeCalledNow.Must().BeNull();
      [XF] public void JAStudio_works_with_no_collection_before_its_first_call() => _ankiOverHttp.CollectionJAStudioWorksWith.Must().BeNull();
      [XF] public void Anki_has_no_other_collection_open_than_the_one_JAStudio_works_with() => _ankiOverHttp.TheOtherCollectionAnkiHasOpen.Must().BeNull();

      public class after_JAStudios_first_call : once_an_Anki_serves
      {
         public after_JAStudios_first_call() => _ = TheNumberOfVocabACallFinds();

         [XF] public void JAStudio_works_with_the_collection_Anki_has_open() => _ankiOverHttp.CollectionJAStudioWorksWith!.FullName.Must().Be(_anki.Collection.FullName);
         [XF] public void JAStudio_is_still_told_that_a_call_would_be_answered() => _ankiOverHttp.WhyItCannotBeCalledNow.Must().BeNull();

         public class while_Ankis_own_sync_is_under_way : after_JAStudios_first_call
         {
            public while_Ankis_own_sync_is_under_way() => _anki.ItsOwnSyncStarts();

            [XF] public void JAStudio_is_told_beforehand_that_Anki_is_syncing() => _ankiOverHttp.WhyItCannotBeCalledNow.Must().Be("Anki is syncing.");
         }

         public class once_that_Anki_has_stopped_serving : after_JAStudios_first_call
         {
            public once_that_Anki_has_stopped_serving()
            {
               _anki.StopsServing();
               _theAccessFile.IsRemoved();
            }

            [XF] public void a_call_fails_saying_that_Anki_is_not_running_or_has_no_profile_open() => WhyACallFailsForWantOfAnAnki().Must().Be("Anki isn't running, or has no profile open.");
            [XF] public void JAStudio_still_works_with_the_collection_that_Anki_had_open() => _ankiOverHttp.CollectionJAStudioWorksWith!.FullName.Must().Be(_anki.Collection.FullName);
         }

         public class once_that_Anki_has_ended_without_stopping_its_serving_so_that_the_access_file_still_names_it : after_JAStudios_first_call
         {
            public once_that_Anki_has_ended_without_stopping_its_serving_so_that_the_access_file_still_names_it() => _anki.StopsServing();

            [XF] public void a_call_fails_saying_that_the_Anki_no_longer_answers() =>
               WhyACallFailsForWantOfAnAnki().Must().Be("The Anki that served JAStudio no longer answers: it ended without saying so.");

            [XF] public void JAStudio_is_told_beforehand_that_the_Anki_no_longer_answers() =>
               _ankiOverHttp.WhyItCannotBeCalledNow.Must().Be("The Anki that served JAStudio no longer answers: it ended without saying so.");
         }

         public class once_an_Anki_with_another_collection_open_serves_instead_as_when_Anki_opens_another_profile : after_JAStudios_first_call
         {
            readonly AnkiServingACopyOfTheTestCollection _ankiWithAnotherCollectionOpen = AnkisHttpServersForSpecifications.StartOne();

            public once_an_Anki_with_another_collection_open_serves_instead_as_when_Anki_opens_another_profile()
            {
               _anki.StopsServing();
               _theAccessFile.Names(_ankiWithAnotherCollectionOpen);
            }

            public override void Dispose()
            {
               base.Dispose();
               _ankiWithAnotherCollectionOpen.Dispose();
            }

            [XF] public void a_call_is_refused_naming_the_collection_Anki_has_open() =>
               Invoking(() => _ankiOverHttp.Notes.Find(EveryVocab)).Must().Throw<AnkiRefusedTheCallException>()
                                                                   .Which.CollectionAnkiHasOpen.Must().Be(_ankiWithAnotherCollectionOpen.Collection.FullName);

            [XF] public void JAStudio_is_told_beforehand_that_Anki_has_another_collection_open_and_which() =>
               _ankiOverHttp.WhyItCannotBeCalledNow.Must().Be($"Anki has another collection open than the one JAStudio works with: {_ankiWithAnotherCollectionOpen.Collection.FullName}");

            [XF] public void the_other_collection_Anki_has_open_is_that_Ankis() =>
               _ankiOverHttp.TheOtherCollectionAnkiHasOpen!.FullName.Must().Be(_ankiWithAnotherCollectionOpen.Collection.FullName);

            [XF] public void JAStudio_still_works_with_the_collection_the_first_Anki_had_open() => _ankiOverHttp.CollectionJAStudioWorksWith!.FullName.Must().Be(_anki.Collection.FullName);

            public class after_JAStudio_takes_the_collection_Anki_has_open : once_an_Anki_with_another_collection_open_serves_instead_as_when_Anki_opens_another_profile
            {
               public after_JAStudio_takes_the_collection_Anki_has_open() => _ankiOverHttp.TakeTheCollectionAnkiHasOpen();

               [XF] public void JAStudio_works_with_that_collection() => _ankiOverHttp.CollectionJAStudioWorksWith!.FullName.Must().Be(_ankiWithAnotherCollectionOpen.Collection.FullName);
               [XF] public void a_call_is_answered() => TheNumberOfVocabACallFinds().Must().Be(253);
               [XF] public void JAStudio_is_told_that_a_call_would_be_answered_again() => _ankiOverHttp.WhyItCannotBeCalledNow.Must().BeNull();
            }
         }
      }
   }
}
