using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// A branch of the tag tree: the tag at <see cref="Path"/>, such as <c>source::anime</c>, and every tag below it, such as <c>source::anime::natsume</c>.<br/>
/// The path needn't be a tag itself. Only whole levels count, so <c>source::ani</c> holds neither. Paths compare ignoring case, as tags do.
/// </summary>
public sealed class TagBranch(string path)
{
   /// <summary>The path of the branch's top, such as <c>source::anime</c>.</summary>
   public string Path { get; } = path;

   /// <summary>The last level of <see cref="Path"/>, such as <c>anime</c> for <c>source::anime</c>.</summary>
   public string LastLevel => Levels[^1];

   /// <summary>The paths of the branches this one is in, from the top level down: <c>source</c> for <c>source::anime</c>, and none for <c>source</c>.</summary>
   public IReadOnlyList<string> PathsAboveIt => [..Enumerable.Range(1, Levels.Length - 1).Select(levelCount => string.Join(Tag.LevelSeparator, Levels.Take(levelCount)))];

   string[] Levels => Path.Split(Tag.LevelSeparator);

   /// <summary>Whether <paramref name="tag"/> is the tag at <see cref="Path"/> or a tag below it.</summary>
   public bool Contains(Tag tag) => Contains(tag.Name);

   /// <summary>Whether the tag named <paramref name="tagName"/> is the tag at <see cref="Path"/> or a tag below it.</summary>
   public bool Contains(string tagName) =>
      tagName.Equals(Path, StringComparison.OrdinalIgnoreCase)
   || tagName.StartsWith(Path + Tag.LevelSeparator, StringComparison.OrdinalIgnoreCase);

   /// <summary>This branch with the last level of its path replaced by <paramref name="lastLevel"/>, such as <c>source::tv</c> for <c>source::anime</c> and <c>tv</c>.</summary>
   public TagBranch WithTheLastLevel(string lastLevel) => new(string.Join(Tag.LevelSeparator, [..PathsAboveIt.TakeLast(1), lastLevel]));

   /// <summary>
   /// The name the tag named <paramref name="tagName"/>, which is in this branch, gets when the branch is renamed to <paramref name="renamed"/>:<br/>
   /// <c>source::anime::natsume</c> becomes <c>source::tv::natsume</c> when <c>source::anime</c> is renamed to <c>source::tv</c>.
   /// </summary>
   public string TheNameRenamedTo(TagBranch renamed, string tagName)
   {
      Contract.Argument.Assert(Contains(tagName));
      return renamed.Path + tagName[Path.Length..];
   }

   /// <summary>
   /// Why this branch can't be renamed or moved by hand, or null when it can, given <paramref name="tagsInUse"/>, every tag some note has:<br/>
   /// it's the source branch itself (<see cref="Tags.Source.Branch"/>), or it holds a tag whose name JAStudio's code uses (<see cref="Tags.IsDefinedByJAStudio"/>).
   /// </summary>
   public string? WhyItCannotBeRenamedByHand(IEnumerable<Tag> tagsInUse) =>
      IsTheSourceBranch
         ? WhyTheSourceBranchIsNeverChanged
         : tagsInUse.FirstOrDefault(it => Contains(it) && Tags.IsDefinedByJAStudio(it.Name)) is {} defined
            ? $"{Path} holds {defined.Name}, whose name JAStudio uses, so it can't be renamed by hand."
            : null;

   /// <summary>
   /// Why this branch can't be deleted by hand, or null when it can, given <paramref name="tagsInUse"/>, every tag some note has:<br/>
   /// it's the source branch itself (<see cref="Tags.Source.Branch"/>), or it holds a tag JAStudio maintains (<see cref="Tags.IsMaintainedByJAStudio"/>).
   /// </summary>
   public string? WhyItCannotBeDeletedByHand(IEnumerable<Tag> tagsInUse) =>
      IsTheSourceBranch
         ? WhyTheSourceBranchIsNeverChanged
         : tagsInUse.FirstOrDefault(it => Contains(it) && Tags.IsMaintainedByJAStudio(it.Name)) is {} maintained
            ? $"{Path} holds {maintained.Name}, which JAStudio maintains, so it can't be deleted by hand."
            : null;

   /// <summary>Whether this branch is in the source branch (<see cref="Tags.Source"/>), the source branch itself included: changing its tags changes notes' sources.</summary>
   public bool IsInTheSourceBranch => Tags.Source.Branch.Contains(Path);

   bool IsTheSourceBranch => Path.Equals(Tags.Source.Branch.Path, StringComparison.OrdinalIgnoreCase);

   const string WhyTheSourceBranchIsNeverChanged = "source holds every note's source, so it's never renamed or deleted.";
}
