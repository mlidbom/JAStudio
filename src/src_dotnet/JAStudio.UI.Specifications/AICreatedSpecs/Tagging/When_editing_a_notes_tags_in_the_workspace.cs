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
/// A note's Tags editor in the workspace's editor section shows the tags a person manages, each with a button that removes it,<br/>
/// and adds a tag picked in a dialog. Ctrl+T and Ctrl+Shift+T do the same for the note on the shown tab.
/// </summary>
public class When_editing_a_notes_tags_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string SourceTag = "source::anime::natsume";
   const string NewTag = "-::todo::check-the-reading";

   readonly SentenceNote _sentence;
   readonly NoteTagsEditorViewModel _tagsEditor;

   public When_editing_a_notes_tags_in_the_workspace()
   {
      _sentence = CreateTestSentence("家に帰る", "Go home");
      _sentence.Tags.Set(Tag.FromName(SourceTag));
      _sentence.Tags.Set(Tags.TTSAudio);
      _tagsEditor = Edit<SentenceEditorSectionViewModel>(_sentence).Tags;
   }

   List<string> TagsShownInTheEditor() => [.._tagsEditor.Tags.Select(it => it.Name)];

   bool TheSentenceHas(string tagPath) => _sentence.Tags.Contains(Tag.FromName(tagPath));

   /// <summary>Picks <paramref name="tagPath"/> in the tag picker dialog, as a person would.</summary>
   static Func<TagPickerDialogViewModel, Task<string?>> Picking(string tagPath) => _ => Task.FromResult<string?>(tagPath);

   NoteTabViewModel ShownTab() => Workspace.SelectedTab ?? throw new InvalidOperationException("No tab is shown.");

   [XF] public void the_editor_shows_the_tags_a_person_manages_but_not_the_jastudio_defined_ones() => TagsShownInTheEditor().Must().SequenceEqual([SourceTag]);

   public class after_removing_a_tag_with_its_button : When_editing_a_notes_tags_in_the_workspace
   {
      public after_removing_a_tag_with_its_button() => _tagsEditor.Tags.Single().RemoveCommand.Execute(null);

      [XF] public void the_sentence_no_longer_has_it() => TheSentenceHas(SourceTag).Must().BeFalse();
      [XF] public void the_editor_no_longer_shows_it() => TagsShownInTheEditor().Must().BeEmpty();
      [XF] public void the_jastudio_defined_tag_stays() => _sentence.Tags.Contains(Tags.TTSAudio).Must().BeTrue();
   }

   public class after_adding_a_new_tag_through_the_editor : When_editing_a_notes_tags_in_the_workspace
   {
      public after_adding_a_new_tag_through_the_editor() => _tagsEditor.AddATag(Picking(NewTag));

      [XF] public void the_sentence_has_it() => TheSentenceHas(NewTag).Must().BeTrue();
      [XF] public void the_editor_shows_it_in_alphabetical_order() => TagsShownInTheEditor().Must().SequenceEqual([NewTag, SourceTag]);
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
