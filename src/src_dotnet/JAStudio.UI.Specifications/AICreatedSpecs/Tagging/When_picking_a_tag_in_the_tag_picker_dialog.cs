using System;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.ViewModels.Workspace.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// The tag picker dialog, adding a tag to some notes or removing one from them, warns when the tag picked is a source tag, since that changes<br/>
/// the notes' source. The notes: a sentence tagged <c>source::misc</c>, and a sentence with no tags.
/// </summary>
public class When_picking_a_tag_in_the_tag_picker_dialog : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   protected readonly SentenceNote _sentenceFromMisc;
   protected readonly SentenceNote _sentenceWithNoTags;

   public When_picking_a_tag_in_the_tag_picker_dialog()
   {
      _sentenceFromMisc = CreateTestSentence("家に帰る", "Go home");
      _sentenceFromMisc.Tags.Set(Tag.FromName("source::misc"));
      _sentenceWithNoTags = CreateTestSentence("学校に行く", "Go to school");
   }

   private protected static string TheSourceWarningOf(TagPickerDialogViewModel dialog) =>
      dialog.SourceWarning ?? throw new InvalidOperationException("The dialog showed no source warning.");

   private protected static TagPickerDialogViewModel Typing(string typedParts, TagPickerDialogViewModel dialog)
   {
      dialog.Picker.TypedParts = typedParts;
      return dialog;
   }

   public class to_add_to_both_notes : When_picking_a_tag_in_the_tag_picker_dialog
   {
      private protected TagPickerDialogViewModel Adding(string typedParts) =>
         Typing(typedParts, TagPickerDialogViewModel.ForAddingATagTo([_sentenceFromMisc, _sentenceWithNoTags], GetService<JPCollection>()));

      [XF] public void a_new_source_tag_warns_that_it_changes_the_source_of_both() =>
         TheSourceWarningOf(Adding("source::tatoeba")).Must().Be("source::tatoeba is a source tag: adding it changes the source of 2 notes.");
      [XF] public void a_source_tag_one_of_them_has_warns_that_it_changes_the_source_of_the_other() =>
         TheSourceWarningOf(Adding("source::misc")).Must().Be("source::misc is a source tag: adding it changes the source of 1 note.");
      [XF] public void a_tag_outside_the_source_branch_warns_of_nothing() => Adding("-::counting").SourceWarning.Must().BeNull();
   }

   public class to_remove_from_both_notes : When_picking_a_tag_in_the_tag_picker_dialog
   {
      [XF] public void the_source_tag_one_of_them_has_warns_that_it_changes_the_source_of_that_one() =>
         TheSourceWarningOf(Typing("source::misc", TagPickerDialogViewModel.ForRemovingATagFrom([_sentenceFromMisc, _sentenceWithNoTags])))
            .Must().Be("source::misc is a source tag: removing it changes the source of 1 note.");
   }
}
