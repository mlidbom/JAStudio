using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Editing;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

/// <summary>The vocab 勉強, with the answer "study". The specifications' Anki is one whose card operations do nothing.</summary>
public class When_running_a_universal_note_action : McpServerSpecification
{
   protected readonly VocabNote Vocab;

   public When_running_a_universal_note_action() => Vocab = CreateVocab("勉強", "study", "べんきょう");

   protected CallToolResult Run(string tool) => CallTool(tool, new Dictionary<string, object?> { ["noteId"] = Vocab.GetId().ToString() });

   protected CallToolResult Delete(string? ankiNote) => CallTool("delete_a_note", new Dictionary<string, object?> { ["noteId"] = Vocab.GetId().ToString(), ["ankiNote"] = ankiNote });

   protected bool TheVocabIsInTheCollection() => GetService<JPCollection>().Vocab.WithIdOrNone(Vocab.GetId()) != null; //todo:compze: assert on the lookup itself once Must's failure message can serialize a note without crashing the test process

   public class deleting_the_vocab_which_has_an_Anki_note
   {
      public class keeping_its_Anki_note : When_running_a_universal_note_action
      {
         readonly NoteSummary _deleted;

         public keeping_its_Anki_note() => _deleted = ResultOf<NoteSummary>(Delete("keep"));

         [XF] public void the_vocab_had_an_Anki_note() => Vocab.HasAnAnkiNote().Must().BeTrue();
         [XF] public void the_vocab_deleted_is_reported() => _deleted.Question.Must().Be("勉強");
         [XF] public void the_vocab_is_no_longer_in_the_collection() => TheVocabIsInTheCollection().Must().BeFalse();
      }

      public class deleting_its_Anki_note_too : When_running_a_universal_note_action
      {
         public deleting_its_Anki_note_too() => ResultOf<NoteSummary>(Delete("delete"));

         [XF] public void the_vocab_is_no_longer_in_the_collection() => TheVocabIsInTheCollection().Must().BeFalse();
      }

      public class without_saying_what_to_do_with_its_Anki_note : When_running_a_universal_note_action
      {
         readonly CallToolResult _result;

         public without_saying_what_to_do_with_its_Anki_note() => _result = Delete(null);

         [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
         [XF] public void the_error_asks_whether_to_keep_or_delete_the_Anki_note() => TextOf(_result).Must().Contain("has an Anki note, which holds its cards and their review history. Say what to do with it");
         [XF] public void the_vocab_is_still_in_the_collection() => TheVocabIsInTheCollection().Must().BeTrue();
      }
   }

   public class suspending_all_cards_of_the_vocab : When_running_a_universal_note_action
   {
      readonly NoteCards _cards;

      public suspending_all_cards_of_the_vocab() => _cards = ResultOf<NoteCards>(Run("suspend_all_cards_of_a_note"));

      [XF] public void the_vocab_is_reported_with_no_active_cards() => _cards.HasActiveCards.Must().BeFalse();
   }
}
