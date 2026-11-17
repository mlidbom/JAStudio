using System;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The kanji 走 (run), 歩 (walk) and 赴 (proceed), and the radicals 土 (soil) and 人 (person).</summary>
public class When_relating_kanji_to_other_kanji : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly ITaggregateReference<KanjiNote> RunReference;
   protected KanjiNote Run => RunReference.Current;
   protected readonly ITaggregateReference<KanjiNote> WalkReference;
   protected KanjiNote Walk => WalkReference.Current;
   protected readonly ITaggregateReference<KanjiNote> ProceedReference;
   protected KanjiNote Proceed => ProceedReference.Current;

   public When_relating_kanji_to_other_kanji()
   {
      RunReference = CreateKanji("走", "run", "ソウ", "はし").Reference();
      WalkReference = CreateKanji("歩", "walk", "ホ", "ある").Reference();
      ProceedReference = CreateKanji("赴", "proceed", "フ", "おもむ").Reference();
   }

   public class after_adding_歩_to_走_s_kanji_with_a_similar_meaning : When_relating_kanji_to_other_kanji
   {
      public after_adding_歩_to_走_s_kanji_with_a_similar_meaning() => Run.Edit(it => it.SimilarMeaning.Add("歩"));

      [XF] public void 走_has_歩_as_a_kanji_with_a_similar_meaning() => Run.SimilarMeaning.Kanji.Must().SequenceEqual(["歩"]);
      [XF] public void 歩_has_走_as_a_kanji_with_a_similar_meaning() => Walk.SimilarMeaning.Kanji.Must().SequenceEqual(["走"]);
      [XF] public void the_notes_of_走_s_kanji_with_a_similar_meaning_are_歩_s() => Run.SimilarMeaning.Notes().Must().SequenceEqual([Walk]);

      public class and_then_removing_it : after_adding_歩_to_走_s_kanji_with_a_similar_meaning
      {
         public and_then_removing_it() => Run.Edit(it => it.SimilarMeaning.Remove("歩"));

         [XF] public void 走_has_no_kanji_with_a_similar_meaning() => Run.SimilarMeaning.Kanji.Must().BeEmpty();
         [XF] public void 歩_has_no_kanji_with_a_similar_meaning() => Walk.SimilarMeaning.Kanji.Must().BeEmpty();
      }

      public class and_then_adding_赴_too : after_adding_歩_to_走_s_kanji_with_a_similar_meaning
      {
         public and_then_adding_赴_too() => Run.Edit(it => it.SimilarMeaning.Add("赴"));

         [XF] public void 走_has_歩_and_赴_in_the_order_they_were_added() => Run.SimilarMeaning.Kanji.Must().SequenceEqual(["歩", "赴"]);
      }
   }

   public class after_歩_adds_走_to_its_kanji_with_a_similar_meaning_and_走_then_removes_歩_from_its_own : When_relating_kanji_to_other_kanji
   {
      public after_歩_adds_走_to_its_kanji_with_a_similar_meaning_and_走_then_removes_歩_from_its_own()
      {
         Walk.Edit(it => it.SimilarMeaning.Add("走"));
         Run.Edit(it => it.SimilarMeaning.Remove("歩"));
      }

      [XF] public void neither_has_a_kanji_with_a_similar_meaning() =>
         new { 走 = Run.SimilarMeaning.Kanji, 歩 = Walk.SimilarMeaning.Kanji }.Must().Satisfy(it => it.走.Count == 0 && it.歩.Count == 0);
   }

   public class after_adding_歩_to_走_s_kanji_with_a_similar_meaning_in_a_unit_of_work_that_then_fails : When_relating_kanji_to_other_kanji
   {
      public after_adding_歩_to_走_s_kanji_with_a_similar_meaning_in_a_unit_of_work_that_then_fails() =>
         Invoking(() => GetService<JPCollection>().InAUnitOfWork(unitOfWork =>
                 {
                    unitOfWork.ForUpdate(Run).SimilarMeaning.Add("歩");
                    throw new InvalidOperationException("The unit of work fails.");
                 }))
           .Must().Throw<InvalidOperationException>();

      [XF] public void neither_has_a_kanji_with_a_similar_meaning() =>
         new { 走 = Run.SimilarMeaning.Kanji, 歩 = Walk.SimilarMeaning.Kanji }.Must().Satisfy(it => it.走.Count == 0 && it.歩.Count == 0);
   }

   public class after_adding_a_kanji_the_corpus_has_no_note_for_to_走_s_kanji_with_a_similar_meaning : When_relating_kanji_to_other_kanji
   {
      public after_adding_a_kanji_the_corpus_has_no_note_for_to_走_s_kanji_with_a_similar_meaning() => Run.Edit(it => it.SimilarMeaning.Add("駆"));

      [XF] public void 走_has_it_as_a_kanji_with_a_similar_meaning() => Run.SimilarMeaning.Kanji.Must().SequenceEqual(["駆"]);
      [XF] public void there_are_no_notes_of_走_s_kanji_with_a_similar_meaning() => Run.SimilarMeaning.Notes().Must().BeEmpty();
   }

   public class after_adding_走_itself_to_its_kanji_with_a_similar_meaning_and_those_it_is_confused_with : When_relating_kanji_to_other_kanji
   {
      public after_adding_走_itself_to_its_kanji_with_a_similar_meaning_and_those_it_is_confused_with()
      {
         Run.SimilarMeaning.Add("走");
         Run.ConfusedWith.Add("走");
      }

      [XF] public void 走_has_no_kanji_with_a_similar_meaning() => Run.SimilarMeaning.Kanji.Must().BeEmpty();
      [XF] public void 走_is_confused_with_no_kanji() => Run.ConfusedWith.Kanji.Must().BeEmpty();
   }

   public class after_adding_赴_to_the_kanji_走_is_confused_with : When_relating_kanji_to_other_kanji
   {
      public after_adding_赴_to_the_kanji_走_is_confused_with() => Run.Edit(it => it.ConfusedWith.Add("赴"));

      [XF] public void 走_is_confused_with_赴() => Run.ConfusedWith.Kanji.Must().SequenceEqual(["赴"]);
      [XF] public void 赴_is_confused_with_走() => Proceed.ConfusedWith.Kanji.Must().SequenceEqual(["走"]);

      public class and_then_removing_it : after_adding_赴_to_the_kanji_走_is_confused_with
      {
         public and_then_removing_it() => Run.Edit(it => it.ConfusedWith.Remove("赴"));

         [XF] public void 走_is_confused_with_no_kanji() => Run.ConfusedWith.Kanji.Must().BeEmpty();
         [XF] public void 赴_is_confused_with_no_kanji() => Proceed.ConfusedWith.Kanji.Must().BeEmpty();
      }
   }

   public class after_adding_the_radicals_土_土_and_人_to_走 : When_relating_kanji_to_other_kanji
   {
      public after_adding_the_radicals_土_土_and_人_to_走()
      {
         Run.Edit(it =>
         {
            it.Radicals.Add("土");
            it.Radicals.Add("土");
            it.Radicals.Add("人");
         });
      }

      [XF] public void 走_has_土_twice_then_人_as_its_radicals() => Run.Radicals.All.Must().SequenceEqual(["土", "土", "人"]);

      public class and_then_removing_土 : after_adding_the_radicals_土_土_and_人_to_走
      {
         public and_then_removing_土() => Run.Edit(it => it.Radicals.Remove("土"));

         [XF] public void 走_has_土_once_then_人_as_its_radicals() => Run.Radicals.All.Must().SequenceEqual(["土", "人"]);
      }

      public class and_then_adding_走_itself : after_adding_the_radicals_土_土_and_人_to_走
      {
         public and_then_adding_走_itself() => Run.Radicals.Add("走");

         [XF] public void 走_s_radicals_are_unchanged() => Run.Radicals.All.Must().SequenceEqual(["土", "土", "人"]);
      }
   }
}
