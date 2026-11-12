using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.Matching;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Tools.Editing;
using JAStudio.Mcp.Tools.Editing.Fields;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

/// <summary>The vocab 行った, with the reading いいた and the answer "went", and the sentence 会いに行った, which contains it, with no answer.</summary>
public class When_editing_a_note : McpServerSpecification
{
   protected readonly VocabNote Vocab;
   protected readonly SentenceNote Sentence;

   public When_editing_a_note()
   {
      Vocab = CreateVocab("行った", "went", "いいた");
      Sentence = CreateSentence("会いに行った");
   }

   protected CallToolResult Edit(JsonObject changes) => Edit(Vocab, changes);

   protected CallToolResult Edit(JPNote note, JsonObject changes) =>
      CallTool("edit_a_note", new Dictionary<string, object?> { ["noteId"] = note.GetId().ToString(), ["changes"] = changes });

   protected VocabNote VocabStored() => GetService<INoteRepository>().LoadAll().Vocab.Single(it => it.GetId() == Vocab.GetId());

   protected List<ParsedMatch> TheSentencesParsedWords() => [.. Sentence.GetParsingResult().ParsedWords];

   public class of_a_vocab_writing_its_readings_its_answer_and_its_surface_rule : When_editing_a_note
   {
      readonly NoteEdit _edit;

      public of_a_vocab_writing_its_readings_its_answer_and_its_surface_rule() =>
         _edit = ResultOf<NoteEdit>(Edit(new JsonObject
         {
            ["readings"] = new JsonArray("いった", "おこなった"),
            ["corpusAnswer"] = "went/left",
            ["matching.rules.surface"] = "required"
         }));

      [XF] public void the_edit_names_the_vocab() => _edit.Note.Must().Be("行った");
      [XF] public void the_edit_reports_each_field_that_changed_in_the_order_of_the_vocabs_fields() => _edit.Changes.Select(it => it.Field).Must().SequenceEqual(["readings", "corpusAnswer", "matching.rules.surface", "matching.condition"]);
      [XF] public void the_edit_reports_the_readings_before() => _edit.Changes[0].Before.Must().Satisfy(it => JsonNode.DeepEquals(it, new JsonArray("いいた")));
      [XF] public void the_edit_reports_the_readings_after() => _edit.Changes[0].After.Must().Satisfy(it => JsonNode.DeepEquals(it, new JsonArray("いった", "おこなった")));
      [XF] public void the_edit_reports_the_surface_rule_before_as_unset() => _edit.Changes[2].Before.Must().Satisfy(it => JsonNode.DeepEquals(it, JsonValue.Create("unset")));
      [XF] public void the_vocab_has_the_readings() => Vocab.GetReadings().Must().SequenceEqual(["いった", "おこなった"]);
      [XF] public void the_vocab_has_the_answer() => Vocab.CorpusAnswer.Value.Must().Be("went/left");
      [XF] public void the_vocab_requires_the_surface() => Vocab.MatchingConfiguration.Rules.ChoiceOf(MatchingRules.Surface).Must().Be(RequireForbidChoice.Required);
      [XF] public void the_vocab_is_saved_with_the_answer() => VocabStored().CorpusAnswer.Value.Must().Be("went/left");
      [XF] public void the_sentences_that_may_contain_the_vocab_are_reparsed_since_the_surface_rule_changes_how_sentences_parse() => _edit.SentencesReparsed.Must().BeTrue();
   }

   public class of_a_vocab_writing_only_its_answer : When_editing_a_note
   {
      readonly NoteEdit _edit;

      public of_a_vocab_writing_only_its_answer() => _edit = ResultOf<NoteEdit>(Edit(new JsonObject { ["corpusAnswer"] = "went/left" }));

      [XF] public void no_sentences_are_reparsed() => _edit.SentencesReparsed.Must().BeFalse();
   }

   public class of_a_vocab_making_it_a_poison_word : When_editing_a_note
   {
      readonly List<ParsedMatch> _theSentencesParsedWordsBefore;

      public of_a_vocab_making_it_a_poison_word()
      {
         _theSentencesParsedWordsBefore = TheSentencesParsedWords();
         ResultOf<NoteEdit>(Edit(new JsonObject { ["matching.flags.isPoisonWord"] = true }));
      }

