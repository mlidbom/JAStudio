using System;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.NotePages;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// The sentence パンを食べる。, with the vocab 食べる, opened in the workspace. A tab shows a sentence's page in Avalonia (<see cref="SentencePageViewModel"/>),<br/>
/// rather than its card's page in a web view as for the other notes, and follows the page's links in the tab.
/// </summary>
public class When_a_sentence_is_shown_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string Question = "パンを食べる。";
   const string Answer = "I eat bread.";

   readonly SentenceNote _sentence;
   readonly VocabNote _toEat;

   public When_a_sentence_is_shown_in_the_workspace()
   {
      _toEat = CreateVocab("食べる", "to eat", "たべる");
      _sentence = CreateTestSentence(Question, Answer);
      Workspace.OpenNote(_sentence);
   }

   NoteTabViewModel Tab => Workspace.SelectedTab ?? throw new InvalidOperationException("The workspace shows no tab.");

   SentencePageViewModel Page => Tab.NotePage as SentencePageViewModel ?? throw new InvalidOperationException("The tab shows no sentence's page.");

   /// <summary>The note the tab shows, by its question, and whether it shows the note's card page in the web view, rather than a page in Avalonia.</summary>
   (string Question, bool ShowsTheCardPage) TheTab() => (Tab.Header, Tab.ShowsTheCardPage);

   NoteLinkViewModel TheLinkToTheVocab() => Page.Entries.Select(it => it.WordLinkedToItsVocab).OfType<NoteLinkViewModel>().Single(it => it.LinkedNote.Equals(_toEat.GetId()));

   [XF] public void the_tab_shows_the_sentences_page_in_Avalonia() => TheTab().Must().Be((Question, false));
   [XF] public void the_page_shows_its_question_and_answer() => (Page.SourceQuestion, Page.CorpusAnswer).Must().Be((Question, Answer));
   [XF] public void the_breakdown_links_the_word_食べる_to_the_vocab() => TheLinkToTheVocab().Text.Must().Be("食べる");
   [XF] public void the_breakdown_gives_the_vocabs_answer() => Page.Entries.Select(it => it.Answer).Must().Contain("to eat");
   [XF] public void the_breakdown_shows_no_token_tables_outside_edit_mode() => Page.TokenTables.Must().BeEmpty();

   public class after_following_the_link_to_the_vocab : When_a_sentence_is_shown_in_the_workspace
   {
      public after_following_the_link_to_the_vocab() => Page.Follow(TheLinkToTheVocab());

      [XF] public void the_tab_shows_the_vocabs_card_page() => TheTab().Must().Be(("食べる", true));

      public class then_going_back : after_following_the_link_to_the_vocab
      {
         public then_going_back() => Tab.GoBackCommand.Execute(null);

         [XF] public void the_tab_shows_the_sentences_page_again() => TheTab().Must().Be((Question, false));
      }
   }

   public class after_the_breakdown_is_switched_to_edit_mode : When_a_sentence_is_shown_in_the_workspace
   {
      public after_the_breakdown_is_switched_to_edit_mode() => GetService<JapaneseConfig>().ShowSentenceBreakdownInEditMode.Value = true;

      [XF] public void the_page_is_made_anew_with_the_sentences_token_tables() => Page.TokenTables.Select(it => it.Title).Must().Contain("Tokens");
      [XF] public void the_view_settings_show_edit_mode_as_on() => Page.ViewToggles.Select(it => it.Abbreviation).Must().Contain("EM");
   }

   public class after_its_corpus_question_is_set : When_a_sentence_is_shown_in_the_workspace
   {
      const string CorpusQuestion = "パンを食べた。";

      public after_its_corpus_question_is_set() => _sentence.CorpusQuestion.Set(CorpusQuestion);

      [XF] public void the_page_is_made_anew_showing_it_over_the_sources_question() => (Page.CorpusQuestion, Page.HasACorpusQuestion).Must().Be((CorpusQuestion, true));
   }
}
