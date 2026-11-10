using System;
using System.Collections.Generic;
using System.ComponentModel;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Tagging;

/// <summary>The tools about the tags the notes have.</summary>
[McpServerToolType]
public class TagTools
{
   /// <summary>How many levels below the branch asked for <see cref="GetTheTagTree"/> lists, unless asked for more or fewer.</summary>
   public const int LevelsListedByDefault = 2;

   readonly JPCollection _collection;

   public TagTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "get_the_tag_tree", ReadOnly = true, Idempotent = true)]
   [Description("""
                Gets the tag tree, as the workspace's tag management shows it: every tag path some note has, and every parent of one, such as
                source above source::anime, each with the vocab, kanji and sentences in its branch, meaning those that have the path's tag or a tag
                below it, counted, and how many of them are being studied. A note with several tags in a branch counts once. A note type with no
                note in the branch is left out. Gets the whole tree, or one branch, to as many levels below it as asked for: a node whose children
                aren't listed says how many paths are below it.
                """)]
   public IReadOnlyList<TagTreeNodeDescription> GetTheTagTree(
      [Description("The path of the branch to get, such as source or source::anime. Empty for the whole tree.")] string branch = "",
      [Description("How many levels to list below the branch's path, or below the top level for the whole tree.")] int levelsListed = LevelsListedByDefault)
   {
      var tagTree = _collection.TagTree();
      return branch == ""
                ? TagTreeNodeDescription.Of(tagTree.TopLevel, levelsListed)
                : [TagTreeNodeDescription.Of(NodeOf(branch), levelsListed)];

      TagTreeNode NodeOf(string path) => tagTree.NodeOf(new TagBranch(path)) ?? throw new ArgumentException($"No note has a tag in the branch {path}.", nameof(branch));
   }
}
