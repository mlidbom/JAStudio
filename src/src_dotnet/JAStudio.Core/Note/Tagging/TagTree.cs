using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// The tag tree: the tree all tag paths in use form, for browsing the tags. Each path is a <see cref="TagTreeNode"/>,<br/>
/// with the paths one level below it as its children, and the number of notes in its branch.<br/>
/// It's computed from the notes' tags when it's made, so a tag no note has isn't in it. Made by <see cref="JPCollection.TagTree"/>.
/// </summary>
/// <remarks>
/// Tags ignore case, so paths that differ only in case are one node, spelled as the first note found spells it.<br/>
/// The tree has a few hundred nodes, so it's made whole every time rather than kept up to date.
/// </remarks>
public sealed class TagTree
{
   static readonly StringComparer IgnoringCase = StringComparer.OrdinalIgnoreCase;
   static readonly StringComparer LevelsWithNumbersAsNumbers = StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.NumericOrdering | CompareOptions.IgnoreCase);

   /// <summary>The tag tree of the tags <paramref name="notes"/> have.</summary>
   internal TagTree(IReadOnlyList<JPNote> notes)
   {
      var numberOfNotesInTheBranchOf = NumberOfNotesInEachBranchOf(notes);
      var branchPathsOfEachTag = notes.SelectMany(it => it.Tags).Distinct().Select(it => it.PathsOfTheBranchesItIsIn).ToList();
      var pathsOneLevelBelow = branchPathsOfEachTag.SelectMany(PairsOfAPathAndThePathOneLevelBelow).ToLookup(it => it.Path, it => it.PathOneLevelBelow, IgnoringCase);

      TopLevel = NodesFor(branchPathsOfEachTag.Select(it => it[0]));
      return;

      IReadOnlyList<TagTreeNode> NodesFor(IEnumerable<string> paths) =>
      [
         ..paths.Distinct(IgnoringCase)
                .Select(path => new TagTreeNode(path, numberOfNotesInTheBranchOf[path], NodesFor(pathsOneLevelBelow[path])))
                .OrderBy(it => it.LastLevel, LevelsWithNumbersAsNumbers)
      ];
   }

   TagTree(IReadOnlyList<TagTreeNode> topLevel) => TopLevel = topLevel;

   /// <summary>The nodes of the paths with a single level, such as <c>source</c>, in the order of <see cref="TagTreeNode.Children"/>.</summary>
   public IReadOnlyList<TagTreeNode> TopLevel { get; }

   /// <summary>
   /// The tree with only the tag paths that contain every part of <paramref name="typedParts"/>, and their parents, as a filter box shows it.<br/>
   /// So <c>s3</c> leaves each <c>s3</c> with everything below it, and the paths above it. A node keeps the number of notes in its whole branch.
   /// </summary>
   public TagTree WithOnlyThePathsMatching(TypedTagPathParts typedParts) => new([..TopLevel.Select(it => it.WithOnlyThePathsMatching(typedParts)).OfType<TagTreeNode>()]);

   /// <summary>How many notes are in each branch, by the branch's path. A note is in a branch once, however many of its tags are in it.</summary>
   static Dictionary<string, int> NumberOfNotesInEachBranchOf(IEnumerable<JPNote> notes)
   {
      var numberOfNotesIn = new Dictionary<string, int>(IgnoringCase);
      foreach(var branchPath in notes.SelectMany(PathsOfTheBranchesTheNoteIsIn))
         numberOfNotesIn[branchPath] = numberOfNotesIn.GetValueOrDefault(branchPath) + 1;
      return numberOfNotesIn;

      static IEnumerable<string> PathsOfTheBranchesTheNoteIsIn(JPNote note) => note.Tags.SelectMany(it => it.PathsOfTheBranchesItIsIn).Distinct(IgnoringCase);
   }

   /// <summary>Each path in <paramref name="branchPaths"/>, a tag's paths from the top level down, paired with the next one, which is one level below it.</summary>
   static IEnumerable<(string Path, string PathOneLevelBelow)> PairsOfAPathAndThePathOneLevelBelow(IReadOnlyList<string> branchPaths) => branchPaths.Zip(branchPaths.Skip(1));
}
