using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.TestUtils;
using JAStudio.UI.Menus.UIAgnosticMenuStructure;
using JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

// ReSharper disable InconsistentNaming

namespace JAStudio.UI.Specifications.AICreatedSpecs.Tagging;

/// <summary>
/// Renaming and deleting branches of the notes' Anki tags, below <c>anki::</c>, in the navigation pane's Tags tab: in Anki, on every Anki note with a tag<br/>
/// in the branch, here the pretend Anki of the specifications. The study-note of the sentence 家に帰る is tagged <c>tmp::a</c> and <c>marked</c> in Anki,<br/>
/// that of 学校に行く <c>tmp::b</c>, and the study-notes were read from Anki again. The <c>anki</c> line is expanded.
/// </summary>
public class When_editing_the_Anki_tags_in_the_tag_tree_in_the_workspace : SpecificationUsingAWorkspace, IAIGeneratedSpec
{
   static readonly TimeSpan TimeToWaitForTheChange = TimeSpan.FromSeconds(10);

   readonly TagManagementViewModel _tags;
   readonly PretendAnki _anki;
   readonly JPNote _sentenceAboutHome;
   readonly JPNote _sentenceAboutSchool;

   /// <summary>The rename dialog the tag management showed last.</summary>
   TagRenameDialogViewModel? _renameDialogShown;

   /// <summary>The question the delete confirmation asked last.</summary>
   string? _deleteQuestionAsked;

   public When_editing_the_Anki_tags_in_the_tag_tree_in_the_workspace()
   {
      _sentenceAboutHome = CreateTestSentence("家に帰る", "Go home");
      _sentenceAboutSchool = CreateTestSentence("学校に行く", "Go to school");
      _anki = GetService<PretendAnki>();
      _anki.Tag(_sentenceAboutHome, "tmp::a", "marked");
      _anki.Tag(_sentenceAboutSchool, "tmp::b");
      GetService<JPCollection>().ReadTheStudyNotesFromAnkiAgain();

      _tags = Workspace.Navigation.TagManagement;
      Workspace.Navigation.IsTagManagementShown = true;
      LineOf("anki").IsExpanded = true;
   }

   TagTreeNodeViewModel LineOf(string tagPath) => _tags.TopLevelLines.SelectMany(it => it.ThisAndAllBelow()).SingleOrDefault(it => it.TagPath == tagPath)
                                               ?? throw new InvalidOperationException($"No line shows {tagPath}.");

   string? SelectedPath() => _tags.SelectedLine?.TagPath;

   string PathOfTheSelectedLine() => SelectedPath() ?? throw new InvalidOperationException("No line is selected.");

   TagRenameDialogViewModel RenameDialogShown => _renameDialogShown ?? throw new InvalidOperationException("No rename dialog was shown.");

   string DeleteQuestionAsked => _deleteQuestionAsked ?? throw new InvalidOperationException("No delete confirmation was shown.");

   /// <summary>Answers the rename dialog as a person typing <paramref name="newName"/> and clicking its accept button would, when it can be clicked.</summary>
   Func<TagRenameDialogViewModel, Task<TagBranchRename?>> Typing(string newName) => dialog =>
   {
      _renameDialogShown = dialog;
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

   public class after_renaming_anki_tmp_to_scratch : When_editing_the_Anki_tags_in_the_tag_tree_in_the_workspace
   {
      public after_renaming_anki_tmp_to_scratch()
      {
         _tags.SelectedLine = LineOf("anki::tmp");
         _tags.RenameTheSelectedTag(Typing("scratch"));
         WaitUntilTheTreeShowsSelected("anki::scratch");
      }

      [XF] public void the_dialog_said_how_many_Anki_notes_change_and_that_Ankis_undo_undoes_it() =>
         RenameDialogShown.WhatChanges.Must().Be("anki::tmp and every tag below it change on 2 Anki notes, in Anki: Anki's Edit > Undo undoes it.");
      [XF] public void the_study_notes_have_the_renamed_tags_in_Anki() =>
         new { AboutHome = _anki.TagsOf(_sentenceAboutHome), AboutSchool = _anki.TagsOf(_sentenceAboutSchool) }.Must().Satisfy(it => it.AboutHome.SequenceEqual(["marked", "scratch::a"]) && it.AboutSchool.SequenceEqual(["scratch::b"]));
      [XF] public void the_tree_shows_the_renamed_line_selected() => PathOfTheSelectedLine().Must().Be("anki::scratch");
   }

   public class after_deleting_anki_tmp_and_confirming : When_editing_the_Anki_tags_in_the_tag_tree_in_the_workspace
   {
      public after_deleting_anki_tmp_and_confirming()
      {
         _tags.SelectedLine = LineOf("anki::tmp");
         _tags.DeleteTheSelectedTag(Answering(yes: true));
         WaitUntilTheTreeShowsSelected("anki");
      }

      [XF] public void the_question_said_how_many_Anki_notes_change_and_that_Ankis_undo_undoes_it() =>
         DeleteQuestionAsked.Must().Be("Delete anki::tmp and every tag below it from 2 Anki notes in Anki? Anki's Edit > Undo undoes it.");
      [XF] public void the_study_notes_keep_only_their_other_tags_in_Anki() =>
         new { AboutHome = _anki.TagsOf(_sentenceAboutHome), AboutSchool = _anki.TagsOf(_sentenceAboutSchool) }.Must().Satisfy(it => it.AboutHome.SequenceEqual(["marked"]) && !it.AboutSchool.Any());
   }

   List<SpecMenuItem> TheMenuOfTheLineOf(string tagPath)
   {
      _tags.SelectedLine = LineOf(tagPath);
      return _tags.SelectedLineMenu(rename: () => {}, renameWithParents: () => {}, delete: () => {});
   }

   static List<bool> WhetherRenameRenameWithParentsAndDeleteAreEnabled(List<SpecMenuItem> menu) =>
      [..menu.Where(it => it.Name.Contains("Rename") || it.Name.Contains("Delete")).Select(it => it.IsEnabled)];

   public class the_menu_of_the_line_of_the_anki_branch : When_editing_the_Anki_tags_in_the_tag_tree_in_the_workspace
   {
      readonly List<SpecMenuItem> _menu;

      public the_menu_of_the_line_of_the_anki_branch() => _menu = TheMenuOfTheLineOf("anki");

      [XF] public void cannot_rename_or_delete() => WhetherRenameRenameWithParentsAndDeleteAreEnabled(_menu).Must().SequenceEqual([false, false, false]);
      [XF] public void says_why_once() => _menu[^1].Name.Must().Be(Tags.Anki.WhyTheBranchItselfIsNeverChanged);
   }

   public class the_menu_of_the_line_of_a_branch_below_anki : When_editing_the_Anki_tags_in_the_tag_tree_in_the_workspace
   {
      [XF] public void can_rename_and_delete() => WhetherRenameRenameWithParentsAndDeleteAreEnabled(TheMenuOfTheLineOf("anki::tmp")).Must().SequenceEqual([true, true, true]);
   }
}