      [XF] public void the_sentence_matched_the_vocab_before() => _theSentencesParsedWordsBefore.Must().Satisfy(words => words.Any(it => it.VocabId == Vocab.GetId()));
      [XF] public void the_sentence_is_reparsed_and_no_longer_matches_the_vocab() => TheSentencesParsedWords().Must().Satisfy(words => !words.Any(it => it.VocabId == Vocab.GetId()));
   }

   public class of_a_vocab_writing_its_forms : When_editing_a_note
   {
      public of_a_vocab_writing_its_forms() =>
         ResultOf<NoteEdit>(Edit(new JsonObject { ["forms"] = new JsonArray(new JsonObject { ["text"] = "いった", ["isOwned"] = true }, new JsonObject { ["text"] = "往った", ["isOwned"] = false }) }));

      [XF] public void the_vocab_has_the_forms_with_their_ownership() => Vocab.Forms.EnteredForms().OrderBy(it => it.Text).Must().SequenceEqual([new EnteredForm("いった", true), new EnteredForm("往った", false)]);
   }

   public class of_a_vocab_writing_its_forms_with_its_question_among_them : When_editing_a_note
   {
      public of_a_vocab_writing_its_forms_with_its_question_among_them() =>
         ResultOf<NoteEdit>(Edit(new JsonObject { ["forms"] = new JsonArray(new JsonObject { ["text"] = "行った", ["isOwned"] = true }, new JsonObject { ["text"] = "いった", ["isOwned"] = false }) }));

      [XF] public void the_question_is_left_out_of_the_entered_forms_since_it_is_always_a_form() => Vocab.Forms.EnteredForms().Must().SequenceEqual([new EnteredForm("いった", false)]);
   }

   public class of_a_kanji_writing_its_on_readings_in_katakana : When_editing_a_note
   {
      readonly KanjiNote _kanji;
      readonly NoteEdit _edit;

      public of_a_kanji_writing_its_on_readings_in_katakana()
      {
         _kanji = CreateKanji("行", "go", "コウ", "い");
         _edit = ResultOf<NoteEdit>(Edit(_kanji, new JsonObject
         {
            ["readings.on"] = new JsonArray(new JsonObject { ["kana"] = "ギョウ", ["isPrimary"] = true }, new JsonObject { ["kana"] = "コウ", ["isPrimary"] = false })
         }));
      }

      [XF] public void the_edit_names_the_kanji() => _edit.Note.Must().Be("行");
      [XF] public void the_kanji_has_the_on_readings_in_hiragana_in_the_order_given_each_primary_or_not() => _kanji.Readings.On.All().Must().SequenceEqual([new KanjiReading("ぎょう", true), new KanjiReading("こう", false)]);
      [XF] public void no_sentences_are_reparsed() => _edit.SentencesReparsed.Must().BeFalse();
   }

   public class of_a_sentence_writing_its_answer : When_editing_a_note
   {
      readonly NoteEdit _edit;

      public of_a_sentence_writing_its_answer() => _edit = ResultOf<NoteEdit>(Edit(Sentence, new JsonObject { ["corpusAnswer"] = "I went to meet them." }));

      [XF] public void the_edit_reports_the_answer_before_as_empty() => _edit.Changes.Single().Before.Must().Satisfy(it => JsonNode.DeepEquals(it, JsonValue.Create("")));
      [XF] public void the_sentence_has_the_answer() => Sentence.CorpusAnswer.Value.Must().Be("I went to meet them.");
   }

   /// <summary>Another vocab, 行く, with the answer "to go".</summary>
   public class of_a_vocab_writing_its_synonyms : When_editing_a_note
   {
      readonly VocabNote _synonym;
      readonly NoteEdit _edit;

      public of_a_vocab_writing_its_synonyms()
      {
         _synonym = CreateVocab("行く", "to go", "いく");
         _edit = ResultOf<NoteEdit>(Edit(new JsonObject { ["relatedVocab.synonyms"] = new JsonArray(new JsonObject { ["text"] = "行く" }) }));
      }

      [XF] public void the_vocab_has_the_synonym() => Vocab.RelatedNotes.Synonyms.Words.Must().SequenceEqual([new WordReference("行く")]);
      [XF] public void the_synonym_has_the_vocab_as_its_synonym_too_as_when_a_person_adds_it() => _synonym.RelatedNotes.Synonyms.Words.Must().SequenceEqual([new WordReference("行った")]);

