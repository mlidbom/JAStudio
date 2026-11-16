using System.Linq;
using Compze.Contracts.Exceptions;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;
using JAStudio.Core.UI.Web.Sentence;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.ParsingCases;

public class When_a_sentence_is_marked_as_a_parsing_case : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly SentenceNote _sentence;

   protected When_a_sentence_is_marked_as_a_parsing_case()
   {
      CreateVocab("走る", "to-run", "はしる");
      CreateVocab("ている", "is-doing", "ている");
      _sentence = CreateSentence("毎日走っている");
      _sentence.Edit(it => it.MarkAsAParsingCase());
   }

   ParsingCase TheCase => _sentence.ParsingCase!;
   ParsingCaseComparison Comparison => _sentence.CompareWithWhatTheParsingCaseExpects();

   static readonly BreakdownParsingOptions EveryOptionOff = new(false, false, false, false, false);

   /// <summary>The forms of the words the case expects the breakdown to display under <paramref name="options"/>.</summary>
   /// <summary>Each parsing case the collection lists, compared: its sentence, then its outcome.</summary>
   string[] TheParsingCasesComparedByTheCollection => [.. GetService<JPCollection>().Sentences.ParsingCasesCompared().Select(it => $"{it.Sentence.GetQuestion()}: {it.Comparison.Outcome}")];

   /// <summary>
   /// The breakdown the sentence displays under the options as they're set, beside the one its case expects under them: a line for each row, the word<br/>
   /// displayed, then the word expected, <c>-</c> for none, and <c>(differs)</c> after a row whose two sides aren't the same word.
   /// </summary>
   string[] TheBreakdownBesideTheExpected
   {
      get
      {
         var settings = GetService<Settings>();
         var displayed = new SentenceViewModel(_sentence, settings, GetService<JPCollection>().Vocab).Analysis.MatchesDisplayedOutsideEditMode;
         var beside = BreakdownBesideTheExpected.Of(displayed, TheCase.Expected.BreakdownUnder(settings.BreakdownParsingOptions()));
         return [.. beside.Rows.Select(it => $"{it.Displayed?.ParsedForm ?? "-"} | {it.Expected?.Form ?? "-"}{(it.IsTheSameWord ? "" : " (differs)")}")];
      }
   }

   string[] FormsExpectedDisplayedUnder(BreakdownParsingOptions options) => [.. TheCase.Expected.BreakdownUnder(options).DisplayedWords.Select(it => it.Form)];

   public class at_first : When_a_sentence_is_marked_as_a_parsing_case
   {
      [XF] public void it_is_a_parsing_case() => _sentence.ParsingCase.Must().NotBeNull();
      [XF] public void nobody_has_named_it() => TheCase.Name.Must().Be("");
      [XF] public void nobody_has_said_why() => TheCase.Why.Must().Be("");
      [XF] public void its_displayed_vocab_are_unverified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Unverified);
      [XF] public void it_expects_what_the_sentence_parses_to() => Comparison.Differences.Must().BeEmpty();
      [XF] public void its_outcome_is_that_it_parses_as_expected() => Comparison.Outcome.Must().Be(ParsingCaseOutcome.ParsesAsExpected);
      [XF] public void its_breakdown_beside_the_expected_has_each_word_beside_itself() => TheBreakdownBesideTheExpected.Must().SequenceEqual(["毎日 | 毎日", "走る | 走る", "ている | ている"]);
      [XF] public void the_collection_compares_it_as_parsing_as_expected() => TheParsingCasesComparedByTheCollection.Must().SequenceEqual(["毎日走っている: ParsesAsExpected"]);
      [XF] public void it_is_the_one_sentence_the_collection_lists_as_a_parsing_case() => GetService<JPCollection>().Sentences.ThatAreParsingCases().Select(it => it.GetQuestion()).Must().SequenceEqual(["毎日走っている"]);
      [XF] public void it_expects_the_words_the_sentence_parses_to() => TheCase.Expected.ParsedWords.Must().NotBeEmpty();
      [XF] public void it_expects_a_breakdown_displaying_the_two_vocab() => TheCase.Expected.DisplayedVocab.Words.Count(it => it.VocabId != null).Must().Be(2);
      [XF] public void it_holds_each_vocab_by_its_id_alone() => TheCase.Expected.ParsedWords.Must().Satisfy(words => words.All(it => it.Vocab == null));
      [XF] public void marking_it_again_fails() => Invoking(() => _sentence.MarkAsAParsingCase()).Must().Throw<StateAssertionFailedException>();
      [XF] public void only_hiding_all_compounds_changes_the_breakdown_it_expects() => TheCase.Expected.OptionsChangingTheBreakdown.Must().SequenceEqual(["hideAllCompounds"]);
      [XF] public void the_breakdown_it_expects_with_every_option_off_displays_the_compound() => FormsExpectedDisplayedUnder(EveryOptionOff).Must().Contain("ている");
      [XF] public void the_breakdown_it_expects_with_all_compounds_hidden_does_not_display_the_compound() => FormsExpectedDisplayedUnder(EveryOptionOff with { HideAllCompounds = true }).Must().Satisfy(forms => !forms.Contains("ている"));
      [XF] public void the_breakdown_it_expects_with_only_an_option_on_that_changes_nothing_is_the_one_with_every_option_off() =>
         FormsExpectedDisplayedUnder(EveryOptionOff with { HideCompositionallyTransparentCompounds = true }).Must().SequenceEqual(FormsExpectedDisplayedUnder(EveryOptionOff));
   }

   public class and_named_and_given_its_why : When_a_sentence_is_marked_as_a_parsing_case
   {
      public and_named_and_given_its_why()
      {
         _sentence.Edit(it =>
         {
            it.NameTheParsingCase("the progressive");
            it.SayWhyItIsAParsingCase("ている was matched as て and いる");
         });
      }

      [XF] public void it_has_the_name() => TheCase.Name.Must().Be("the progressive");
      [XF] public void it_has_the_why() => TheCase.Why.Must().Be("ている was matched as て and いる");
      [XF] public void it_still_expects_what_the_sentence_parses_to() => Comparison.Differences.Must().BeEmpty();
      [XF] public void its_editable_name_field_reads_the_name() => SentenceEditableFields.ParsingCaseName.Read(_sentence).Must().Be("the progressive");
      [XF] public void its_editable_why_field_reads_the_why() => SentenceEditableFields.ParsingCaseWhy.Read(_sentence).Must().Be("ている was matched as て and いる");
   }

   public class and_named_and_given_its_why_through_its_editable_fields : When_a_sentence_is_marked_as_a_parsing_case
   {
      public and_named_and_given_its_why_through_its_editable_fields()
      {
         SentenceEditableFields.ParsingCaseName.Write(_sentence, "the progressive");
         SentenceEditableFields.ParsingCaseWhy.Write(_sentence, "ている was matched as て and いる");
      }

      [XF] public void it_has_the_name() => TheCase.Name.Must().Be("the progressive");
      [XF] public void it_has_the_why() => TheCase.Why.Must().Be("ている was matched as て and いる");
   }

   public class and_written_as_the_corpus_stores_it_and_read_back : When_a_sentence_is_marked_as_a_parsing_case
   {
      readonly SentenceNote _readBack;

      public and_written_as_the_corpus_stores_it_and_read_back()
      {
         _sentence.Edit(it =>
         {
            it.NameTheParsingCase("the progressive");
            it.SetTheVerdictOnTheDisplayedVocab(Verdict.Broken);
         });
         var serializer = GetService<NoteSerializer>();
         _readBack = serializer.DeserializeSentence(serializer.Serialize(_sentence));
      }

      [XF] public void it_has_its_name() => _readBack.ParsingCase!.Name.Must().Be("the progressive");
      [XF] public void it_has_its_verdict() => _readBack.ParsingCase!.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Broken);
      [XF] public void it_expects_the_same_result() => TheCase.Expected.HowItDiffersFromThisExpectedResult(_readBack.ParsingCase!.Expected, GetService<JPCollection>()).Must().BeEmpty();
      [XF] public void it_parses_as_expected() => _readBack.CompareWithWhatTheParsingCaseExpects().Differences.Must().BeEmpty();
   }

   public class a_sentence_that_is_not_one_written_as_the_corpus_stores_it : When_a_sentence_is_marked_as_a_parsing_case
   {
      [XF] public void has_nothing_about_a_parsing_case() => GetService<NoteSerializer>().Serialize(CreateSentence("毎日走る")).Must().NotContain("parsingCase");
   }

   public class a_sentence_that_is_not_one : When_a_sentence_is_marked_as_a_parsing_case
   {
      readonly SentenceNote _ordinarySentence;
      public a_sentence_that_is_not_one() => _ordinarySentence = CreateSentence("毎日走る");

      [XF] public void is_not_among_the_parsing_cases_the_collection_lists() => GetService<JPCollection>().Sentences.ThatAreParsingCases().Select(it => it.GetQuestion()).Must().SequenceEqual(["毎日走っている"]);
      [XF] public void has_an_empty_editable_name_field() => SentenceEditableFields.ParsingCaseName.Read(_ordinarySentence).Must().Be("");
      [XF] public void has_an_empty_editable_why_field() => SentenceEditableFields.ParsingCaseWhy.Read(_ordinarySentence).Must().Be("");
      [XF] public void reads_as_unverified_in_its_editable_verdict_field() => SentenceEditableFields.VerdictOnTheDisplayedVocab.Read(_ordinarySentence).Must().Be(Verdict.Unverified);
      [XF] public void cannot_be_given_a_verdict_through_its_editable_verdict_field() => Invoking(() => SentenceEditableFields.VerdictOnTheDisplayedVocab.Write(_ordinarySentence, Verdict.Verified)).Must().Throw<StateAssertionFailedException>();
      [XF] public void cannot_be_named_through_its_editable_name_field() => Invoking(() => SentenceEditableFields.ParsingCaseName.Write(_ordinarySentence, "the progressive")).Must().Throw<StateAssertionFailedException>();
   }

   public class after_a_vocab_is_added_that_the_breakdown_displays : When_a_sentence_is_marked_as_a_parsing_case
   {
      public after_a_vocab_is_added_that_the_breakdown_displays() => CreateVocab("毎日", "every-day", "まいにち");

      [XF] public void it_no_longer_parses_as_expected() => Comparison.Differences.Must().NotBeEmpty();
      [XF] public void the_displayed_vocab_differ() => Comparison.TheDisplayedVocabDiffer.Must().BeTrue();
      [XF] public void it_does_not_fail_since_nobody_verified_the_displayed_vocab() => Comparison.Fails.Must().BeFalse();
      [XF] public void it_is_to_review() => Comparison.IsToReview.Must().BeTrue();
      [XF] public void its_outcome_is_that_it_is_to_review() => Comparison.Outcome.Must().Be(ParsingCaseOutcome.IsToReview);
      [XF] public void its_breakdown_beside_the_expected_has_the_vocabs_word_beside_the_word_of_no_vocab_expected_at_its_place_as_differing() =>
         TheBreakdownBesideTheExpected.Must().SequenceEqual(["毎日 | 毎日 (differs)", "走る | 走る", "ている | ている"]);
      [XF] public void the_collection_compares_it_as_being_to_review() => TheParsingCasesComparedByTheCollection.Must().SequenceEqual(["毎日走っている: IsToReview"]);

      public class and_what_it_parses_to_is_accepted : after_a_vocab_is_added_that_the_breakdown_displays
      {
         public and_what_it_parses_to_is_accepted() => _sentence.Edit(it => it.AcceptWhatItParsesToAsAParsingCase());

         [XF] public void it_parses_as_expected_again() => Comparison.Differences.Must().BeEmpty();
         [XF] public void its_displayed_vocab_are_still_unverified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Unverified);
      }
   }

   public class after_a_vocab_is_added_that_covers_two_of_the_words_the_breakdown_displayed : When_a_sentence_is_marked_as_a_parsing_case
   {
      public after_a_vocab_is_added_that_covers_two_of_the_words_the_breakdown_displayed() => CreateVocab("走っている", "is-running", "はしっている");

      [XF] public void its_breakdown_beside_the_expected_has_the_vocabs_word_beside_the_first_word_it_covers_and_nothing_beside_the_second() =>
         TheBreakdownBesideTheExpected.Must().SequenceEqual(["毎日 | 毎日", "走っている | 走る (differs)", "- | ている (differs)"]);
   }

   public class whose_displayed_vocab_a_person_has_verified : When_a_sentence_is_marked_as_a_parsing_case
   {
      public whose_displayed_vocab_a_person_has_verified() => _sentence.Edit(it => it.SetTheVerdictOnTheDisplayedVocab(Verdict.Verified));

      [XF] public void they_are_verified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Verified);
      [XF] public void its_editable_verdict_field_reads_verified() => SentenceEditableFields.VerdictOnTheDisplayedVocab.Read(_sentence).Must().Be(Verdict.Verified);
      [XF] public void it_does_not_fail() => Comparison.Fails.Must().BeFalse();

      public class and_then_a_vocab_is_added_that_the_breakdown_displays : whose_displayed_vocab_a_person_has_verified
      {
         public and_then_a_vocab_is_added_that_the_breakdown_displays() => CreateVocab("毎日", "every-day", "まいにち");

         [XF] public void it_fails() => Comparison.Fails.Must().BeTrue();
         [XF] public void its_outcome_is_that_it_fails() => Comparison.Outcome.Must().Be(ParsingCaseOutcome.Fails);
         [XF] public void it_is_not_merely_to_review() => Comparison.IsToReview.Must().BeFalse();

         public class and_what_it_then_parses_to_is_accepted : and_then_a_vocab_is_added_that_the_breakdown_displays
         {
            public and_what_it_then_parses_to_is_accepted() => _sentence.Edit(it => it.AcceptWhatItParsesToAsAParsingCase());

            [XF] public void it_no_longer_fails() => Comparison.Fails.Must().BeFalse();
            [XF] public void its_displayed_vocab_are_still_verified() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Verified);
         }
      }

      public class and_then_a_vocab_is_added_that_matches_but_is_not_displayed : whose_displayed_vocab_a_person_has_verified
      {
         public and_then_a_vocab_is_added_that_matches_but_is_not_displayed() => CreateVocab("いる", "to-be", "いる");

         [XF] public void it_parses_otherwise_than_expected() => Comparison.Differences.Must().NotBeEmpty();
         [XF] public void the_displayed_vocab_are_the_same() => Comparison.TheDisplayedVocabDiffer.Must().BeFalse();
         [XF] public void it_still_does_not_fail() => Comparison.Fails.Must().BeFalse();
         [XF] public void it_is_to_review() => Comparison.IsToReview.Must().BeTrue();
      }
   }

   public class whose_displayed_vocab_are_marked_broken_through_its_editable_verdict_field : When_a_sentence_is_marked_as_a_parsing_case
   {
      public whose_displayed_vocab_are_marked_broken_through_its_editable_verdict_field() => SentenceEditableFields.VerdictOnTheDisplayedVocab.Write(_sentence, Verdict.Broken);

      [XF] public void they_are_broken() => TheCase.VerdictOnTheDisplayedVocab.Must().Be(Verdict.Broken);
   }

   /// <summary>Edit mode lists every match of the sentence, valid or not, where the breakdown otherwise displays the words a person reads it by.</summary>
   public class and_then_the_breakdown_is_switched_to_edit_mode : When_a_sentence_is_marked_as_a_parsing_case
   {
      public and_then_the_breakdown_is_switched_to_edit_mode() => GetService<JapaneseConfig>().ShowSentenceBreakdownInEditMode.Value = true;

      [XF] public void it_still_parses_as_expected() => Comparison.Differences.Must().BeEmpty();

      public class and_another_sentence_with_the_same_words_is_marked_as_a_parsing_case : and_then_the_breakdown_is_switched_to_edit_mode
      {
         readonly SentenceNote _markedInEditMode;

         public and_another_sentence_with_the_same_words_is_marked_as_a_parsing_case()
         {
            _markedInEditMode = CreateSentence("毎日走っている");
            _markedInEditMode.Edit(it => it.MarkAsAParsingCase());
         }

         [XF] public void it_expects_the_same_breakdown_as_the_one_marked_outside_edit_mode() =>
            _markedInEditMode.ParsingCase!.Expected.BreakdownWithEveryOptionOff.DisplayedWords.Must().SequenceEqual(TheCase.Expected.BreakdownWithEveryOptionOff.DisplayedWords);
      }
   }

   public class and_then_stops_being_one : When_a_sentence_is_marked_as_a_parsing_case
   {
      public and_then_stops_being_one() => _sentence.Edit(it => it.StopBeingAParsingCase());

      [XF] public void it_is_no_parsing_case() => _sentence.ParsingCase.Must().BeNull();
      [XF] public void comparing_it_fails() => Invoking(() => _sentence.CompareWithWhatTheParsingCaseExpects()).Must().Throw<StateAssertionFailedException>();
      [XF] public void accepting_what_it_parses_to_fails() => Invoking(() => _sentence.AcceptWhatItParsesToAsAParsingCase()).Must().Throw<StateAssertionFailedException>();
   }
}
