using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

/// <summary>
/// An Immersion Kit sentence card is checked twice, as an import run window and an automatic run might, and the first run is executed.<br/>
/// The second run's check found the sentence card to import, but the first run imported it meanwhile.
/// </summary>
public class When_another_import_run_was_executed_after_a_check : ImportingSpecification, IAIGeneratedSpec
{
   readonly RawNote _sentenceCard;
   protected readonly ImportRun SecondRun;

   public When_another_import_run_was_executed_after_a_check()
   {
      _sentenceCard = AddImmersionKitSentenceCard("ずっと1人で生きていくんだ", "anime_the_garden_of_words_000000346", "garden_of_words_346.mp3", "garden_of_words_346.jpg");
      var firstRun = Importer.Prepare(ImmersionKitProfile);
      SecondRun = Importer.Prepare(ImmersionKitProfile);
      firstRun.ExecuteUnlessOutOfDate().Must().BeTrue();
   }

   [XF] public void the_run_checked_before_it_is_out_of_date() => SecondRun.IsOutOfDate.Must().BeTrue();

   public class and_the_run_checked_before_it_is_executed : When_another_import_run_was_executed_after_a_check
   {
      readonly bool _executed;
      public and_the_run_checked_before_it_is_executed() => _executed = SecondRun.ExecuteUnlessOutOfDate();

      [XF] public void it_is_not_executed() => _executed.Must().BeFalse();
      [XF] public void the_sentence_card_is_imported_only_once() => Sentences.All().Count.Must().Be(1);
      [XF] public void it_imported_no_sentences() => SecondRun.ImportedSentences.Must().BeEmpty();
   }

   public class and_checked_again : When_another_import_run_was_executed_after_a_check
   {
      readonly ImportRun _runCheckedAgain;
      public and_checked_again() => _runCheckedAgain = Importer.Prepare(ImmersionKitProfile);

      [XF] public void the_run_checked_again_is_not_out_of_date() => _runCheckedAgain.IsOutOfDate.Must().BeFalse();
      [XF] public void it_finds_the_sentence_card_imported_by_an_earlier_run() => CheckOf(_runCheckedAgain, _sentenceCard).Outcome.Must().Be(RawNoteOutcome.ImportedByAnEarlierRun);
   }
}
