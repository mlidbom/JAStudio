using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// Renaming a <see cref="TagBranch"/> on every note, as the tag management's Rename and Rename with parents do: each tag in the branch<br/>
/// gets the <see cref="Renamed"/> branch's path in place of the branch's, so <c>source::anime::natsume</c> becomes <c>source::tv::natsume</c>.<br/>
/// It's made from the name a person typed, so it says why it can't be done, when it can't, and which tags in use it would merge with.
/// </summary>
/// <remarks>
/// It works on tag names, not on <see cref="Tag"/> objects, until it's applied: the first <see cref="Tag"/> made with a name keeps its spelling,<br/>
/// so a <see cref="Tag"/> made for a name half typed, such as <c>Tv</c> on the way to <c>tv</c>, would give the renamed tag that spelling.
/// </remarks>
public sealed class TagBranchRename
{
   TagBranchRename(TagBranch branch, string renamedPath, IReadOnlyCollection<Tag> tagsInUse, string? whyTheTypedNameCannotBeUsed)
   {
      Branch = branch;
      Renamed = new TagBranch(renamedPath);
      var tagNamesInTheBranch = tagsInUse.Where(branch.Contains).Select(it => it.Name).ToList();
      var tagNamesInUseOutsideTheBranch = tagsInUse.Where(it => !branch.Contains(it)).Select(it => it.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
      TagsInUseItMergesWith = [..tagNamesInTheBranch.Select(RenamedName).Where(tagNamesInUseOutsideTheBranch.Contains).Order(StringComparer.OrdinalIgnoreCase)];
      WhyItCannotBeDone = whyTheTypedNameCannotBeUsed ?? WhyItCannotBeRenamed();
      return;

      string RenamedName(string tagName) => branch.TheNameRenamedTo(Renamed, tagName);

      string? WhyItCannotBeRenamed()
      {
         if(branch.WhyItCannotBeRenamedByHand(tagsInUse) is {} whyNot)
            return whyNot;
         if(renamedPath.Equals(branch.Path, StringComparison.Ordinal))
            return "Type the new name.";
         if(renamedPath.Equals(branch.Path, StringComparison.OrdinalIgnoreCase))
            return "Only the case differs, and tags ignore case, so the tag can't be renamed this way.";
         if(branch.Contains(renamedPath))
            return "A tag can't be moved below itself.";
         if(Renamed.IsInTheAnkiBranch != branch.IsInTheAnkiBranch)
            return Tags.Anki.WhyNoTagMovesIntoOrOutOfIt;
         return tagNamesInTheBranch.Where(it => Tags.IsDefinedByJAStudio(RenamedName(it)))
                                   .Select(it => $"{it} would become {RenamedName(it)}, which JAStudio defines.")
                                   .FirstOrDefault();
      }
   }

   /// <summary>Renaming only the last level of <paramref name="branch"/>'s path to <paramref name="typedLastLevel"/>, as Rename does, so the branch stays where it is.</summary>
   /// <param name="branch">The branch to rename.</param>
   /// <param name="typedLastLevel">The new last level, as typed. It can't hold <c>::</c>, which would move the branch.</param>
   /// <param name="tagsInUse">Every tag some note has, as <see cref="Collection.JPCollection.TagsInUse"/> lists them.</param>
   public static TagBranchRename OfTheLastLevel(TagBranch branch, string typedLastLevel, IReadOnlyCollection<Tag> tagsInUse) =>
      new(branch, branch.WithTheLastLevel(typedLastLevel).Path, tagsInUse, WhyItCannotBeALevel(typedLastLevel));

   /// <summary>Renaming the whole path of <paramref name="branch"/> to <paramref name="typedPath"/>, as Rename with parents does, which can move the branch.</summary>
   /// <param name="branch">The branch to rename.</param>
   /// <param name="typedPath">The new path, as typed.</param>
   /// <param name="tagsInUse">Every tag some note has, as <see cref="Collection.JPCollection.TagsInUse"/> lists them.</param>
   public static TagBranchRename WithItsParents(TagBranch branch, string typedPath, IReadOnlyCollection<Tag> tagsInUse) =>
      new(branch, typedPath, tagsInUse, Tag.WhyItCannotBeTheNameOfATag(typedPath));

   /// <summary>The branch renamed.</summary>
   public TagBranch Branch { get; }

   /// <summary>The branch it becomes, such as <c>source::tv</c> when <c>source::anime</c> is renamed to <c>tv</c>.</summary>
   public TagBranch Renamed { get; }

   /// <summary>Why the rename can't be done, such as "A tag can't contain spaces.", or null when it can.</summary>
   public string? WhyItCannotBeDone { get; }

   /// <summary>
   /// The names of the tags in use, outside the branch, that tags in the branch would be renamed to, in alphabetical order.<br/>
   /// The rename merges each with the tag renamed to it: afterwards their notes have the same tag, and can't be told apart, so renaming back doesn't undo it.
   /// </summary>
   public IReadOnlyList<string> TagsInUseItMergesWith { get; }

   /// <summary>
   /// Renames the tags in <see cref="Branch"/>, a branch of the notes' own tags, on <paramref name="note"/>, writing it once. Only when <see cref="WhyItCannotBeDone"/><br/>
   /// is null. A branch below <c>anki::</c> is renamed in Anki instead (<see cref="AnkiTagChanges.Rename"/>).
   /// </summary>
   public void ApplyTo(JPNote note)
   {
      Contract.State.Assert(WhyItCannotBeDone is null, () => $"The rename of {Branch.Path} to {Renamed.Path} can't be done: {WhyItCannotBeDone}");
      Contract.State.Assert(!Branch.IsInTheAnkiBranch, () => $"{Branch.Path} is a branch of Anki tags, renamed in Anki.");
      note.Tags.RenameTheBranch(Branch, Renamed);
   }

   static string? WhyItCannotBeALevel(string typedLevel) =>
      typedLevel.Contains(Tag.LevelSeparator)
         ? $"A level can't contain {Tag.LevelSeparator}. Rename with parents moves a tag below other tags."
         : Tag.WhyItIsNotWellFormed(typedLevel);
}
