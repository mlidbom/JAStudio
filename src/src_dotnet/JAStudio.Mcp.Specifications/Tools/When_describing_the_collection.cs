using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Mcp.Tools;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools;

/// <summary>Describes a collection of two vocab, 行く with its reading studied and 来る, one kanji, 行, and one sentence, 明日学校に行く.</summary>
public class When_describing_the_collection : McpServerSpecification
{
   readonly CollectionDescription _description;

   public When_describing_the_collection()
   {
      CreateVocab("行く", "to go", "いく").SetStudyingStatus(new CardStudyingStatus(0, CardTypes.Reading, isSuspended: false, NoteTypes.Vocab));
      CreateVocab("来る", "to come", "くる");
      CreateKanji("行", "go", "コウ", "い");
      CreateTestSentence("明日学校に行く", "I'll go to school tomorrow");

      _description = ResultOf<CollectionDescription>(CallTool("describe_collection", new Dictionary<string, object?>()));
   }

   [XF] public void the_corpus_folder_is_the_folder_the_notes_are_loaded_from_and_saved_to() =>
      _description.CorpusFolder.Must().Be(GetService<IEnvironmentPaths>().DatabaseDir);

   [XF] public void it_counts_the_two_vocab_one_of_them_studied() => _description.Vocab.Must().Be(new NoteCounts(Notes: 2, Studying: 1));
   [XF] public void it_counts_the_one_kanji_not_studied() => _description.Kanji.Must().Be(new NoteCounts(Notes: 1, Studying: 0));
   [XF] public void it_counts_the_one_sentence_not_studied() => _description.Sentences.Must().Be(new NoteCounts(Notes: 1, Studying: 0));
}
