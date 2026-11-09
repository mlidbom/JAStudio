using System;
using System.Linq;
using System.Text.Json;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using static Compze.Must.MustActions;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A vocab's parts of speech (<see cref="VocabNote.PartsOfSpeech"/>), written as the corpus's JSON and read back, as JAStudio saves and loads the corpus:<br/>
/// <c>partsOfSpeech</c> is a list of their <see cref="PartOfSpeech.Name"/>s, and a name JAStudio doesn't know can't be loaded.
/// </summary>
public class When_a_vocabs_parts_of_speech_are_saved_to_the_corpus : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly NoteSerializer _serializer;
   readonly string _json;

   public When_a_vocabs_parts_of_speech_are_saved_to_the_corpus()
   {
      var vocab = CreateVocab("走る", "to run", "はしる");
      vocab.PartsOfSpeech.Set([PartOfSpeech.Intransitive, PartOfSpeech.GodanVerb]);
      _serializer = GetService<NoteSerializer>();
      _json = _serializer.Serialize(vocab);
   }

   [XF] public void the_corpus_json_stores_their_names_in_alphabetical_order() =>
      JsonDocument.Parse(_json).RootElement.GetProperty("partsOfSpeech").EnumerateArray().Select(it => it.ToString()).Must().SequenceEqual(["godan verb", "intransitive"]);

   [XF] public void they_are_read_back_as_they_were() =>
      _serializer.DeserializeVocab(_json).PartsOfSpeech.All().Must().SequenceEqual([PartOfSpeech.GodanVerb, PartOfSpeech.Intransitive]);

   [XF] public void a_vocab_naming_a_part_of_speech_JAStudio_does_not_know_cannot_be_loaded() =>
      Invoking(() => _serializer.DeserializeVocab(_json.Replace("\"intransitive\"", "\"intransitiv\""))).Must().Throw<ArgumentException>();
}
