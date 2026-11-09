using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// The vocab かける, with the form 掛ける, which other vocab refer to by name: 掛かる has it as its ergative twin, 話しかける has it as a compound part, and<br/>
/// 掛け算 is derived from it. 引っ掛ける has its form 掛ける as a compound part, which refers to it through the form rather than by its name.
/// </summary>
public class When_renaming_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly VocabNote ToHang;
   protected readonly VocabNote ToBeHanging;
   protected readonly VocabNote ToStartTalkingTo;
   protected readonly VocabNote Multiplication;
   protected readonly VocabNote ToHook;

   public When_renaming_a_vocab()
   {
      ToHang = CreateVocab("かける", "to hang", ["かける"], ["掛ける"]);
      ToBeHanging = CreateVocab("掛かる", "to be hanging", "かかる");
      ToBeHanging.RelatedNotes.ErgativeTwins.Add(new WordReference("かける"));
      ToStartTalkingTo = CreateVocab("話しかける", "to start talking to", "はなしかける");
      ToStartTalkingTo.CompoundParts.SetPrimary(Words("話す", "かける"));
      Multiplication = CreateVocab("掛け算", "multiplication", "かけざん");
      Multiplication.RelatedNotes.DerivedFrom.Set(new WordReference("かける"));
      ToHook = CreateVocab("引っ掛ける", "to hook", "ひっかける");
      ToHook.CompoundParts.SetPrimary(Words("引く", "掛ける"));
   }

   protected static string[] ErgativeTwinsOf(VocabNote vocab) => SortedNamesOf(vocab.RelatedNotes.ErgativeTwins.Words);

   public class after_changing_its_question_to_懸ける : When_renaming_a_vocab
   {
      public after_changing_its_question_to_懸ける() => ToHang.Question.SetText("懸ける");

      [XF] public void 掛かる_has_懸ける_as_its_ergative_twin() => ErgativeTwinsOf(ToBeHanging).Must().SequenceEqual(["懸ける"]);
      [XF] public void 懸ける_keeps_掛かる_as_its_ergative_twin() => ErgativeTwinsOf(ToHang).Must().SequenceEqual(["掛かる"]);
      [XF] public void 話しかける_has_the_compound_parts_話す_and_懸ける() => NamesOf(ToStartTalkingTo.CompoundParts.Primary()).Must().SequenceEqual(["話す", "懸ける"]);
      [XF] public void 掛け算_is_derived_from_懸ける() => Multiplication.RelatedNotes.DerivedFrom.Word.Must().Be(new WordReference("懸ける"));

      [XF] public void 引っ掛ける_keeps_its_compound_part_掛ける_which_named_the_form_rather_than_the_question() =>
         NamesOf(ToHook.CompoundParts.Primary()).Must().SequenceEqual(["引く", "掛ける"]);

      [XF] public void 引っ掛ける_s_compound_part_掛ける_still_refers_to_the_vocab_through_its_form() =>
         ToHook.CompoundParts.PrimaryPartsNotes().Must().SequenceEqual([ToHang]);
   }

   public class after_giving_it_the_disambiguation_hang : When_renaming_a_vocab
   {
      public after_giving_it_the_disambiguation_hang() => ToHang.Question.SetDisambiguation("hang");

      [XF] public void 掛かる_has_かける_hang_as_its_ergative_twin() => ErgativeTwinsOf(ToBeHanging).Must().SequenceEqual(["かける:hang"]);
      [XF] public void 話しかける_has_the_compound_parts_話す_and_かける_hang() => NamesOf(ToStartTalkingTo.CompoundParts.Primary()).Must().SequenceEqual(["話す", "かける:hang"]);
      [XF] public void 掛け算_is_derived_from_かける_hang() => Multiplication.RelatedNotes.DerivedFrom.Word.Must().Be(new WordReference("かける", "hang"));
   }

   /// <summary>Another vocab, a duplicate, with the question かける and no disambiguation either.</summary>
   public class while_another_vocab_has_the_same_name : When_renaming_a_vocab
   {
      public while_another_vocab_has_the_same_name() => CreateVocab("かける", "to bet", "かける");

      public class and_after_changing_its_question_to_懸ける : while_another_vocab_has_the_same_name
      {
         public and_after_changing_its_question_to_懸ける() => ToHang.Question.SetText("懸ける");

         [XF] public void 掛かる_keeps_かける_as_its_ergative_twin_since_it_still_names_the_other_vocab() => ErgativeTwinsOf(ToBeHanging).Must().SequenceEqual(["かける"]);
         [XF] public void 話しかける_keeps_the_compound_parts_話す_and_かける() => NamesOf(ToStartTalkingTo.CompoundParts.Primary()).Must().SequenceEqual(["話す", "かける"]);
      }
   }
}
