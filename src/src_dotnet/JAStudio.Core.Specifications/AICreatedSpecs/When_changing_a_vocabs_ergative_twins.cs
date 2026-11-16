using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using static JAStudio.Core.Specifications.WordReferenceNames;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>The vocab 起こす, 起きる and 起こる, none with ergative twins: 起こす is the transitive counterpart of both the others.</summary>
public class When_changing_a_vocabs_ergative_twins : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   protected readonly ITaggregateReference<VocabNote> ToWakeSomeoneReference;
   protected VocabNote ToWakeSomeone => ToWakeSomeoneReference.Current;
   protected readonly ITaggregateReference<VocabNote> ToWakeUpReference;
   protected VocabNote ToWakeUp => ToWakeUpReference.Current;
   protected readonly ITaggregateReference<VocabNote> ToHappenReference;
   protected VocabNote ToHappen => ToHappenReference.Current;

   public When_changing_a_vocabs_ergative_twins()
   {
      ToWakeSomeoneReference = VocabNote.Create(NoteServices, "起こす", "to wake someone; to cause", "おこす").Reference();
      ToWakeUpReference = VocabNote.Create(NoteServices, "起きる", "to wake up", "おきる").Reference();
      ToHappenReference = VocabNote.Create(NoteServices, "起こる", "to happen", "おこる").Reference();
   }

   protected static string[] ErgativeTwinsOf(VocabNote vocab) => SortedNamesOf(vocab.RelatedNotes.ErgativeTwins.Words);

   public class adding_起きる_and_起こる_as_ergative_twins_of_起こす : When_changing_a_vocabs_ergative_twins
   {
      public adding_起きる_and_起こる_as_ergative_twins_of_起こす()
      {
         ToWakeSomeone.Edit(it =>
         {
            it.RelatedNotes.ErgativeTwins.Add(new WordReference("起きる"));
            it.RelatedNotes.ErgativeTwins.Add(new WordReference("起こる"));
         });
      }

      [XF] public void 起こす_has_both_as_its_ergative_twins() => ErgativeTwinsOf(ToWakeSomeone).Must().SequenceEqual(["起きる", "起こる"]);
      [XF] public void 起きる_has_起こす_as_its_ergative_twin() => ErgativeTwinsOf(ToWakeUp).Must().SequenceEqual(["起こす"]);
      [XF] public void 起こる_has_起こす_as_its_ergative_twin() => ErgativeTwinsOf(ToHappen).Must().SequenceEqual(["起こす"]);

      public class and_then_removing_起きる_from_起こす : adding_起きる_and_起こる_as_ergative_twins_of_起こす
      {
         public and_then_removing_起きる_from_起こす() => ToWakeSomeone.Edit(it => it.RelatedNotes.ErgativeTwins.Remove(new WordReference("起きる")));

         [XF] public void 起こす_keeps_起こる_as_its_ergative_twin() => ErgativeTwinsOf(ToWakeSomeone).Must().SequenceEqual(["起こる"]);
         [XF] public void 起きる_no_longer_has_起こす_as_its_ergative_twin() => ErgativeTwinsOf(ToWakeUp).Must().BeEmpty();
         [XF] public void 起こる_keeps_起こす_as_its_ergative_twin() => ErgativeTwinsOf(ToHappen).Must().SequenceEqual(["起こす"]);
      }
   }
}
