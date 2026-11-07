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
   /// A tag in <paramref name="tagsInUse"/> that is in this branch and that JAStudio defines, or null when there is none.<br/>
   /// Such tags are generated or have editors of their own, so a branch holding one can't be renamed or deleted by hand (<see cref="Tags.IsDefinedByJAStudio"/>).
   /// </summary>
   public Tag? ATagDefinedByJAStudioIn(IEnumerable<Tag> tagsInUse) => tagsInUse.FirstOrDefault(it => Contains(it) && Tags.IsDefinedByJAStudio(it.Name));
}
