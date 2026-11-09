using System;
using System.Linq;
using System.Threading;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.Controls.NoteFields;
using JAStudio.UI.ViewModels.Workspace.Editors;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// Editing a vocab's parts of speech in the workspace's vocab editor (<see cref="PartsOfSpeechEditorViewModel"/>): only a part of speech JAStudio knows,<br/>
/// and the vocab doesn't have yet, can be added.
/// </summary>
public class When_editing_a_vocabs_parts_of_speech_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly VocabNote _vocab;
   readonly VocabEditorSectionViewModel _editor;

   public When_editing_a_vocabs_parts_of_speech_in_the_workspace()
   {
      _vocab = CreateVocab("走る", "to run", "はしる");
      _vocab.PartsOfSpeech.Set([PartOfSpeech.GodanVerb, PartOfSpeech.Intransitive]);
      _editor = Edit<VocabEditorSectionViewModel>(_vocab);
   }

   PartsOfSpeechEditorViewModel PartsOfSpeech => _editor.Text.PartsOfSpeech;

   void WaitForTheReparseToFinish() => SpinWait.SpinUntil(() => !_editor.SentencesReparser.IsReparsingSentences, TimeSpan.FromMinutes(1)).Must().BeTrue();

   [XF] public void the_box_offers_every_part_of_speech_but_the_vocabs() =>
      PartsOfSpeech.Addable.Must().SequenceEqual(PartOfSpeech.All.Where(it => it != PartOfSpeech.GodanVerb && it != PartOfSpeech.Intransitive).Select(it => it.Name));

   public class after_typing_text_that_names_no_part_of_speech : When_editing_a_vocabs_parts_of_speech_in_the_workspace
   {
      public after_typing_text_that_names_no_part_of_speech() => PartsOfSpeech.NewPartOfSpeech = "godan";

      [XF] public void it_cannot_be_added() => PartsOfSpeech.AddCommand.CanExecute(null).Must().BeFalse();
   }

   public class after_typing_a_part_of_speech_the_vocab_has : When_editing_a_vocabs_parts_of_speech_in_the_workspace
   {
      public after_typing_a_part_of_speech_the_vocab_has() => PartsOfSpeech.NewPartOfSpeech = "godan verb";

      [XF] public void it_cannot_be_added() => PartsOfSpeech.AddCommand.CanExecute(null).Must().BeFalse();
   }

   public class after_adding_transitive : When_editing_a_vocabs_parts_of_speech_in_the_workspace
   {
      public after_adding_transitive()
      {
         PartsOfSpeech.NewPartOfSpeech = "transitive";
         PartsOfSpeech.AddCommand.Execute(null);
         WaitForTheReparseToFinish();
      }

      [XF] public void the_vocab_has_it() => _vocab.PartsOfSpeech.All().Must().SequenceEqual([PartOfSpeech.GodanVerb, PartOfSpeech.Intransitive, PartOfSpeech.Transitive]);
      [XF] public void the_editor_shows_it_as_a_chip() => PartsOfSpeech.Items.Select(it => it.Name).Must().SequenceEqual(["godan verb", "intransitive", "transitive"]);
      [XF] public void the_box_no_longer_offers_it() => PartsOfSpeech.Addable.Contains("transitive").Must().BeFalse();
      [XF] public void the_box_is_emptied() => PartsOfSpeech.NewPartOfSpeech.Must().Be("");
   }

   public class after_removing_intransitive : When_editing_a_vocabs_parts_of_speech_in_the_workspace
   {
      public after_removing_intransitive()
      {
         PartsOfSpeech.Items.Single(it => it.Name == "intransitive").RemoveCommand.Execute(null);
         WaitForTheReparseToFinish();
      }

      [XF] public void the_vocab_has_only_the_other() => _vocab.PartsOfSpeech.All().Must().SequenceEqual([PartOfSpeech.GodanVerb]);
      [XF] public void the_box_offers_it_again() => PartsOfSpeech.Addable.Contains("intransitive").Must().BeTrue();
   }
}
