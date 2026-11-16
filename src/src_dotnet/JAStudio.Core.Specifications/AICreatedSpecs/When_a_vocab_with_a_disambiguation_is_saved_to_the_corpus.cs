using System.Text.Json;
using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A vocab whose question has a disambiguation, written as the corpus's JSON and read back, as JAStudio saves and loads the corpus.</summary>
public class When_a_vocab_with_a_disambiguation_is_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly JsonElement _questionInTheJson;
   readonly ITaggregateReference<VocabNote> _readBackReference;
   VocabNote _readBack => _readBackReference.Current;

   public When_a_vocab_with_a_disambiguation_is_saved_to_the_corpus()
   {
      var vocabReference = CreateVocab("入る", "to enter (after a masu stem)", "いる").Reference();
      vocabReference.Current.Edit(it => it.Question.SetDisambiguation("masu"));
      var serializer = GetService<NoteSerializer>();
      var json = serializer.Serialize(vocabReference.Current);
      _questionInTheJson = JsonDocument.Parse(json).RootElement.GetProperty("question");
      _readBackReference = serializer.DeserializeVocab(json).Reference();
   }

   [XF] public void it_is_read_back_with_its_question() => _readBack.Question.Text.Must().Be("入る");
   [XF] public void it_is_read_back_with_its_disambiguation() => _readBack.Question.Disambiguation.Must().Be("masu");
   [XF] public void the_corpus_json_stores_the_question_text() => _questionInTheJson.GetProperty("text").ToString().Must().Be("入る");
   [XF] public void the_corpus_json_stores_the_disambiguation() => _questionInTheJson.GetProperty("disambiguation").ToString().Must().Be("masu");
}
