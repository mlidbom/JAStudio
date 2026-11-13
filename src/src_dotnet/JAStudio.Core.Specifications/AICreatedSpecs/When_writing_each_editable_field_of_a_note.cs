using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.KanjiReadings;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Each editable field of a note type (<see cref="IEditableField{TNote}"/>) written, on a note of its own, with a value other than the note's, and the<br/>
/// note then serialized as the corpus stores it: a field's <see cref="IEditableField{TNote}.Name"/> must be where in the stored note the field is.
/// </summary>
public abstract class When_writing_each_editable_field_of_a_note : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   /// <summary>The names of <paramref name="fields"/> that aren't where the stored note changed when the field was written, each on a note <paramref name="createANote"/> creates.</summary>
   protected IReadOnlyList<string> FieldsWhoseNameIsNotWhereTheStoredNoteChanged<TNote>(IReadOnlyList<IEditableField<TNote>> fields, Func<int, TNote> createANote) where TNote : JPNote =>
      [.. fields.Select((field, index) => (field, index))
                .Where(it => !TheStoredNoteChangesAtTheNameOf(it.field, createANote(it.index)))
                .Select(it => it.field.Name)];

   public class of_a_vocab : When_writing_each_editable_field_of_a_note
   {
      readonly IReadOnlyList<string> _fieldsWhoseNameIsNotWhereTheStoredVocabChanged;

      public of_a_vocab()
      {
         CreateVocab("another word", "another", "あなざー"); //The word the word-set fields are written with: a perfect synonym must be a vocab.
         _fieldsWhoseNameIsNotWhereTheStoredVocabChanged = FieldsWhoseNameIsNotWhereTheStoredNoteChanged(VocabEditableFields.All, index => CreateVocab($"行く{index}", "to go", "いく"));
      }

      [XF] public void the_stored_vocab_changes_where_each_fields_name_says() => _fieldsWhoseNameIsNotWhereTheStoredVocabChanged.Must().BeEmpty();
   }

   public class of_a_kanji : When_writing_each_editable_field_of_a_note
   {
      readonly IReadOnlyList<string> _fieldsWhoseNameIsNotWhereTheStoredKanjiChanged;

      public of_a_kanji() => _fieldsWhoseNameIsNotWhereTheStoredKanjiChanged = FieldsWhoseNameIsNotWhereTheStoredNoteChanged(KanjiEditableFields.All, index => CreateKanji($"行{index}", "go", "コウ", "い"));

      [XF] public void the_stored_kanji_changes_where_each_fields_name_says() => _fieldsWhoseNameIsNotWhereTheStoredKanjiChanged.Must().BeEmpty();
   }

   public class of_a_sentence : When_writing_each_editable_field_of_a_note
   {
      readonly IReadOnlyList<string> _fieldsWhoseNameIsNotWhereTheStoredSentenceChanged;

      public of_a_sentence() => _fieldsWhoseNameIsNotWhereTheStoredSentenceChanged = FieldsWhoseNameIsNotWhereTheStoredNoteChanged(SentenceEditableFields.All, index => CreateSentence($"行った{index}"));

      [XF] public void the_stored_sentence_changes_where_each_fields_name_says() => _fieldsWhoseNameIsNotWhereTheStoredSentenceChanged.Must().BeEmpty();
   }

   bool TheStoredNoteChangesAtTheNameOf<TNote>(IEditableField<TNote> field, TNote note) where TNote : JPNote
   {
      var before = StoredValueAt(note, field.Name);
      field.WriteValue(note, AnotherValueThanTheNotesFor(field, note));
      return !JsonNode.DeepEquals(before, StoredValueAt(note, field.Name));
   }

   /// <summary>
   /// The value at <paramref name="name"/>, a path, in <paramref name="note"/> as the corpus stores it. A path that ends in a list's item, as a flag's<br/>
   /// does, such as <c>matching.flags.isPoisonWord</c>, gives whether the list holds the item.
   /// </summary>
   JsonNode? StoredValueAt(JPNote note, string name) =>
      name.Split('.').Aggregate(JsonNode.Parse(GetService<NoteSerializer>().Serialize(note)), (node, step) => node switch
      {
         JsonArray list => JsonValue.Create(list.Any(it => it?.GetValue<string>() == step)),
         JsonObject @object => @object[step],
         _ => throw new InvalidOperationException($"{name} goes past a value at {step}.")
      });

   static object AnotherValueThanTheNotesFor<TNote>(IEditableField<TNote> field, TNote note) where TNote : JPNote => field switch
   {
      { Name: "partsOfSpeech" }                                                => new List<string> { "noun" },
      { Name: "tags" }                                                         => new List<string> { "another::tag" },
      { Name: "matching.condition" }                                           => """{"fact": "singleToken"}""",
      _ when field.ValueType == typeof(string)                                 => "another text",
      _ when field.ValueType == typeof(bool)                                   => !(bool)field.ReadValue(note),
      _ when field.ValueType == typeof(IReadOnlyList<string>)                  => new List<string> { "another text" },
      _ when field.ValueType == typeof(IReadOnlyList<EnteredForm>)             => new List<EnteredForm> { new("another form", true) },
      _ when field.ValueType == typeof(IReadOnlyList<KanjiReading>)            => new List<KanjiReading> { new("あ", true) },
      _ when field.ValueType == typeof(IReadOnlyList<WordExclusion>)           => new List<WordExclusion> { WordExclusion.Global("another word") },
      _ when field.ValueType == typeof(IReadOnlyList<WordReference>)           => new List<WordReference> { new("another word") },
      { ValueType.IsEnum: true }                                               => Enum.GetValues(field.ValueType).Cast<object>().First(it => !it.Equals(field.ReadValue(note))),
      _                                                                        => throw new ArgumentOutOfRangeException(nameof(field), field.ValueType, "No value for this type of field.")
   };
}
