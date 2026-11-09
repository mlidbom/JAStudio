using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Tools.Editing;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

/// <summary>The vocab 行った has the reading いいた.</summary>
public class When_setting_the_readings_of_a_vocab : McpServerSpecification
{
   protected readonly VocabNote Vocab;

   public When_setting_the_readings_of_a_vocab() => Vocab = CreateVocab("行った", "went", "いいた");

   protected IEnumerable<string> ReadingsStored() =>
      GetService<INoteRepository>().LoadAll().Vocab.Single(it => it.GetId() == Vocab.GetId()).GetReadings();

   public class to_いった_and_おこなった : When_setting_the_readings_of_a_vocab
   {
      readonly ReadingsChange _change;

      public to_いった_and_おこなった() =>
         _change = ResultOf<ReadingsChange>(CallTool("set_the_readings_of_a_vocab",
                                                     new Dictionary<string, object?> { ["vocabNoteId"] = Vocab.GetId().ToString(), ["readings"] = new[] { "いった", "おこなった" } }));

      [XF] public void the_change_names_the_vocab() => _change.Vocab.Must().Be("行った");
      [XF] public void the_change_has_the_readings_before() => _change.Before.Must().SequenceEqual(["いいた"]);
      [XF] public void the_change_has_the_readings_after_in_their_order() => _change.After.Must().SequenceEqual(["いった", "おこなった"]);
      [XF] public void the_vocab_has_them() => Vocab.GetReadings().Must().SequenceEqual(["いった", "おこなった"]);
      [XF] public void the_vocab_is_saved_with_them() => ReadingsStored().Must().SequenceEqual(["いった", "おこなった"]);
   }
}
