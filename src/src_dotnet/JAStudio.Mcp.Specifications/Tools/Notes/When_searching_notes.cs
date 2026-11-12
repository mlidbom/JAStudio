using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Notes;

/// <summary>Searches a collection of the kanji 行, the vocab 行く, and the sentences 明日学校に行く and 家に帰る.</summary>
public class When_searching_notes : McpServerSpecification
{
   readonly KanjiNote _kanji行;
   readonly VocabNote _vocab行く;
   readonly SentenceNote _sentence明日学校に行く;

   public When_searching_notes()
   {
      _sentence明日学校に行く = CreateTestSentence("明日学校に行く", "I'll go to school tomorrow");
      CreateTestSentence("家に帰る", "Go home");
      _vocab行く = CreateVocab("行く", "to go", "いく");
      _kanji行 = CreateKanji("行", "go", "コウ", "い");
   }

   protected CallToolResult SearchNotes(JsonObject search, int? maxNotesListed = null) =>
      CallTool("search_notes", maxNotesListed == null
                                  ? new Dictionary<string, object?> { ["search"] = search }
                                  : new Dictionary<string, object?> { ["search"] = search, ["maxNotesListed"] = maxNotesListed });

   static JsonObject QuestionContains行 => new() { ["text"] = "Question", ["contains"] = "行" };

   public class for_the_notes_whose_question_contains_行 : When_searching_notes
   {
      readonly FoundNotes _found;
      public for_the_notes_whose_question_contains_行() => _found = ResultOf<FoundNotes>(SearchNotes(QuestionContains行));

      [XF] public void the_three_notes_are_found() => _found.NotesFound.Must().Be(3);

      [XF] public void they_are_listed_by_note_type_then_question_kanji_first() =>
         _found.Notes.Select(it => it.Question).Must().SequenceEqual(["行", "明日学校に行く", "行く"]);

      [XF] public void each_is_listed_with_its_note_type_as_a_search_names_it() =>
         _found.Notes.Select(it => it.NoteType).Must().SequenceEqual(["kanji", "sentence", "vocab"]);

      [XF] public void each_is_listed_with_its_answer() =>
         _found.Notes.Select(it => it.Answer).Must().SequenceEqual([_kanji行.GetAnswer(), "I'll go to school tomorrow", _vocab行く.GetAnswer()]);

      [XF] public void each_is_listed_with_its_id_which_get_note_takes() =>
         _found.Notes.Select(it => NoteId.Parse(it.Id)).Must().SequenceEqual([_kanji行.GetId(), _sentence明日学校に行く.GetId(), _vocab行く.GetId()]);
   }

   public class listing_at_most_one_note : When_searching_notes
   {
      readonly FoundNotes _found;
      public listing_at_most_one_note() => _found = ResultOf<FoundNotes>(SearchNotes(QuestionContains行, maxNotesListed: 1));

      [XF] public void all_three_notes_found_are_counted() => _found.NotesFound.Must().Be(3);
      [XF] public void only_the_first_in_order_is_listed() => _found.Notes.Select(it => it.Question).Must().SequenceEqual(["行"]);
   }

   public class with_a_condition_of_an_unknown_kind : When_searching_notes
   {
      readonly CallToolResult _result;
      public with_a_condition_of_an_unknown_kind() => _result = SearchNotes(new JsonObject { ["nonsense"] = "x" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);

      [XF] public void the_whole_exception_is_reported_with_its_type_and_message() =>
         //bug: failed once in a dotnet test run of the whole solution (2026-11-12), and passed alone three times and in two runs of the whole project
         //   just after. Its failure message wasn't kept, so what the reported text held is unknown: rerun it under load to catch it.
         TextOf(_result).Must().Contain("System.FormatException: A condition of the unknown kind nonsense");
   }
}
