using System;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using static Compze.Must.MustActions;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// The vocab かける, with the form 掛ける, which other vocab refer to by name: 掛かる has it as its ergative twin, 話しかける has it as a compound part, and<br/>
/// 掛け算 is derived from it, and the kanji 掛 has it as its primary vocab. 引っ掛ける has its form 掛ける as a compound part, which refers to it through<br/>
/// the form rather than by its name.
/// </summary>
public class When_renaming_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly ITaggregateReference<VocabNote> ToHangReference;
   protected VocabNote ToHang => ToHangReference.Current;
   protected readonly ITaggregateReference<VocabNote> ToBeHangingReference;
   protected VocabNote ToBeHanging => ToBeHangingReference.Current;
   protected readonly ITaggregateReference<VocabNote> ToStartTalkingToReference;
   protected VocabNote ToStartTalkingTo => ToStartTalkingToReference.Current;
   protected readonly ITaggregateReference<VocabNote> MultiplicationReference;
   protected VocabNote Multiplication => MultiplicationReference.Current;
   protected readonly ITaggregateReference<VocabNote> ToHookReference;
   protected VocabNote ToHook => ToHookReference.Current;
   protected readonly ITaggregateReference<KanjiNote> HangReference;
   protected KanjiNote Hang => HangReference.Current;

   public When_renaming_a_vocab()
   {
      ToHangReference = CreateVocab("かける", "to hang", ["かける"], ["掛ける"]).Reference();
      ToBeHangingReference = CreateVocab("掛かる", "to be hanging", "かかる").Reference();
      ToBeHanging.Edit(it => it.RelatedNotes.ErgativeTwins.Add(new WordReference("かける")));
      ToStartTalkingToReference = CreateVocab("話しかける", "to start talking to", "はなしかける").Reference();
      ToStartTalkingTo.Edit(it => it.CompoundParts.SetPrimary(Words("話す", "かける")));
      MultiplicationReference = CreateVocab("掛け算", "multiplication", "かけざん").Reference();
      Multiplication.Edit(it => it.RelatedNotes.DerivedFrom.Set(new WordReference("かける")));
      ToHookReference = CreateVocab("引っ掛ける", "to hook", "ひっかける").Reference();
      ToHook.Edit(it => it.CompoundParts.SetPrimary(Words("引く", "掛ける")));
      HangReference = CreateKanji("掛", "hang", "カイ", "か").Reference();
      Hang.Edit(it => it.PrimaryVocab.PutLast(new WordReference("かける")));
   }

   protected static string[] ErgativeTwinsOf(VocabNote vocab) => SortedNamesOf(vocab.RelatedNotes.ErgativeTwins.Words);

   public class after_changing_its_question_to_懸ける : When_renaming_a_vocab
   {
      public after_changing_its_question_to_懸ける() => ToHang.Edit(it => it.Question.SetText("懸ける"));

      [XF] public void 掛かる_has_懸ける_as_its_ergative_twin() => ErgativeTwinsOf(ToBeHanging).Must().SequenceEqual(["懸ける"]);
      [XF] public void 懸ける_keeps_掛かる_as_its_ergative_twin() => ErgativeTwinsOf(ToHang).Must().SequenceEqual(["掛かる"]);
      [XF] public void 話しかける_has_the_compound_parts_話す_and_懸ける() => NamesOf(ToStartTalkingTo.CompoundParts.Primary()).Must().SequenceEqual(["話す", "懸ける"]);
      [XF] public void 掛け算_is_derived_from_懸ける() => Multiplication.RelatedNotes.DerivedFrom.Word.Must().Be(new WordReference("懸ける"));
      [XF] public void the_kanji_掛_has_懸ける_as_its_primary_vocab() => Hang.PrimaryVocab.Words.Must().SequenceEqual([new WordReference("懸ける")]);

      [XF] public void 引っ掛ける_keeps_its_compound_part_掛ける_which_named_the_form_rather_than_the_question() =>
         NamesOf(ToHook.CompoundParts.Primary()).Must().SequenceEqual(["引く", "掛ける"]);

      [XF] public void 引っ掛ける_s_compound_part_掛ける_still_refers_to_the_vocab_through_its_form() =>
         ToHook.CompoundParts.PrimaryPartsNotes().Must().SequenceEqual([ToHang]);
   }

   public class after_changing_its_question_to_懸ける_in_a_unit_of_work_that_then_fails : When_renaming_a_vocab
   {
      public after_changing_its_question_to_懸ける_in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(unitOfWork =>
                 {
                    unitOfWork.ForUpdate(ToHang).Question.SetText("懸ける");
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void it_keeps_the_question_かける() => ToHang.GetQuestion().Must().Be("かける");
      [XF] public void 掛かる_keeps_かける_as_its_ergative_twin() => ErgativeTwinsOf(ToBeHanging).Must().SequenceEqual(["かける"]);
      [XF] public void the_kanji_掛_keeps_かける_as_its_primary_vocab() => Hang.PrimaryVocab.Words.Must().SequenceEqual([new WordReference("かける")]);
   }

   /// <summary>
   /// Five more vocab, each with かける in one relationship to words, the only one it has かける in: 吊るす as a synonym, 外す as an antonym, 掛け軸 as<br/>
   /// a see-also, 賭ける as a word it's confused with, and ぶら下げる as a perfect synonym, which is a synonym too.
   /// </summary>
   public class and_five_more_vocab_have_it_each_in_one_other_relationship_to_words : When_renaming_a_vocab
   {
      readonly ITaggregateReference<VocabNote> _synonymReference;
      readonly ITaggregateReference<VocabNote> _antonymReference;
      readonly ITaggregateReference<VocabNote> _seeAlsoReference;
      readonly ITaggregateReference<VocabNote> _confusedWithReference;
      readonly ITaggregateReference<VocabNote> _perfectSynonymReference;

      public and_five_more_vocab_have_it_each_in_one_other_relationship_to_words()
      {
         var toHang = new WordReference("かける");
         _synonymReference = CreateVocab("吊るす", "to hang", "つるす").Reference();
         _synonymReference.Current.Edit(it => it.RelatedNotes.Synonyms.Add(toHang));
         _antonymReference = CreateVocab("外す", "to unhook", "はずす").Reference();
         _antonymReference.Current.Edit(it => it.RelatedNotes.Antonyms.Add(toHang));
         _seeAlsoReference = CreateVocab("掛け軸", "hanging scroll", "かけじく").Reference();
         _seeAlsoReference.Current.Edit(it => it.RelatedNotes.SeeAlso.Add(toHang));
         _confusedWithReference = CreateVocab("賭ける", "to bet", "かける").Reference();
         _confusedWithReference.Current.Edit(it => it.RelatedNotes.ConfusedWith.Add(toHang));
         _perfectSynonymReference = CreateVocab("ぶら下げる", "to hang", "ぶらさげる").Reference();
         _perfectSynonymReference.Current.Edit(it => it.RelatedNotes.PerfectSynonyms.AddOverwritingTheAnswerOfTheAddedSynonym(toHang));

         ToHang.Edit(it => it.Question.SetText("懸ける"));
      }

      [XF] public void after_changing_its_question_to_懸ける_each_has_懸ける_instead() =>
         new
         {
            SynonymsOf吊るす = SortedNamesOf(_synonymReference.Current.RelatedNotes.Synonyms.Words),
            AntonymsOf外す = SortedNamesOf(_antonymReference.Current.RelatedNotes.Antonyms.Words),
            SeeAlsoOf掛け軸 = SortedNamesOf(_seeAlsoReference.Current.RelatedNotes.SeeAlso.Words),
            ConfusedWithOf賭ける = SortedNamesOf(_confusedWithReference.Current.RelatedNotes.ConfusedWith.Words),
            PerfectSynonymsOfぶら下げる = SortedNamesOf(_perfectSynonymReference.Current.RelatedNotes.PerfectSynonyms.Words)
         }.Must().DeepEqual(new
         {
            SynonymsOf吊るす = new[] { "懸ける" },
            AntonymsOf外す = new[] { "懸ける" },
            SeeAlsoOf掛け軸 = new[] { "懸ける" },
            ConfusedWithOf賭ける = new[] { "懸ける" },
            PerfectSynonymsOfぶら下げる = new[] { "懸ける" }
         });
   }

   public class after_giving_it_the_disambiguation_hang : When_renaming_a_vocab
   {
      public after_giving_it_the_disambiguation_hang() => ToHang.Edit(it => it.Question.SetDisambiguation("hang"));

      [XF] public void 掛かる_has_かける_hang_as_its_ergative_twin() => ErgativeTwinsOf(ToBeHanging).Must().SequenceEqual(["かける:hang"]);
      [XF] public void 話しかける_has_the_compound_parts_話す_and_かける_hang() => NamesOf(ToStartTalkingTo.CompoundParts.Primary()).Must().SequenceEqual(["話す", "かける:hang"]);
      [XF] public void 掛け算_is_derived_from_かける_hang() => Multiplication.RelatedNotes.DerivedFrom.Word.Must().Be(new WordReference("かける", "hang"));
      [XF] public void the_kanji_掛_has_かける_hang_as_its_primary_vocab() => Hang.PrimaryVocab.Words.Must().SequenceEqual([new WordReference("かける", "hang")]);
   }
}
