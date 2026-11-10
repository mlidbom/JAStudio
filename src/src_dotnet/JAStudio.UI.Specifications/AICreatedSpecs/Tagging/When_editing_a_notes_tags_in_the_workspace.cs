using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.ViewModels.Workspace;
using JAStudio.UI.ViewModels.Workspace.Editors;
using JAStudio.UI.ViewModels.Workspace.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// A note's Tags editor in the workspace's editor section shows every tag the note has, each a person manages with a button that removes it,<br/>
/// asking first for a source tag, and adds a tag picked in a dialog. Ctrl+T and Ctrl+Shift+T do the same for the note on the shown tab.<br/>
/// The sentence is tagged <c>source::anime::natsume</c>, <c>-::counting</c> and the tag JAStudio maintains for TTS audio, <c>_tts_audio</c>.
/// </summary>
public class When_editing_a_notes_tags_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string SourceTag = "source::anime::natsume";
   const string Label = "-::counting";
   const string NewTag = "-::todo::check-the-reading";

   readonly SentenceNote _sentence;
   readonly NoteTagsEditorViewModel _tagsEditor;

   public When_editing_a_notes_tags_in_the_workspace()
   {
      _sentence = CreateTestSentence("家に帰る", "Go home");
      _sentence.Tags.Set(Tag.FromName(SourceTag));
      _sentence.Tags.Set(Tag.FromName(Label));
      _sentence.Tags.Set(Tags.TTSAudio);
      _tagsEditor = Edit<SentenceEditorSectionViewModel>(_sentence).Tags;
   }

   List<string> TagsShownInTheEditor() => [.._tagsEditor.Tags.Select(it => it.Name)];

   bool TheSentenceHas(string tagPath) => _sentence.Tags.Contains(Tag.FromName(tagPath));

   /// <summary>Picks <paramref name="tagPath"/> in the tag picker dialog, as a person would.</summary>
   static Func<TagPickerDialogViewModel, Task<string?>> Picking(string tagPath) => _ => Task.FromResult<string?>(tagPath);

   NoteTabViewModel ShownTab() => Workspace.SelectedTab ?? throw new InvalidOperationException("No tab is shown.");

   TagOnTheNoteViewModel TheTagShown(string tagPath) => _tagsEditor.Tags.Single(it => it.Name == tagPath);

   [XF] public void the_editor_shows_every_tag_the_one_JAStudio_maintains_too_in_alphabetical_order() =>
      TagsShownInTheEditor().Must().SequenceEqual([Label, SourceTag, Tags.TTSAudio.Name]);

   [XF] public void the_tag_JAStudio_maintains_cannot_be_removed_by_hand() => TheTagShown(Tags.TTSAudio.Name).CanBeRemovedByHand.Must().BeFalse();
   [XF] public void the_tags_a_person_manages_can() => (TheTagShown(SourceTag).CanBeRemovedByHand && TheTagShown(Label).CanBeRemovedByHand).Must().BeTrue();

   public class when_removing_a_tag_with_its_button : When_editing_a_notes_tags_in_the_workspace
   {
      protected readonly List<string> _questionsAsked = [];

      protected void RemoveWithItsButton(string tagPath, bool answerToTheQuestion) =>
         _tagsEditor.Remove(TheTagShown(tagPath), question =>
         {
            _questionsAsked.Add(question);
            return Task.FromResult(answerToTheQuestion);
         });

      public class that_is_not_a_source_tag : when_removing_a_tag_with_its_button
      {
         public that_is_not_a_source_tag() => RemoveWithItsButton(Label, answerToTheQuestion: false);

         [XF] public void it_asks_nothing() => _questionsAsked.Must().BeEmpty();
         [XF] public void the_sentence_no_longer_has_it() => TheSentenceHas(Label).Must().BeFalse();
         [XF] public void the_editor_no_longer_shows_it() => TagsShownInTheEditor().Must().SequenceEqual([SourceTag, Tags.TTSAudio.Name]);
      }

      public class that_is_a_source_tag_confirming_it : when_removing_a_tag_with_its_button
      {
         public that_is_a_source_tag_confirming_it() => RemoveWithItsButton(SourceTag, answerToTheQuestion: true);

         [XF] public void it_asks_first_saying_it_changes_the_notes_source() =>
            _questionsAsked.Must().SequenceEqual([$"{SourceTag} is a source tag: removing it changes the note's source. Remove it?"]);
         [XF] public void the_sentence_no_longer_has_it() => TheSentenceHas(SourceTag).Must().BeFalse();
         [XF] public void the_tag_JAStudio_maintains_stays() => _sentence.Tags.Contains(Tags.TTSAudio).Must().BeTrue();
      }

      public class that_is_a_source_tag_declining_it : when_removing_a_tag_with_its_button
      {
         public that_is_a_source_tag_declining_it() => RemoveWithItsButton(SourceTag, answerToTheQuestion: false);

         [XF] public void the_sentence_still_has_it() => TheSentenceHas(SourceTag).Must().BeTrue();
      }
   }

   public class after_adding_a_new_tag_through_the_editor : When_editing_a_notes_tags_in_the_workspace
   {
      public after_adding_a_new_tag_through_the_editor() => _tagsEditor.AddATag(Picking(NewTag));

      [XF] public void the_sentence_has_it() => TheSentenceHas(NewTag).Must().BeTrue();
      [XF] public void the_editor_shows_it_in_alphabetical_order() => TagsShownInTheEditor().Must().SequenceEqual([Label, NewTag, SourceTag, Tags.TTSAudio.Name]);
   }

   public class after_adding_a_tag_to_the_note_on_the_shown_tab_as_ctrl_t_does : When_editing_a_notes_tags_in_the_workspace
   {
      public after_adding_a_tag_to_the_note_on_the_shown_tab_as_ctrl_t_does() => ShownTab().AddATagToTheShownNote(Picking(NewTag));

      [XF] public void the_sentence_has_it() => TheSentenceHas(NewTag).Must().BeTrue();
      [XF] public void the_editor_shows_it() => TagsShownInTheEditor().Must().Contain(NewTag);
   }

   public class after_removing_a_tag_from_the_note_on_the_shown_tab_as_ctrl_shift_t_does : When_editing_a_notes_tags_in_the_workspace
   {
      public after_removing_a_tag_from_the_note_on_the_shown_tab_as_ctrl_shift_t_does() => ShownTab().RemoveATagFromTheShownNote(Picking(SourceTag));

      [XF] public void the_sentence_no_longer_has_it() => TheSentenceHas(SourceTag).Must().BeFalse();
   }
}
