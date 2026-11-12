using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// Renaming and deleting branches of the tag tree in the navigation pane's Tags tab, through dialogs answered as a person would.<br/>
/// Two sentences are tagged <c>source::anime::natsume::s3::01</c> and <c>…::02</c>, a vocab <c>source::wani</c>,<br/>
/// and a kanji a tag JAStudio defines, below <c>-</c>. The <c>source</c> line is expanded.
/// </summary>
public class When_editing_the_tag_tree_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   const string Natsume = "source::anime::natsume";
   static readonly TimeSpan TimeToWaitForTheChange = TimeSpan.FromSeconds(10);

   readonly TagManagementViewModel _tags;
   readonly JPNote _sentenceAboutHome;
   readonly JPNote _sentenceAboutSchool;
   readonly JPNote _vocab;

   /// <summary>The rename dialog the tag management showed last.</summary>
   TagRenameDialogViewModel? _renameDialogShown;

   /// <summary>The name in the rename dialog when it was shown.</summary>
   string? _nameTheRenameDialogOpenedWith;

   /// <summary>The question the delete confirmation asked last.</summary>
   string? _deleteQuestionAsked;

   public When_editing_the_tag_tree_in_the_workspace()
   {
      _sentenceAboutHome = Tagged(CreateTestSentence("家に帰る", "Go home"), $"{Natsume}::s3::01");
      _sentenceAboutSchool = Tagged(CreateTestSentence("学校に行く", "Go to school"), $"{Natsume}::s3::02");
      _vocab = Tagged(CreateVocab("行く", "to go", "いく"), "source::wani");
      Tagged(CreateKanji("行", "go", "コウ", "い"), Tags.Kanji.InAnyVocabForm.Name);

      _tags = Workspace.Navigation.TagManagement;
      Workspace.Navigation.IsTagManagementShown = true;
      LineOf("source").IsExpanded = true;
   }

   static JPNote Tagged(JPNote note, string tagName)
   {
      note.Tags.Set(Tag.FromName(tagName));
      return note;
   }

   static List<string> TagsOf(JPNote note) => [..note.Tags.Select(it => it.Name).Order(StringComparer.Ordinal)];

   TagTreeNodeViewModel LineOf(string tagPath) => AllLines().SingleOrDefault(it => it.TagPath == tagPath) ?? throw new InvalidOperationException($"No line shows {tagPath}.");

   IEnumerable<TagTreeNodeViewModel> AllLines() => _tags.TopLevelLines.SelectMany(it => it.ThisAndAllBelow());

   List<string> PathsOfTheVisibleLines() => [.._tags.TopLevelLines.SelectMany(VisibleFrom).Select(it => it.TagPath)];

   static IEnumerable<TagTreeNodeViewModel> VisibleFrom(TagTreeNodeViewModel line) => [line, ..line.IsExpanded ? line.Children.SelectMany(VisibleFrom) : []];

   string? SelectedPath() => _tags.SelectedLine?.TagPath;

   string PathOfTheSelectedLine() => SelectedPath() ?? throw new InvalidOperationException("No line is selected.");

   TagRenameDialogViewModel RenameDialogShown => _renameDialogShown ?? throw new InvalidOperationException("No rename dialog was shown.");

   string NameTheRenameDialogOpenedWith => _nameTheRenameDialogOpenedWith ?? throw new InvalidOperationException("No rename dialog was shown.");

   string DeleteQuestionAsked => _deleteQuestionAsked ?? throw new InvalidOperationException("No delete confirmation was shown.");

   /// <summary>Answers the rename dialog as a person typing <paramref name="newName"/> and clicking its accept button would, when it can be clicked.</summary>
   Func<TagRenameDialogViewModel, Task<TagBranchRename?>> Typing(string newName) => dialog =>
   {
      _renameDialogShown = dialog;
      _nameTheRenameDialogOpenedWith = dialog.TypedName;
      dialog.TypedName = newName;
      return Task.FromResult(dialog.CanAccept ? dialog.Rename : null);
   };

   Func<string, Task<bool>> Answering(bool yes) => question =>
   {
      _deleteQuestionAsked = question;
      return Task.FromResult(yes);
   };

   /// <summary>The change runs in the background, and the tree is shown again with <paramref name="tagPath"/> selected when it's done.</summary>
   void WaitUntilTheTreeShowsSelected(string tagPath) => SpinWait.SpinUntil(() => SelectedPath() == tagPath, TimeToWaitForTheChange).Must().BeTrue();

   public class after_renaming_natsume_to_soseki : When_editing_the_tag_tree_in_the_workspace
   {
      public after_renaming_natsume_to_soseki()
      {
         _tags.SelectedLine = LineOf(Natsume);
         _tags.RenameTheSelectedTag(Typing("soseki"));
         WaitUntilTheTreeShowsSelected("source::anime::soseki");
      }

      [XF] public void the_dialog_opened_with_the_last_level() => NameTheRenameDialogOpenedWith.Must().Be("natsume");
      [XF] public void the_dialog_showed_the_path_above_as_staying() => RenameDialogShown.FixedPathAbove.Must().Be("source::anime::");
      [XF] public void the_dialog_said_how_many_notes_change() => RenameDialogShown.WhatChanges.Must().Be($"{Natsume} and every tag below it change on 2 notes.");
      [XF] public void the_dialog_warned_that_it_changes_the_source_of_2_notes() => (RenameDialogShown.SourceWarning ?? throw new InvalidOperationException("The dialog showed no source warning.")).Must().Be("This changes the source of 2 notes.");
      [XF] public void the_notes_have_the_renamed_tags() => TagsOf(_sentenceAboutHome).Must().SequenceEqual(["source::anime::soseki::s3::01"]);
      [XF] public void the_renamed_line_is_visible() => PathsOfTheVisibleLines().Must().Contain("source::anime::soseki");
      [XF] public void no_line_shows_the_old_name() => AllLines().Must().Satisfy(lines => !lines.Any(it => it.TagPath.StartsWith(Natsume, StringComparison.Ordinal)));
   }

   public class after_renaming_episode_02_to_01_which_is_in_use : When_editing_the_tag_tree_in_the_workspace
   {
      public after_renaming_episode_02_to_01_which_is_in_use()
      {
         LineOf("source::anime").IsExpanded = true;
         LineOf(Natsume).IsExpanded = true;
         LineOf($"{Natsume}::s3").IsExpanded = true;
         _tags.SelectedLine = LineOf($"{Natsume}::s3::02");
         _tags.RenameTheSelectedTag(Typing("01"));
         WaitUntilTheTreeShowsSelected($"{Natsume}::s3::01");
      }

      [XF] public void the_dialog_warned_that_it_merges() => RenameDialogShown.Merges.Must().BeTrue();
      [XF] public void the_dialog_named_the_tag_it_merges_with() => (RenameDialogShown.MergeWarning ?? throw new InvalidOperationException("The dialog showed no merge warning.")).Must().Contain($"merges with {Natsume}::s3::01, already in use");
      [XF] public void the_dialogs_accept_button_said_merge() => RenameDialogShown.AcceptText.Must().Be("Merge");
      [XF] public void both_sentences_have_the_tag() => TagsOf(_sentenceAboutSchool).Must().SequenceEqual(TagsOf(_sentenceAboutHome));
   }

   public class after_renaming_wani_with_its_parents_to_web_wani : When_editing_the_tag_tree_in_the_workspace
   {
      public after_renaming_wani_with_its_parents_to_web_wani()
      {
         _tags.SelectedLine = LineOf("source::wani");
         _tags.RenameTheSelectedTagWithItsParents(Typing("web::wani"));
         WaitUntilTheTreeShowsSelected("web::wani");
      }

      [XF] public void the_dialog_opened_with_the_whole_path() => NameTheRenameDialogOpenedWith.Must().Be("source::wani");
      [XF] public void the_dialog_showed_no_path_as_staying() => RenameDialogShown.FixedPathAbove.Must().Be("");
      [XF] public void the_vocab_has_the_moved_tag() => TagsOf(_vocab).Must().SequenceEqual(["web::wani"]);
      [XF] public void the_moved_line_is_visible_below_its_expanded_parent() => PathsOfTheVisibleLines().Must().Contain("web::wani");
   }

   public class after_deleting_natsume_and_confirming : When_editing_the_tag_tree_in_the_workspace
   {
      public after_deleting_natsume_and_confirming()
      {
         _tags.SelectedLine = LineOf(Natsume);
         _tags.DeleteTheSelectedTag(Answering(yes: true));
         WaitUntilTheTreeShowsSelected("source");
      }

      [XF] public void the_nearest_line_above_that_is_left_is_selected_since_natsume_was_all_anime_held() => PathOfTheSelectedLine().Must().Be("source");

      [XF] public void the_question_said_what_is_deleted_from_how_many_notes() =>
         DeleteQuestionAsked.Must().Be($"Delete {Natsume} and every tag below it from 2 notes? This changes the source of 2 notes. This can't be undone.");
      [XF] public void the_notes_no_longer_have_the_tags() => TagsOf(_sentenceAboutHome).Concat(TagsOf(_sentenceAboutSchool)).Must().BeEmpty();
      [XF] public void the_other_notes_keep_their_tags() => TagsOf(_vocab).Must().SequenceEqual(["source::wani"]);
   }

   public class after_deleting_natsume_and_declining : When_editing_the_tag_tree_in_the_workspace
   {
      public after_deleting_natsume_and_declining()
      {
         _tags.SelectedLine = LineOf(Natsume);
         _tags.DeleteTheSelectedTag(Answering(yes: false));
      }

      [XF] public void the_notes_keep_their_tags() => TagsOf(_sentenceAboutHome).Must().SequenceEqual([$"{Natsume}::s3::01"]);
   }

   List<SpecMenuItem> TheMenuOfTheLineOf(string tagPath)
   {
      _tags.SelectedLine = LineOf(tagPath);
      return _tags.SelectedLineMenu(rename: () => {}, renameWithParents: () => {}, delete: () => {});
   }

   static List<bool> WhetherRenameRenameWithParentsAndDeleteAreEnabled(List<SpecMenuItem> menu) =>
      [..menu.Where(it => it.Name.Contains("Rename") || it.Name.Contains("Delete")).Select(it => it.IsEnabled)];

   public class the_menu_of_the_line_of_a_branch_holding_a_tag_jastudio_maintains : When_editing_the_tag_tree_in_the_workspace
   {
      readonly List<SpecMenuItem> _menu;

      public the_menu_of_the_line_of_a_branch_holding_a_tag_jastudio_maintains() => _menu = TheMenuOfTheLineOf("-");

      [XF] public void cannot_rename_or_delete() => WhetherRenameRenameWithParentsAndDeleteAreEnabled(_menu).Must().SequenceEqual([false, false, false]);
      [XF] public void says_why_for_each() =>
         _menu.TakeLast(2).Select(it => it.Name).Must().SequenceEqual([$"- holds {Tags.Kanji.InAnyVocabForm.Name}, whose name JAStudio uses, so it can't be renamed by hand.",
                                                                       $"- holds {Tags.Kanji.InAnyVocabForm.Name}, which JAStudio maintains, so it can't be deleted by hand."]);
   }

   public class the_menu_of_the_line_of_the_source_branch : When_editing_the_tag_tree_in_the_workspace
   {
      readonly List<SpecMenuItem> _menu;

      public the_menu_of_the_line_of_the_source_branch() => _menu = TheMenuOfTheLineOf("source");

      [XF] public void cannot_rename_or_delete() => WhetherRenameRenameWithParentsAndDeleteAreEnabled(_menu).Must().SequenceEqual([false, false, false]);
      [XF] public void says_why_once() => _menu[^1].Name.Must().Be("source holds every note's source, so it's never renamed or deleted.");
   }

   public class the_menu_of_the_line_of_a_branch_in_the_source_branch : When_editing_the_tag_tree_in_the_workspace
   {
      [XF] public void can_rename_and_delete() => WhetherRenameRenameWithParentsAndDeleteAreEnabled(TheMenuOfTheLineOf(Natsume)).Must().SequenceEqual([true, true, true]);
   }
}
