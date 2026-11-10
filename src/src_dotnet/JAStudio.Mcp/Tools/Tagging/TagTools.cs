using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Search;
using JAStudio.Core.Note.Tagging;
using JAStudio.Mcp.Tools.Notes;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Tagging;

/// <summary>The tools about the tags the notes have: browsing them, and changing them on many notes at once.</summary>
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

   [McpServerTool(Name = "remove_a_tag_from_the_notes_found", Destructive = true, Idempotent = true)]
   [Description("""
                Removes a tag from every note a search finds that has it, as removing it from a note's tags in the editor does, and saves
                each note it changes to the corpus checkout. Removes the tag itself only, not the tags below it. The search is in the form
                search_notes takes, such as {"allOf": [{"noteTypeIs": "kanji"}, {"tagsInclude": "source::lyrics"}]}. Refuses a tag JAStudio
                defines, which is generated or has an editor of its own, as the editor does. Reports the notes it removed the tag from.
                """)]
   public NotesTheTagWasRemovedFrom RemoveATagFromTheNotesFound(
      [Description("The search finding the notes, in the form search_notes takes.")] JsonObject search,
      [Description("The whole path of the tag to remove, such as source::anime::violet::08.")] string tag,
      [Description("The most notes to list. Every note the tag is removed from is counted either way.")] int maxNotesListed = NoteTools.NotesListedByDefault)
   {
      if(Tags.IsDefinedByJAStudio(tag))
         throw new ArgumentException($"{tag} is defined by JAStudio: it's generated or has an editor of its own, so it isn't removed by hand.", nameof(tag));

      var tagToRemove = Tag.FromName(tag);
      var notesWithTheTag = _collection.NotesMatching(NoteConditionJson.ConditionFrom(search)).Where(it => it.Tags.Contains(tagToRemove)).ToList();
      foreach(var note in notesWithTheTag) note.Tags.Unset(tagToRemove);

      return new NotesTheTagWasRemovedFrom(notesWithTheTag.Count, [..notesWithTheTag.Take(maxNotesListed).Select(NoteSummary.Of)]);
   }
}
