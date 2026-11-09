using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note.Vocabulary.EditableFields;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// Each of the <see cref="VocabEditableFields"/> written, on a vocab of its own, with a value other than the vocab's, and the vocab then serialized<br/>
/// as the corpus stores it: a field's <see cref="IEditableField{TNote}.Name"/> must be where in the stored vocab the field is.
/// </summary>
public class When_writing_each_editable_field_of_a_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly IReadOnlyList<string> _fieldsWhoseNameIsNotWhereTheStoredVocabChanged;

   public When_writing_each_editable_field_of_a_vocab() =>
      _fieldsWhoseNameIsNotWhereTheStoredVocabChanged = [.. VocabEditableFields.All.Select((field, index) => (field, index))
                                                                                .Where(it => !TheStoredVocabChangesAtTheNameOf(it.field, $"行く{it.index}"))
                                                                                .Select(it => it.field.Name)];

   [XF] public void the_stored_vocab_changes_where_each_fields_name_says() => _fieldsWhoseNameIsNotWhereTheStoredVocabChanged.Must().BeEmpty();

   bool TheStoredVocabChangesAtTheNameOf(IEditableField<VocabNote> field, string question)
   {
      var vocab = CreateVocab(question, "to go", "いく");
      var before = StoredValueAt(vocab, field.Name);
      field.WriteValue(vocab, AnotherValueThanTheVocabsFor(field, vocab));
      return !JsonNode.DeepEquals(before, StoredValueAt(vocab, field.Name));
   }

   /// <summary>
   /// The value at <paramref name="name"/>, a path, in <paramref name="vocab"/> as the corpus stores it. A path that ends in a list's item, as a flag's<br/>
   /// does, such as <c>matching.flags.isPoisonWord</c>, gives whether the list holds the item.
   /// </summary>
   JsonNode? StoredValueAt(VocabNote vocab, string name) =>
      name.Split('.').Aggregate(JsonNode.Parse(GetService<NoteSerializer>().Serialize(vocab)), (node, step) => node switch
      {
         JsonArray list => JsonValue.Create(list.Any(it => it?.GetValue<string>() == step)),
         JsonObject @object => @object[step],
         _ => throw new InvalidOperationException($"{name} goes past a value at {step}.")
      });

   static object AnotherValueThanTheVocabsFor(IEditableField<VocabNote> field, VocabNote vocab) => field.ValueType switch
   {
      _ when field == VocabEditableFields.PartsOfSpeech                => new List<string> { "noun" },
      _ when field == VocabEditableFields.Tags                         => new List<string> { "another::tag" },
      _ when field.ValueType == typeof(string)                         => "another text",
      _ when field.ValueType == typeof(bool)                           => !(bool)field.ReadValue(vocab),
      _ when field.ValueType == typeof(IReadOnlyList<string>)          => new List<string> { "another text" },
      _ when field.ValueType == typeof(IReadOnlyList<EnteredForm>)     => new List<EnteredForm> { new("another form", true) },
      { IsEnum: true } enumType                                        => Enum.GetValues(enumType).Cast<object>().First(it => !it.Equals(field.ReadValue(vocab))),
      _                                                                => throw new ArgumentOutOfRangeException(nameof(field), field.ValueType, "No value for this type of field.")
   };
}
