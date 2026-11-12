using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

public class When_creating_a_note : McpServerSpecification
{
   protected CallToolResult Create(string noteType, string text) =>
      CallTool("create_a_note", new Dictionary<string, object?> { ["noteType"] = noteType, ["text"] = text });

   protected JPCollection Collection => GetService<JPCollection>();

   /// <summary>The sentence 時間を要する exists before the vocab is created.</summary>
   public class a_vocab_from_the_word_要する : When_creating_a_note
   {
      readonly SentenceNote _sentence;
      readonly NoteSummary _created;

      public a_vocab_from_the_word_要する()
      {
         _sentence = CreateSentence("時間を要する");
         _created = ResultOf<NoteSummary>(Create("vocab", "要する"));
      }

      [XF] public void the_vocab_created_is_reported() => _created.Must().Be(NoteSummary.Of(Collection.Vocab.WithQuestion("要する").Single()));
      [XF] public void the_vocab_has_the_dictionarys_reading() => Collection.Vocab.WithQuestion("要する").Single().GetReadings().Must().SequenceEqual(["ようする"]);
      [XF] public void the_sentence_that_contains_it_is_reparsed_and_matches_it() =>
         _sentence.GetParsingResult().ParsedWords.Must().Satisfy(parsedWords => parsedWords.Any(it => it.VocabId == Collection.Vocab.WithQuestion("要する").Single().GetId()));
   }

   public class a_sentence : When_creating_a_note
   {
      readonly NoteSummary _created;

      public a_sentence() => _created = ResultOf<NoteSummary>(Create("sentence", "時間を要する"));

      [XF] public void the_sentence_is_in_the_collection_with_the_text_as_its_question() => Collection.Sentences.WithQuestion("時間を要する").Single().GetId().ToString().Must().Be(_created.Id);
   }

   public class a_kanji : When_creating_a_note
   {
      readonly NoteSummary _created;

      public a_kanji() => _created = ResultOf<NoteSummary>(Create("kanji", "要"));

      [XF] public void the_kanji_is_created_with_the_answer_TODO_as_the_context_menu_creates_it() => _created.Answer.Must().Be("TODO");
      [XF] public void the_kanji_is_in_the_collection() => Collection.Kanji.WithKanji("要").Must().NotBeNull();
   }

   public class of_a_note_type_JAStudio_does_not_have : When_creating_a_note
   {
      readonly CallToolResult _result;

      public of_a_note_type_JAStudio_does_not_have() => _result = Create("grammar point", "ている");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_lists_the_note_types() => TextOf(_result).Must().Contain("The note types are \"vocab\", \"sentence\" and \"kanji\"");
   }
}
