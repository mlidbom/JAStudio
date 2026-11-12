using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Corpus;

/// <summary>
/// The corpus repository has a commit with the files of the vocab 行く and 来る, the kanji 行 and the sentence 明日学校に行く. Since then, the<br/>
/// form 逝く has been added to 行く, the vocab 学校 has been added, and the sentence hides the vocab 行く, so that the sentence, parsed again,<br/>
/// matches its 学校 to the vocab 学校 and no longer displays its 行く. 行's file has been deleted, and 来る's file has been written again on a<br/>
/// single line, with the note unchanged.
/// </summary>
public class When_diffing_the_changed_notes : CorpusRepositorySpecification
{
   protected readonly string FirstCommit;
   protected readonly VocabNote Vocab学校;

   public When_diffing_the_changed_notes()
   {
      var vocab行く = CreateVocab("行く", "to go", "いく");
      var vocab来る = CreateVocab("来る", "to come", "くる");
      var kanji行 = CreateKanji("行", "go", "コウ", "い");
      var sentence = CreateTestSentence("明日学校に行く", "I'll go to school tomorrow");
      WriteTheFilesOf(vocab行く, vocab来る, kanji行, sentence);
      FirstCommit = CommitEverything();

      vocab行く.Forms.Add("逝く");
      Vocab学校 = CreateVocab("学校", "school", "がっこう");
      sentence.Configuration.HiddenMatches.AddGlobal("行く");
      sentence.UpdateParsedWords(force: true);
      WriteTheFilesOf(vocab行く, Vocab学校, sentence);
      FileOf(kanji行).Delete();
      Write(FileOf(vocab来る), JsonNode.Parse(File.ReadAllText(FileOf(vocab来る).FullName))?.ToJsonString() ?? "");
   }

   protected NotesDiff Diff(Dictionary<string, object?> arguments) => ResultOf<NotesDiff>(CallTool("diff_the_changed_notes", arguments));

   protected string IdOfTheVocab学校 => Vocab学校.GetId().ToString();

   public class comparing_the_head_with_the_working_tree : When_diffing_the_changed_notes
   {
      readonly NotesDiff _diff;

      public comparing_the_head_with_the_working_tree() => _diff = Diff([]);

      NoteDiff TheVocab(string question) => _diff.Vocab.Notes.Single(it => it.Question == question);
      NoteDiff TheSentence => _diff.Sentences.Notes.Single();

      [XF] public void the_diff_is_from_the_head_commit_by_its_hash() => _diff.From.Must().Be(FirstCommit);
      [XF] public void the_diff_is_to_the_working_tree() => _diff.To.Must().Be("the working tree");

      [XF] public void the_vocab_行く_is_changed() => TheVocab("行く").Change.Must().Be(NoteChangeKind.Changed);
      [XF] public void the_only_list_of_行く_that_changed_is_its_forms() => TheVocab("行く").ListsChanged.Select(it => it.Property).Must().SequenceEqual(["forms"]);
      [XF] public void the_form_逝く_is_added_to_the_forms_of_行く() => TheVocab("行く").ListsChanged.Single().Added.Single()?["text"]?.GetValue<string>().Must().Be("逝く");
      [XF] public void no_value_of_行く_changed() => TheVocab("行く").ValuesChanged.Must().BeEmpty();
      [XF] public void the_vocab_学校_is_added() => TheVocab("学校").Change.Must().Be(NoteChangeKind.Added);
      [XF] public void one_vocab_is_counted_as_changed() => _diff.Vocab.Changed.Must().Be(1);
      [XF] public void one_vocab_is_counted_as_added() => _diff.Vocab.Added.Must().Be(1);

      [XF] public void the_vocab_来る_whose_file_was_written_differently_is_counted_as_changed_only_in_its_file() => _diff.Vocab.ChangedOnlyInTheirFiles.Must().Be(1);
      [XF] public void the_vocab_来る_is_not_listed() => _diff.Vocab.Notes.Must().Satisfy(notes => !notes.Any(it => it.Question == "来る"));

