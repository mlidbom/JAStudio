using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Editing.Commands;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

/// <summary>The vocab 勉強, with the reading べんきょう and the answer "study".</summary>
public class When_running_a_command_on_a_note : McpServerSpecification
{
   protected readonly VocabNote Vocab;

   public When_running_a_command_on_a_note() => Vocab = CreateVocab("勉強", "study", "べんきょう");

   protected CallToolResult Run(JPNote note, string command, string? text = null) =>
      CallTool("run_a_command_on_a_note", new Dictionary<string, object?> { ["noteId"] = note.GetId().ToString(), ["command"] = command, ["text"] = text });

   protected VocabNote TheVocabWithTheQuestion(string question) => GetService<JPCollection>().Vocab.WithQuestion(question).Single();

   /// <summary>Its answer is the source's, "To study, Learning", the corpus having none of its own.</summary>
   public class accepting_the_meaning_of_a_vocab : When_running_a_command_on_a_note
   {
      readonly NoteCommandRun _run;

      public accepting_the_meaning_of_a_vocab()
      {
         Vocab.CorpusAnswer.Set("");
         Vocab.SourceData.Answer.Set("To study, Learning");
         _run = ResultOf<NoteCommandRun>(Run(Vocab, "acceptMeaning"));
      }

      [XF] public void the_vocab_has_the_sources_answer_as_its_own_written_as_the_corpus_writes_answers() => Vocab.CorpusAnswer.Value.Must().Be("to-study/learning");
      [XF] public void the_run_reports_the_answer_changed() => _run.Changes.Single().Field.Must().Be("corpusAnswer");
      [XF] public void the_run_reports_no_vocab_created() => _run.VocabCreated.Must().BeEmpty();
   }

   public class accepting_the_meaning_of_a_vocab_with_an_answer_of_its_own : When_running_a_command_on_a_note
   {
      readonly CallToolResult _result;

      public accepting_the_meaning_of_a_vocab_with_an_answer_of_its_own() => _result = Run(Vocab, "acceptMeaning");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_says_the_vocab_has_an_answer_of_its_own() => TextOf(_result).Must().Contain("has an answer of its own already");
      [XF] public void the_answer_is_unchanged() => Vocab.CorpusAnswer.Value.Must().Be("study");
   }

   public class creating_the_suru_verb_of_a_vocab : When_running_a_command_on_a_note
   {
      readonly NoteCommandRun _run;

      public creating_the_suru_verb_of_a_vocab() => _run = ResultOf<NoteCommandRun>(Run(Vocab, "createSuruVerb"));

      [XF] public void the_run_reports_the_vocab_created() => _run.VocabCreated.Must().SequenceEqual([NoteSummary.Of(TheVocabWithTheQuestion("勉強する"))]);
      [XF] public void the_vocab_created_is_in_the_collection_with_the_reading_suffixed_too() => TheVocabWithTheQuestion("勉強する").GetReadings().Must().SequenceEqual(["べんきょうする"]);
      [XF] public void the_run_reports_no_change_to_the_vocab_it_was_created_from() => _run.Changes.Must().BeEmpty();
   }

   public class suffixing_a_text_onto_a_vocab : When_running_a_command_on_a_note
   {
      public suffixing_a_text_onto_a_vocab() => ResultOf<NoteCommandRun>(Run(Vocab, "suffixOntoDictionaryForm", "中"));

      [XF] public void a_vocab_with_the_text_suffixed_onto_the_question_and_the_reading_is_created() => TheVocabWithTheQuestion("勉強中").GetReadings().Must().SequenceEqual(["べんきょう中"]);
   }

   public class suffixing_onto_a_vocab_without_giving_a_text : When_running_a_command_on_a_note
   {
      readonly CallToolResult _result;

      public suffixing_onto_a_vocab_without_giving_a_text() => _result = Run(Vocab, "suffixOntoDictionaryForm");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_says_what_text_the_command_takes() => TextOf(_result).Must().Contain("takes a text: the text to suffix onto the vocab");
   }

   public class naming_a_command_a_vocab_does_not_have : When_running_a_command_on_a_note
   {
      readonly CallToolResult _result;

      public naming_a_command_a_vocab_does_not_have() => _result = Run(Vocab, "createPastTense");

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_unknown_command() => TextOf(_result).Must().Contain("A vocab has no command named 'createPastTense'");
      [XF] public void the_error_lists_the_commands_of_a_vocab() => TextOf(_result).Must().Contain("\"createTaForm\"");
   }

   /// <summary>The sentence 会いに行った.</summary>
   public class splitting_a_word_of_a_sentence_with_a_word_break_tag : When_running_a_command_on_a_note
   {
      readonly NoteCommandRun _run;

      public splitting_a_word_of_a_sentence_with_a_word_break_tag() => _run = ResultOf<NoteCommandRun>(Run(CreateSentence("会いに行った"), "splitWithAWordBreakTag", "行った"));

      [XF] public void the_question_has_a_word_break_tag_after_the_words_first_character() =>
         JsonNode.DeepEquals(_run.Changes.Single().After, JsonValue.Create("会いに行<wbr>った")).Must().BeTrue();
   }
}
