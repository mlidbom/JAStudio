using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A vocab with an owned and a not owned entered form, written as the corpus's JSON and read back, as JAStudio saves and loads the corpus.</summary>
public class When_a_vocab_with_an_owned_form_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly string _json;
   readonly VocabNote _readBack;

   public When_a_vocab_with_an_owned_form_is_saved_to_the_corpus()
   {
      var vocab = CreateVocab("走る", "to run", "はしる");
      vocab.Forms.Add("駆ける");
      vocab.Forms.Own("駆ける");
      vocab.Forms.Add("奔る");
      var serializer = GetService<NoteSerializer>();
      _json = serializer.Serialize(vocab);
      _readBack = serializer.DeserializeVocab(_json);
   }

   [XF] public void it_is_read_back_with_both_entered_forms_owned_as_they_were() =>
      _readBack.Forms.EnteredForms().Must().SequenceEqual([EnteredForm.Owned("駆ける"), EnteredForm.NotOwned("奔る")]);

   [XF] public void the_corpus_json_stores_each_form_as_its_text_and_whether_it_is_owned() =>
      FormsInTheJson().Must().SequenceEqual(["駆ける:True", "奔る:False"]);

   string[] FormsInTheJson() =>
   [
      .. JsonDocument.Parse(_json).RootElement.GetProperty("forms").EnumerateArray()
                     .Select(it => $"{it.GetProperty("text").GetString()}:{it.GetProperty("isOwned").GetBoolean()}")
   ];
}
