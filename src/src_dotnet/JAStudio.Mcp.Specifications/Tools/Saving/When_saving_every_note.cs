using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Mcp.Tools.Saving;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Saving;

/// <summary>A corpus checkout with the vocab 行く, the kanji 行 and a sentence, each file as JAStudio stores the note and committed, every note then saved.</summary>
public class When_saving_every_note : CorpusRepositorySpecification
{
   readonly EveryNoteSaved _result;

   public When_saving_every_note()
   {
      WriteTheFilesOf(CreateVocab("行く", "to go", "いく"), CreateKanji("行", "go", "コウ", "い"), CreateTestSentence("学校に行く", "go to school"));
      CommitEverything();

      _result = ResultOf<EveryNoteSaved>(CallTool("save_every_note", new Dictionary<string, object?>()));
   }

   [XF] public void it_writes_every_note() => _result.NotesWritten.Must().Be(3);
   [XF] public void no_note_file_differs_from_HEAD_since_each_was_as_JAStudio_stores_its_note() => (_result.ChangedNotes.Vocab.Count, _result.ChangedNotes.Kanji.Count, _result.ChangedNotes.Sentences.Count).Must().Be((0, 0, 0));
   [XF] public void it_reports_how_long_it_took() => _result.SecondsTaken.Must().BeGreaterThanOrEqualTo(0);
}
