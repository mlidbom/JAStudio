using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.Editors.Parsing;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The sentence 不浄のもの。, with the vocab 不浄の, opened in the workspace: its editor section's Parsing tab (<see cref="SentenceParsingViewModel"/>)<br/>
/// shows how it parsed, each shown match with the matches it covers that aren't shown indented under it, such as 不浄 and の, which 不浄の shadows.
/// </summary>
public class When_a_sentences_parsing_is_shown_in_its_editor : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   readonly SentenceNote _sentence;

   public When_a_sentences_parsing_is_shown_in_its_editor()
   {
      CreateVocab("不浄の", "unclean", "ふじょうの");
      _sentence = CreateTestSentence("不浄のもの。", "An unclean thing.");
      Workspace.OpenNote(_sentence);
   }

   SentenceParsingViewModel Parsing =>
      ((Workspace.SelectedTab ?? throw new InvalidOperationException("The workspace shows no tab.")).EditorSection as SentenceEditorSectionViewModel
       ?? throw new InvalidOperationException("The tab's editor section is no sentence's.")).Parsing;

   /// <summary>The rows of the matches of 不浄の and of what it covers: each row's word, whether it's shown, and how far it's indented.</summary>
   List<(string Word, bool IsShown, double Indentation)> TheRowsOf不浄の() =>
      [.. Parsing.Matches.SkipWhile(it => it.Word != "不浄の").TakeWhile(it => it.Word == "不浄の" || !it.IsShown).Select(it => (it.Word, it.IsShown, it.Indentation))];

   [XF] public void the_vocab_不浄の_is_shown_with_不浄_and_の_which_it_shadows_greyed_and_indented_under_it() =>
      TheRowsOf不浄の().Must().Satisfy(it => it[0] == ("不浄の", true, 0)
                                          && it.Skip(1).Any(row => row == ("不浄", false, 30))
                                          && it.Skip(1).Any(row => row == ("の", false, 30)));

   public class after_marking_不浄の_incorrect_through_its_rows_menu : When_a_sentences_parsing_is_shown_in_its_editor
   {
      public after_marking_不浄の_incorrect_through_its_rows_menu() =>
         Parsing.MenuOf(Parsing.Matches.First(it => it.Word == "不浄の"), "").Single(it => it.Name == "_e Mark incorrect here").Action();

      [XF] public void the_tab_shows_不浄の_as_not_shown() => Parsing.Matches.First(it => it.Word == "不浄の").IsShown.Must().BeFalse();
   }
}
