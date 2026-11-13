using System;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.UI.Web;
using JAStudio.UI.ViewModels.Workspace;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs;

/// <summary>
/// Three sentences, the first open in a workspace tab. Following a link on the tab's page shows the linked note in the same tab, and back and forward<br/>
/// go through the notes the tab showed, as a browser tab's do. The tab keeps that history itself (<see cref="NoteTabHistory"/>), not its web view.
/// </summary>
public class When_following_links_in_a_workspace_tab : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string FirstSentence = "家に帰る";
   const string SecondSentence = "学校に行く";
   const string ThirdSentence = "駅で待つ";

   readonly SentenceNote _first;
   readonly SentenceNote _second;
   readonly SentenceNote _third;

   public When_following_links_in_a_workspace_tab()
   {
      _first = CreateTestSentence(FirstSentence, "an answer");
      _second = CreateTestSentence(SecondSentence, "an answer");
      _third = CreateTestSentence(ThirdSentence, "an answer");
      Workspace.OpenNote(_first);
   }

   NoteTabViewModel Tab => Workspace.SelectedTab ?? throw new InvalidOperationException("The workspace shows no tab.");

   /// <summary>The note the tab shows, by its question, and whether its back and forward buttons are enabled.</summary>
   (string Question, bool CanGoBack, bool CanGoForward) TheTab() => (Tab.Header, Tab.GoBackCommand.CanExecute(null), Tab.GoForwardCommand.CanExecute(null));

   /// <summary>Follows a link to <paramref name="note"/> on the tab's page, as clicking it does: the page reports it, and the tab shows the note.</summary>
   void FollowALinkTo(SentenceNote note) => Tab.ShowNote(note.GetId());

   void Delete(SentenceNote note) => GetService<JPCollection>().Delete(note, ItsAnkiNote.IsDeleted);

   [XF] public void the_tab_shows_the_first_sentence_and_can_go_neither_back_nor_forward() => TheTab().Must().Be((FirstSentence, false, false));

   public class after_going_back_with_Alt_Left : When_following_links_in_a_workspace_tab
   {
      public after_going_back_with_Alt_Left() => Workspace.GoBackInTheShownTab();

      [XF] public void the_tab_still_shows_the_first_sentence() => TheTab().Must().Be((FirstSentence, false, false));
   }

   public class after_following_a_link_to_the_second_sentence : When_following_links_in_a_workspace_tab
   {
      public after_following_a_link_to_the_second_sentence() => FollowALinkTo(_second);

      [XF] public void the_tab_shows_the_second_sentence_and_can_go_back() => TheTab().Must().Be((SecondSentence, true, false));

      public class then_following_a_link_to_the_second_sentence_again_and_going_back : after_following_a_link_to_the_second_sentence
      {
         public then_following_a_link_to_the_second_sentence_again_and_going_back()
         {
            FollowALinkTo(_second);
            Tab.GoBackCommand.Execute(null);
         }

         [XF] public void the_tab_shows_the_first_sentence() => TheTab().Must().Be((FirstSentence, false, true));
      }

      public class and_going_back : after_following_a_link_to_the_second_sentence
      {
         public and_going_back() => Tab.GoBackCommand.Execute(null);

         [XF] public void the_tab_shows_the_first_sentence_and_can_go_forward() => TheTab().Must().Be((FirstSentence, false, true));

         public class then_going_forward_with_Alt_Right : and_going_back
         {
            public then_going_forward_with_Alt_Right() => Workspace.GoForwardInTheShownTab();

            [XF] public void the_tab_shows_the_second_sentence_again() => TheTab().Must().Be((SecondSentence, true, false));
         }

         public class then_following_a_link_to_the_third_sentence : and_going_back
         {
            public then_following_a_link_to_the_third_sentence() => FollowALinkTo(_third);

            [XF] public void the_tab_shows_the_third_sentence_and_the_second_is_no_longer_ahead() => TheTab().Must().Be((ThirdSentence, true, false));

            public class then_going_back : then_following_a_link_to_the_third_sentence
            {
               public then_going_back() => Tab.GoBackCommand.Execute(null);

               [XF] public void the_tab_shows_the_first_sentence() => TheTab().Must().Be((FirstSentence, false, true));
            }
         }
      }

      public class and_the_page_then_shows_the_first_sentences_page_by_itself : after_following_a_link_to_the_second_sentence
      {
         public and_the_page_then_shows_the_first_sentences_page_by_itself() => Tab.PageShown(CardServerUrl.NotePageUrl(_first));

         [XF] public void the_tab_shows_the_first_sentence_after_the_second() => TheTab().Must().Be((FirstSentence, true, false));
      }

      public class and_the_first_sentence_is_deleted : after_following_a_link_to_the_second_sentence
      {
         public and_the_first_sentence_is_deleted() => Delete(_first);

         [XF] public void the_tab_shows_the_second_sentence_and_can_no_longer_go_back() => TheTab().Must().Be((SecondSentence, false, false));
      }

      public class and_the_second_sentence_is_deleted : after_following_a_link_to_the_second_sentence
      {
         public and_the_second_sentence_is_deleted() => Delete(_second);

         [XF] public void the_tab_closes() => Workspace.Tabs.Must().BeEmpty();
      }

      public class then_following_a_link_back_to_the_first_sentence_and_deleting_the_second : after_following_a_link_to_the_second_sentence
      {
         public then_following_a_link_back_to_the_first_sentence_and_deleting_the_second()
         {
            FollowALinkTo(_first);
            Delete(_second);
         }

         [XF] public void the_tab_shows_the_first_sentence_and_cannot_go_back_to_it_again() => TheTab().Must().Be((FirstSentence, false, false));
      }
   }
}
