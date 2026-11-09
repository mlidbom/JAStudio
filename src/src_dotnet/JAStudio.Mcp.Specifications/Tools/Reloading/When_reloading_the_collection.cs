using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Tools.Reloading;

// ReSharper disable InconsistentNaming

namespace JAStudio.Mcp.Specifications.Tools.Reloading;

/// <summary>
/// The collection holds the vocab 行く and 来る. Behind its back, as when the corpus checkout changes on disk, 行く's stored data is replaced by a<br/>
/// copy whose answer is "to walk".
/// </summary>
public class When_reloading_the_collection : McpServerSpecification
{
   readonly ReloadResult _result;

   public When_reloading_the_collection()
   {
      var vocab行く = CreateVocab("行く", "to go", "いく");
      CreateVocab("来る", "to come", "くる");
      GetService<INoteRepository>().Save(ACopyWithTheAnswer(vocab行く, "to walk"));

      _result = ResultOf<ReloadResult>(CallTool("reload_the_collection", new Dictionary<string, object?>()));
   }

   VocabNote ACopyWithTheAnswer(VocabNote vocab, string answer)
   {
      var serializer = GetService<NoteSerializer>();
      var copy = serializer.DeserializeVocab(serializer.Serialize(vocab));
      copy.CorpusAnswer.Set(answer);
      return copy;
   }

   [XF] public void the_vocab_changed_on_disk_is_the_one_note_listed() => _result.NotesChangedOnDisk.Vocab.Notes.Select(it => it.Question).Must().SequenceEqual(["行く"]);
   [XF] public void it_is_listed_with_its_answer_as_changed_on_disk() => _result.NotesChangedOnDisk.Vocab.Notes.Select(it => it.Answer).Must().SequenceEqual(["to walk"]);
}
