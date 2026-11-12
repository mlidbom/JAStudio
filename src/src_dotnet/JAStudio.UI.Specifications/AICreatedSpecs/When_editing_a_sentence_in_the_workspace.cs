using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Editors;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

public class When_editing_a_sentence_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly VocabNote _vocab;
   readonly SentenceNote _sentence;
   readonly SentenceEditorSectionViewModel _editor;

   protected When_editing_a_sentence_in_the_workspace()
   {
      _vocab = CreateVocab("走る", "to run", "はしる");
      _sentence = CreateSentence("毎日歩く。");
      _editor = Edit<SentenceEditorSectionViewModel>(_sentence);
   }

   TextFieldEditorViewModel CorpusQuestion => _editor.CorpusFields.Single(it => it.Label == "Question");

   public class before_its_question_is_changed : When_editing_a_sentence_in_the_workspace
   {
      [XF] public void the_sentence_does_not_match_the_vocab() => _vocab.Sentences.All().Select(it => it.GetId()).Must().Satisfy(it => !it.Contains(_sentence.GetId()));
   }

   public class after_typing_a_question_containing_the_vocab : When_editing_a_sentence_in_the_workspace
   {
      public after_typing_a_question_containing_the_vocab() => CorpusQuestion.Text = "毎日走る。";

      [XF] public void the_question_is_written_to_the_sentence() => _sentence.GetQuestion().Must().Be("毎日走る。");
      [XF] public void the_sentence_is_reparsed_and_matches_the_vocab() => _vocab.Sentences.All().Must().Contain(_sentence);
   }
}
