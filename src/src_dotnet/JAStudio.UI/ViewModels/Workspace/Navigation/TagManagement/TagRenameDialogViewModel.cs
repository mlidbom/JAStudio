using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JAStudio.Core.Note.Tagging;
using JAStudio.UI.Views.Workspace.Navigation.TagManagement;

namespace JAStudio.UI.ViewModels.Workspace.Navigation.TagManagement;

/// <summary>
/// The small modal dialog that renames a branch of the tag tree (<see cref="TagRenameDialog"/>): only the last level of its path, as Rename does,<br/>
/// or its whole path, as Rename with parents does. It says how many notes change, why the typed name can't be used,<br/>
/// and warns when the rename merges tags, which can't be undone by renaming back. A branch below <c>anki::</c> is renamed in Anki, on every Anki note<br/>
/// with a tag in it, so it says how many Anki notes change.
/// </summary>
sealed partial class TagRenameDialogViewModel : ObservableObject
{
   /// <summary>How many of the tags merged with are named in the warning, so that a big merge doesn't make the dialog huge.</summary>
   const int MergedTagsNamed = 3;

   readonly Func<string, TagBranchRename> _renamingTo;

   TagRenameDialogViewModel(string title, string fixedPathAbove, string typedName, int numberOfNotesThatChange, Func<string, TagBranchRename> renamingTo)
   {
      Title = title;
      FixedPathAbove = fixedPathAbove;
      NumberOfNotesThatChange = numberOfNotesThatChange;
      _renamingTo = renamingTo;
      _typedName = typedName;
      _rename = renamingTo(typedName);
   }

   /// <summary>A dialog for Rename: only the last level of <paramref name="branch"/>'s path is typed, so the branch stays where it is.</summary>
   /// <param name="branch">The branch to rename.</param>
   /// <param name="numberOfNotesThatChange">How many notes have a tag in <paramref name="branch"/>: Anki notes, for a branch below <c>anki::</c>.</param>
   /// <param name="tagsInUse">Every tag some note has, to find what the rename would merge with.</param>
   public static TagRenameDialogViewModel ForRenamingTheLastLevelOf(TagBranch branch, int numberOfNotesThatChange, IReadOnlyCollection<Tag> tagsInUse) =>
      new($"Rename {branch.Path}",
          string.Concat(branch.PathsAboveIt.TakeLast(1).Select(it => it + Tag.LevelSeparator)),
          branch.LastLevel,
          numberOfNotesThatChange,
          typed => TagBranchRename.OfTheLastLevel(branch, typed, tagsInUse));

   /// <summary>A dialog for Rename with parents: the whole path of <paramref name="branch"/> is typed, which can move it below other tags.</summary>
   /// <param name="branch">The branch to rename.</param>
   /// <param name="numberOfNotesThatChange">How many notes have a tag in <paramref name="branch"/>: Anki notes, for a branch below <c>anki::</c>.</param>
   /// <param name="tagsInUse">Every tag some note has, to find what the rename would merge with.</param>
   public static TagRenameDialogViewModel ForRenamingWithItsParents(TagBranch branch, int numberOfNotesThatChange, IReadOnlyCollection<Tag> tagsInUse) =>
      new($"Rename {branch.Path} with its parents", "", branch.Path, numberOfNotesThatChange, typed => TagBranchRename.WithItsParents(branch, typed, tagsInUse));

   public string Title { get; }

   /// <summary>The part of the path that stays, shown before the typed name: <c>source::</c> when renaming the last level of <c>source::anime</c>. Empty when the whole path is typed.</summary>
   public string FixedPathAbove { get; }

   public int NumberOfNotesThatChange { get; }

   /// <summary>The new name, as typed: the last level, or the whole path for Rename with parents.</summary>
   [ObservableProperty] string _typedName;

   /// <summary>The rename the <see cref="TypedName"/> asks for, which the dialog closes with when it's accepted.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(WhyItCannotBeDone), nameof(MergeWarning), nameof(Merges), nameof(AcceptText), nameof(CanAccept), nameof(SourceWarning))]
   TagBranchRename _rename;

   partial void OnTypedNameChanged(string value) => Rename = _renamingTo(value);

   /// <summary>
   /// What the rename changes, such as "source::anime and every tag below it change on 3 notes.", or, for a branch below <c>anki::</c>,<br/>
   /// "anki::tmp and every tag below it change on 3 Anki notes, in Anki: Anki's Edit > Undo undoes it."
   /// </summary>
   public string WhatChanges => Rename.Branch.IsInTheAnkiBranch
                                   ? $"{Rename.Branch.Path} and every tag below it change on {Counted("Anki note")}, in Anki: Anki's Edit > Undo undoes it."
                                   : $"{Rename.Branch.Path} and every tag below it change on {Counted("note")}.";

   /// <inheritdoc cref="TagBranchRename.WhyItCannotBeDone"/>
   public string? WhyItCannotBeDone => Rename.WhyItCannotBeDone;

   /// <summary>Whether the rename merges tags with tags already in use. Then Enter doesn't accept it: only the Merge button does.</summary>
   public bool Merges => Rename.TagsInUseItMergesWith.Count > 0;

   /// <summary>The warning shown when the rename <see cref="Merges"/>, naming the first few tags merged with, or null when it doesn't.</summary>
   public string? MergeWarning => Merges
                                     ? $"This merges with {TheTagsMergedWith()}, already in use. Afterwards their notes can't be told apart, so renaming back won't undo it. Click Merge to go ahead."
                                     : null;

   /// <summary>
   /// The warning shown when the rename moves tags into, out of or within the source branch (<see cref="Tags.Source"/>), which changes<br/>
   /// the notes' sources, or null when it doesn't.
   /// </summary>
   public string? SourceWarning => Rename.Branch.IsInTheSourceBranch || Rename.Renamed.IsInTheSourceBranch
                                      ? $"This changes the source of {Counted("note")}."
                                      : null;

   string Counted(string whatChanges) => NumberOfNotesThatChange == 1 ? $"1 {whatChanges}" : $"{NumberOfNotesThatChange} {whatChanges}s";

   /// <summary>The accept button's text: Merge when the rename <see cref="Merges"/>, as a last reminder, and Rename otherwise.</summary>
   public string AcceptText => Merges ? "Merge" : "Rename";

   public bool CanAccept => WhyItCannotBeDone is null;

   /// <summary>Accepts the <see cref="Rename"/>. Set by the <see cref="TagRenameDialog"/>, which closes with it.</summary>
   public IRelayCommand? AcceptCommand { get; set; }

   /// <summary>Gives up on the rename. Set by the <see cref="TagRenameDialog"/>, which closes without one.</summary>
   public IRelayCommand? CancelCommand { get; set; }

   string TheTagsMergedWith()
   {
      var mergedWith = Rename.TagsInUseItMergesWith;
      var named = string.Join(", ", mergedWith.Take(MergedTagsNamed));
      return mergedWith.Count > MergedTagsNamed ? $"{named} and {mergedWith.Count - MergedTagsNamed} more" : named;
   }
}