      [XF] public void the_forms_are_the_one_property_of_vocab_counted_as_changed_in_one_vocab() =>
         _diff.Vocab.PropertiesChanged.Select(it => (it.Property, it.Notes)).Must().SequenceEqual([("forms", 1)]);

      [XF] public void the_kanji_行_whose_file_was_deleted_is_listed_as_deleted_by_its_question() =>
         _diff.Kanji.Notes.Select(it => (it.Question, it.Change)).Must().SequenceEqual([("行", NoteChangeKind.Deleted)]);

      [XF] public void the_parsed_word_学校_matched_to_the_vocab_学校_is_added_to_the_sentence() =>
         TheSentence.ParsedWords?.Added.Select(it => (it.Form, it.StartIndex, it.Vocab)).Must().SequenceEqual([("学校", 2, (string?)"学校")]);

      [XF] public void the_parsed_word_学校_matched_to_no_vocab_is_removed_from_the_sentence() =>
         TheSentence.ParsedWords?.Removed.Select(it => (it.Form, it.StartIndex, it.VocabId)).Must().SequenceEqual([("学校", 2, (string?)null)]);

      [XF] public void the_parsed_word_行く_matched_to_the_vocab_行く_is_no_longer_displayed_in_the_sentence() =>
         TheSentence.ParsedWords?.NoLongerDisplayed.Select(it => (it.Form, it.Vocab)).Must().SequenceEqual([("行く", (string?)"行く")]);

      [XF] public void no_parsed_word_is_now_displayed_in_the_sentence() => TheSentence.ParsedWords?.NowDisplayed.Must().BeEmpty();

      [XF] public void the_hidden_matches_and_the_parsed_words_are_the_properties_of_sentences_counted_as_changed() =>
         _diff.Sentences.PropertiesChanged.Select(it => it.Property).Order().Must().SequenceEqual(["configuration.hiddenMatches", "parsingResult.parsedWords"]);

      [XF] public void the_parsed_words_changed_by_word_count_one_added_for_the_vocab_学校() =>
         _diff.ParsedWordsChangedByWord.Single(it => it.VocabId == IdOfTheVocab学校).Added.Must().Be(1);

      [XF] public void the_parsed_words_changed_by_word_count_one_removed_for_the_word_学校_matched_to_no_vocab() =>
         _diff.ParsedWordsChangedByWord.Single(it => it.VocabId == null).Removed.Must().Be(1);
   }

   public class comparing_the_commit_before_the_head_with_the_head_by_their_names_after_committing_the_changes : When_diffing_the_changed_notes
   {
      readonly string _secondCommit;
      readonly NotesDiff _diff;

      public comparing_the_commit_before_the_head_with_the_head_by_their_names_after_committing_the_changes()
      {
         _secondCommit = CommitEverything("The changes");
         _diff = Diff(new Dictionary<string, object?> { ["from"] = "HEAD~1", ["to"] = "HEAD" });
      }

      [XF] public void the_diff_is_from_the_first_commit_by_its_hash() => _diff.From.Must().Be(FirstCommit);
      [XF] public void the_diff_is_to_the_second_commit_by_its_hash() => _diff.To.Must().Be(_secondCommit);
      [XF] public void the_vocab_学校_is_counted_as_added() => _diff.Vocab.Added.Must().Be(1);
      [XF] public void the_kanji_行_is_counted_as_deleted() => _diff.Kanji.Deleted.Must().Be(1);

      [XF] public void the_parsed_words_changed_by_word_count_one_added_for_the_vocab_学校() =>
         _diff.ParsedWordsChangedByWord.Single(it => it.VocabId == IdOfTheVocab学校).Added.Must().Be(1);
   }

   public class comparing_from_a_name_that_names_no_commit : When_diffing_the_changed_notes
   {
      readonly CallToolResult _result;

      public comparing_from_a_name_that_names_no_commit() =>
         _result = CallTool("diff_the_changed_notes", new Dictionary<string, object?> { ["from"] = "no-such-branch" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_name() => TextOf(_result).Must().Contain("no-such-branch");
   }
}
