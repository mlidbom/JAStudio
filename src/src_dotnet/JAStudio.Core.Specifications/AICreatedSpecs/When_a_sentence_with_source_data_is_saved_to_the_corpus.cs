using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A sentence with what its source gave it and an answer of the corpus's own, written as the corpus's JSON and read back, as JAStudio saves and loads<br/>
/// the corpus. The source's values are kept apart, in <c>sourceData</c>; the answer and question shown aren't stored, since they follow from the others.
/// </summary>
public class When_a_sentence_with_source_data_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly JsonElement _json;
   readonly SentenceNote _readBack;

   public When_a_sentence_with_source_data_is_saved_to_the_corpus()
   {
      var sentence = CreateTestSentence("川に行く", "go to the river");
      sentence.SourceData.Answer.Set("I go to the river");
      sentence.SourceData.Comments.Set("A comment from the source");
      sentence.SourceData.ExternalId.Set("anime_the_garden_of_words_000000346");
      sentence.Comments.Set("A comment of the corpus's own");
      var serializer = GetService<NoteSerializer>();
      var json = serializer.Serialize(sentence);
      _json = JsonDocument.Parse(json).RootElement;
      _readBack = serializer.DeserializeSentence(json);
   }

   string SourceDataProperty(string name) => _json.GetProperty("sourceData").GetProperty(name).ToString();

   [XF] public void the_source_question_is_stored_in_the_source_data() => SourceDataProperty("question").Must().Be("川に行く");
   [XF] public void the_source_answer_is_stored_in_the_source_data() => SourceDataProperty("answer").Must().Be("I go to the river");
   [XF] public void the_source_comments_are_stored_in_the_source_data() => SourceDataProperty("comments").Must().Be("A comment from the source");
   [XF] public void the_external_id_is_stored_in_the_source_data() => SourceDataProperty("externalId").Must().Be("anime_the_garden_of_words_000000346");
   [XF] public void the_corpus_answer_is_stored_on_the_sentence() => _json.GetProperty("corpusAnswer").ToString().Must().Be("go to the river");
   [XF] public void the_corpus_comments_are_stored_on_the_sentence() => _json.GetProperty("comments").ToString().Must().Be("A comment of the corpus's own");
   [XF] public void the_answer_shown_is_not_stored() => _json.Must().Satisfy(it => !it.TryGetProperty("activeAnswer", out _));
   [XF] public void the_question_shown_is_not_stored() => _json.Must().Satisfy(it => !it.TryGetProperty("activeQuestion", out _));

   [XF] public void it_is_read_back_showing_the_corpus_answer() => _readBack.GetAnswer().Must().Be("go to the river");
   [XF] public void it_is_read_back_with_the_source_answer() => _readBack.SourceData.Answer.Value.Must().Be("I go to the river");
   [XF] public void it_is_read_back_showing_the_source_question() => _readBack.GetQuestion().Must().Be("川に行く");
   [XF] public void it_is_read_back_with_its_external_id() => _readBack.SourceData.ExternalId.Value.Must().Be("anime_the_garden_of_words_000000346");
}
