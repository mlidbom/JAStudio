using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// A tag path in the <see cref="TagTree"/>, such as <c>source::anime</c>, with the paths one level below it as its <see cref="Children"/>,<br/>
/// and the number of notes in its branch. The path needn't be a tag itself: <c>source</c> is in the tree as the parent of <c>source::anime</c>.
/// </summary>
public sealed class TagTreeNode
{
   internal TagTreeNode(string tagPath, int numberOfNotesInTheBranch, IReadOnlyList<TagTreeNode> children)
   {
      TagPath = tagPath;
      LastLevel = tagPath.Split(Tag.LevelSeparator)[^1];
      NumberOfNotesInTheBranch = numberOfNotesInTheBranch;
      Children = children;
   }

   /// <summary>The whole path, such as <c>source::anime</c>.</summary>
   public string TagPath { get; }

   /// <summary>The last level of <see cref="TagPath"/>, such as <c>anime</c> for <c>source::anime</c>, which is all a tree shows below the node's parent.</summary>
   public string LastLevel { get; }

   /// <summary>How many notes have <see cref="TagPath"/>, or a tag below it. A note with several tags in the branch counts once.</summary>
   public int NumberOfNotesInTheBranch { get; }

   /// <summary>
   /// The nodes of the paths one level below, such as <c>source::anime::natsume</c> below <c>source::anime</c>,<br/>
   /// in alphabetical order ignoring case, with the numbers in them compared as numbers, so <c>s2</c> comes before <c>s10</c>.
   /// </summary>
   public IReadOnlyList<TagTreeNode> Children { get; }

   /// <summary>The paths in the node's branch: <see cref="TagPath"/> and every path below it.</summary>
   public IEnumerable<string> PathsInTheBranch() => [TagPath, ..Children.SelectMany(it => it.PathsInTheBranch())];

   /// <summary>
   /// This node with only the paths below it that contain every part of <paramref name="typedParts"/>, and their parents,<br/>
   /// or null when no path in its branch does. See <see cref="TagTree.WithOnlyThePathsMatching"/>.
   /// </summary>
   internal TagTreeNode? WithOnlyThePathsMatching(TypedTagPathParts typedParts)
   {
      if(EveryPathInTheBranchMatches(typedParts)) return this;

      var matchingChildren = Children.Select(it => it.WithOnlyThePathsMatching(typedParts)).OfType<TagTreeNode>().ToList();
      return matchingChildren.Count > 0 ? new TagTreeNode(TagPath, NumberOfNotesInTheBranch, matchingChildren) : null;
   }

   /// <summary>Whether <see cref="TagPath"/> contains every part of <paramref name="typedParts"/>. Every path below it contains it, so then they do too.</summary>
   bool EveryPathInTheBranchMatches(TypedTagPathParts typedParts) => typedParts.AreAllIn(TagPath);
}
