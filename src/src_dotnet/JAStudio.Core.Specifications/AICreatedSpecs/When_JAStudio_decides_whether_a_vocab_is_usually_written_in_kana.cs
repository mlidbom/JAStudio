using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Whether a vocab is usually written in kana (<see cref="VocabNoteUsuallyWrittenInKana"/>): JAStudio decides it while it's undecided, when it generates<br/>
/// the vocab's data, and never changes it once it's decided. A yes shows as "uk" among the vocab's meta tags.
/// </summary>
public class When_JAStudio_decides_whether_a_vocab_is_usually_written_in_kana : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   const string TheUkMetaTagsTooltip = "usually written using kana only";

   static string TheMetaTagsHtmlOf(VocabNote vocab) => vocab.MetaData.MetaTagsHtml(noSentenceStatistics: true);

   public class for_a_new_vocab_whose_question_is_all_kana_and_that_has_no_readings : When_JAStudio_decides_whether_a_vocab_is_usually_written_in_kana
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;
      public for_a_new_vocab_whose_question_is_all_kana_and_that_has_no_readings() => _vocabReference = CreateVocab("すごい", "amazing").Reference();

      [XF] public void it_is_decided_as_yes() => _vocab.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.Yes);
      [XF] public void it_shows_the_uk_meta_tag() => TheMetaTagsHtmlOf(_vocab).Must().Contain(TheUkMetaTagsTooltip);
   }

   public class for_a_new_vocab_whose_question_is_not_all_kana : When_JAStudio_decides_whether_a_vocab_is_usually_written_in_kana
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;
      public for_a_new_vocab_whose_question_is_not_all_kana() => _vocabReference = CreateVocab("毎月", "every month", "まいつき").Reference();

      [XF] public void it_stays_undecided() => _vocab.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.Undecided);
      [XF] public void it_does_not_show_the_uk_meta_tag() => TheMetaTagsHtmlOf(_vocab).Must().NotContain(TheUkMetaTagsTooltip);

      public class after_it_is_decided_as_yes_and_its_data_is_generated_again : for_a_new_vocab_whose_question_is_not_all_kana
      {
         public after_it_is_decided_as_yes_and_its_data_is_generated_again()
         {
            _vocab.Edit(it =>
            {
               it.UsuallyWrittenInKana.Decide(UsuallyWrittenInKanaDecision.Yes);
               it.UpdateGeneratedData();
            });
         }

         [XF] public void it_stays_yes() => _vocab.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.Yes);
         [XF] public void it_shows_the_uk_meta_tag() => TheMetaTagsHtmlOf(_vocab).Must().Contain(TheUkMetaTagsTooltip);
      }
   }

   public class for_a_vocab_whose_question_is_all_kana_decided_as_no : When_JAStudio_decides_whether_a_vocab_is_usually_written_in_kana
   {
      readonly ITaggregateReference<VocabNote> _vocabReference;
      VocabNote _vocab => _vocabReference.Current;

      public for_a_vocab_whose_question_is_all_kana_decided_as_no()
      {
         _vocabReference = CreateVocab("すごい", "amazing").Reference();
         _vocab.Edit(it => it.UsuallyWrittenInKana.Decide(UsuallyWrittenInKanaDecision.No));
      }

      [XF] public void it_does_not_show_the_uk_meta_tag() => TheMetaTagsHtmlOf(_vocab).Must().NotContain(TheUkMetaTagsTooltip);

      public class after_its_data_is_generated_again_without_readings : for_a_vocab_whose_question_is_all_kana_decided_as_no
      {
         public after_its_data_is_generated_again_without_readings()
         {
            _vocab.Edit(it =>
            {
               it.SetReadings([]);
               it.UpdateGeneratedData();
            });
         }

         [XF] public void it_stays_no() => _vocab.UsuallyWrittenInKana.Decision.Must().Be(UsuallyWrittenInKanaDecision.No);
      }
   }
}