      [XF] public void the_edit_reports_the_synonyms_after_as_word_references_with_an_empty_disambiguation() =>
         _edit.Changes.Single().After.Must().Satisfy(it => JsonNode.DeepEquals(it, new JsonArray(new JsonObject { ["text"] = "行く", ["disambiguation"] = "" })));
   }

   /// <summary>Another vocab, 往った, with the answer "gone".</summary>
   public class of_a_vocab_writing_its_perfect_synonyms : When_editing_a_note
   {
      readonly VocabNote _perfectSynonym;
      readonly NoteEdit _edit;

      public of_a_vocab_writing_its_perfect_synonyms()
      {
         _perfectSynonym = CreateVocab("往った", "gone", "いった");
         _edit = ResultOf<NoteEdit>(Edit(new JsonObject { ["relatedVocab.perfectSynonyms"] = new JsonArray(new JsonObject { ["text"] = "往った" }) }));
      }

      [XF] public void the_perfect_synonym_gets_the_vocabs_answer() => _perfectSynonym.GetAnswer().Must().Be("went");
      [XF] public void the_edit_reports_the_synonyms_too_since_adding_a_perfect_synonym_adds_a_synonym() => _edit.Changes.Select(it => it.Field).Must().SequenceEqual(["relatedVocab.synonyms", "relatedVocab.perfectSynonyms"]);
   }

   public class of_a_sentence_marking_the_vocab_an_incorrect_match : When_editing_a_note
   {
      readonly List<ParsedMatch> _theSentencesParsedWordsBefore;
      readonly NoteEdit _edit;

      public of_a_sentence_marking_the_vocab_an_incorrect_match()
      {
         _theSentencesParsedWordsBefore = TheSentencesParsedWords();
         _edit = ResultOf<NoteEdit>(Edit(Sentence, new JsonObject { ["configuration.incorrectMatches"] = new JsonArray(new JsonObject { ["word"] = "行った" }) }));
      }

      [XF] public void the_sentence_matched_the_vocab_before() => _theSentencesParsedWordsBefore.Must().Satisfy(words => words.Any(it => it.VocabId == Vocab.GetId()));
      [XF] public void the_sentence_is_reparsed_and_no_longer_matches_the_vocab() => TheSentencesParsedWords().Must().Satisfy(words => !words.Any(it => it.VocabId == Vocab.GetId()));
      [XF] public void the_edit_reports_the_incorrect_match_wherever_it_is_as_at_the_index_minus_one() =>
         _edit.Changes.Single().After.Must().Satisfy(it => JsonNode.DeepEquals(it, new JsonArray(new JsonObject { ["word"] = "行った", ["index"] = -1 })));
   }

   public class of_a_vocab_with_a_part_of_speech_JAStudio_does_not_know : When_editing_a_note
   {
      readonly CallToolResult _result;

      public of_a_vocab_with_a_part_of_speech_JAStudio_does_not_know() => _result = Edit(new JsonObject { ["partsOfSpeech"] = new JsonArray("expression", "suru verb - special class") });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_unknown_part_of_speech() => TextOf(_result).Must().Contain("There is no part of speech named 'suru verb - special class'");
      [XF] public void the_error_lists_the_parts_of_speech_JAStudio_knows() => TextOf(_result).Must().Contain("\"godan verb\"");
   }

   public class of_a_vocab_naming_a_field_a_vocab_does_not_have : When_editing_a_note
   {
      readonly CallToolResult _result;

      public of_a_vocab_naming_a_field_a_vocab_does_not_have() => _result = Edit(new JsonObject { ["corpusAnswer"] = "went/left", ["answer"] = "went/left" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void the_error_names_the_unknown_field() => TextOf(_result).Must().Contain("A vocab has no editable field named 'answer'");
      [XF] public void the_error_lists_the_editable_fields_of_a_vocab() => TextOf(_result).Must().Contain("\"corpusAnswer\"");
      [XF] public void no_field_is_written() => Vocab.CorpusAnswer.Value.Must().Be("went");
   }

   public class of_a_vocab_giving_a_flag_a_text : When_editing_a_note
   {
      readonly CallToolResult _result;

      public of_a_vocab_giving_a_flag_a_text() => _result = Edit(new JsonObject { ["corpusAnswer"] = "went/left", ["matching.flags.isPoisonWord"] = "yes" });

      [XF] public void the_tool_call_fails() => _result.IsError.Must().Be(true);
      [XF] public void no_field_is_written() => Vocab.CorpusAnswer.Value.Must().Be("went");
   }
}
