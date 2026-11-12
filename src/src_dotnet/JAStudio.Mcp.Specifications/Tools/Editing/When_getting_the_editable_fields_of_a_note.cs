using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Mcp.Tools.Editing;
using JAStudio.Mcp.Tools.Editing.Fields;
using ModelContextProtocol.Protocol;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Editing;

public class When_getting_the_editable_fields_of_a_note : McpServerSpecification
{
   protected CallToolResult GetTheEditableFieldsOf(string noteId) =>
      CallTool("get_the_editable_fields_of_a_note", new Dictionary<string, object?> { ["noteId"] = noteId });

   /// <summary>The vocab 行った, with the reading いった.</summary>
   public class of_a_vocab : When_getting_the_editable_fields_of_a_note
   {
      readonly EditableFieldsReported _fields;

      public of_a_vocab() => _fields = ResultOf<EditableFieldsReported>(GetTheEditableFieldsOf(CreateVocab("行った", "went", "いった").GetId().ToString()));

      EditableFieldReported Field(string name) => _fields.Fields.Single(it => it.Name == name);

      [XF] public void the_fields_are_reported_for_the_vocab() => _fields.Note.Must().Be("行った");
      [XF] public void every_field_the_vocabs_editor_edits_is_listed_by_its_name_in_order() => _fields.Fields.Select(it => it.Name).Must().SequenceEqual(VocabEditableFields.All.Select(it => it.Name));
      [XF] public void the_readings_are_listed_with_their_value() => JsonNode.DeepEquals(Field("readings").Value, new JsonArray("いった")).Must().BeTrue(); //todo:compze: Satisfy on the JSON once Must's failure message can serialize System.Text.Json values
      [XF] public void the_readings_take_a_list_of_texts() => Field("readings").Takes.Must().Be("a list of texts");
      [XF] public void the_readings_are_labeled_as_in_the_vocabs_editor() => Field("readings").Label.Must().Be("Readings");
      [XF] public void the_surface_rule_takes_unset_required_or_forbidden() => Field("matching.rules.surface").Takes.Must().Be("one of \"unset\", \"required\", \"forbidden\"");
      [XF] public void the_forms_change_how_sentences_parse() => Field("forms").ChangesHowSentencesParse.Must().BeTrue();
      [XF] public void the_answer_does_not_change_how_sentences_parse() => Field("corpusAnswer").ChangesHowSentencesParse.Must().BeFalse();
   }

   /// <summary>The kanji 行, with the on reading コウ and the kun reading い.</summary>
   public class of_a_kanji : When_getting_the_editable_fields_of_a_note
   {
      readonly EditableFieldsReported _fields;

      public of_a_kanji() => _fields = ResultOf<EditableFieldsReported>(GetTheEditableFieldsOf(CreateKanji("行", "go", "コウ", "い").GetId().ToString()));

      EditableFieldReported Field(string name) => _fields.Fields.Single(it => it.Name == name);

      [XF] public void every_field_the_kanjis_editor_edits_is_listed_by_its_name_in_order() => _fields.Fields.Select(it => it.Name).Must().SequenceEqual(KanjiEditableFields.All.Select(it => it.Name));
      [XF] public void the_on_readings_take_a_list_of_readings() => Field("readings.on").Takes.Must().Be("a list of readings, each {\"kana\": \"...\", \"isPrimary\": true or false}");
      [XF] public void the_on_reading_is_listed_in_hiragana() => JsonNode.DeepEquals(Field("readings.on").Value?[0]?["kana"], JsonValue.Create("こう")).Must().BeTrue(); //todo:compze: Satisfy on the JSON once Must's failure message can serialize System.Text.Json values
   }

   /// <summary>The sentence 行った.</summary>
   public class of_a_sentence : When_getting_the_editable_fields_of_a_note
   {
      readonly EditableFieldsReported _fields;

      public of_a_sentence() => _fields = ResultOf<EditableFieldsReported>(GetTheEditableFieldsOf(CreateSentence("行った").GetId().ToString()));

      [XF] public void every_field_the_sentences_editor_edits_is_listed_by_its_name_in_order() => _fields.Fields.Select(it => it.Name).Must().SequenceEqual(SentenceEditableFields.All.Select(it => it.Name));
   }
}
