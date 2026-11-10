using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json.Nodes;
using JAStudio.Core.Note;
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
   readonly AnkiTagChanges _ankiTagChanges;

   public TagTools(JPCollection collection, AnkiTagChanges ankiTagChanges)
   {
      _collection = collection;
      _ankiTagChanges = ankiTagChanges;
   }

   [McpServerTool(Name = "get_the_tag_tree", ReadOnly = true, Idempotent = true)]
   [Description("""
                Gets the tag tree, as the workspace's tag management shows it: every tag path some note has, and every parent of one, such as
                source above source::anime, each with the vocab, kanji and sentences in its branch, meaning those that have the path's tag or a tag
                below it, counted, and how many of them are being studied. A note with several tags in a branch counts once. A note type with no
                note in the branch is left out. Gets the whole tree, or one branch, to as many levels below it as asked for: a node whose children
                aren't listed says how many paths are below it. The notes' tags in Anki, which JAStudio only reads, are in the anki branch.
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

   [McpServerTool(Name = "add_a_tag_to_the_notes_found", Destructive = false, Idempotent = true)]
   [Description("""
                Adds a tag to every note a search finds that doesn't have it, as adding it to a note's tags in the editor does, and saves
                each note it changes to the corpus checkout. The search is in the form search_notes takes, such as
                {"allOf": [{"noteTypeIs": "vocab"}, {"not": {"tagsInclude": "source"}}]}. Refuses a tag JAStudio maintains, which is
                generated, as the editor does. A tag below anki::, such as anki::marked, is one of the notes' tags in Anki: it's added to
                the study-notes of the notes that have one, in Anki, as one Anki operation, which Anki's Edit > Undo undoes, and the
                headless JAStudio, which has no Anki, refuses it. Reports the notes it added the tag to.
                """)]
   public NotesTheTagChangedOn AddATagToTheNotesFound(
      [Description("The search finding the notes, in the form search_notes takes.")] JsonObject search,
      [Description("The whole path of the tag to add, such as -::grammar-term.")] string tag,
      [Description("The most notes to list. Every note the tag is added to is counted either way.")] int maxNotesListed = NoteTools.NotesListedByDefault)
   {
      var tagToAdd = TagManagedByHand(tag);
      var notesWithoutTheTag = NotesFoundBy(search).Where(it => !it.AllTags.Contains(tagToAdd) && (!tagToAdd.IsAnAnkiTag || it.HasAnAnkiNote())).ToList();
      if(tagToAdd.IsAnAnkiTag)
         _ankiTagChanges.AddTo(notesWithoutTheTag, tagToAdd);
      else
         foreach(var note in notesWithoutTheTag) note.Tags.Set(tagToAdd);

      return new NotesTheTagChangedOn(notesWithoutTheTag.Count, [..notesWithoutTheTag.Take(maxNotesListed).Select(NoteSummary.Of)]);
   }

   [McpServerTool(Name = "remove_a_tag_from_the_notes_found", Destructive = true, Idempotent = true)]
   [Description("""
                Removes a tag from every note a search finds that has it, as removing it from a note's tags in the editor does, and saves
                each note it changes to the corpus checkout. Removes the tag itself only, not the tags below it. The search is in the form
                search_notes takes, such as {"allOf": [{"noteTypeIs": "kanji"}, {"tagsInclude": "source::lyrics"}]}. Refuses a tag JAStudio
                maintains, which is generated, as the editor does. A tag below anki::, such as anki::marked, is one of the notes' tags in
                Anki: it's removed from their study-notes in Anki, as one Anki operation, which Anki's Edit > Undo undoes, and the headless
                JAStudio, which has no Anki, refuses it. Reports the notes it removed the tag from.
                """)]
   public NotesTheTagChangedOn RemoveATagFromTheNotesFound(
      [Description("The search finding the notes, in the form search_notes takes.")] JsonObject search,
      [Description("The whole path of the tag to remove, such as source::anime::violet::08.")] string tag,
      [Description("The most notes to list. Every note the tag is removed from is counted either way.")] int maxNotesListed = NoteTools.NotesListedByDefault)
   {
      var tagToRemove = TagManagedByHand(tag);
      var notesWithTheTag = NotesFoundBy(search).Where(it => it.AllTags.Contains(tagToRemove)).ToList();
      if(tagToRemove.IsAnAnkiTag)
         _ankiTagChanges.RemoveFrom(notesWithTheTag, tagToRemove);
      else
         foreach(var note in notesWithTheTag) note.Tags.Unset(tagToRemove);

      return new NotesTheTagChangedOn(notesWithTheTag.Count, [..notesWithTheTag.Take(maxNotesListed).Select(NoteSummary.Of)]);
   }

   [McpServerTool(Name = "delete_a_tag_branch", Destructive = true, Idempotent = true)]
   [Description("""
                Deletes a tag and every tag below it, as Delete in the tag tree of the workspace's tag management does. A branch of the
                notes' own tags, such as tmp, is removed from every note with a tag in it, and each note changed is saved to the corpus
                checkout. A branch below anki::, such as anki::tmp, is one of the notes' tags in Anki: it's deleted in Anki from every Anki
                note with a tag in it, of whatever note type, not only from the study-notes, as one Anki operation, which Anki's Edit > Undo
                undoes, and the headless JAStudio, which has no Anki, refuses it. Refuses a branch that can't be deleted by hand, saying why,
                as the tag tree does: the source branch and the anki branch themselves, and a branch holding a tag JAStudio maintains.
                Reports how many notes had a tag in the branch.
                """)]
   public DeletedTagBranch DeleteATagBranch([Description("The path of the branch to delete, such as tmp or anki::tmp.")] string branch)
   {
      var branchToDelete = new TagBranch(branch);
      if(branchToDelete.WhyItCannotBeDeletedByHand(_collection.TagsInUse()) is {} whyNot) throw new ArgumentException(whyNot, nameof(branch));

      var notesWithATagInIt = _collection.NotesWithATagIn(branchToDelete);
      if(!branchToDelete.IsInTheAnkiBranch)
      {
         foreach(var note in notesWithATagInIt) note.Tags.RemoveTheBranch(branchToDelete);
         return new DeletedTagBranch(branch, notesWithATagInIt.Count, AnkiNotesThatHadATagInIt: null);
      }

      var ankiNotesWithATagInIt = _ankiTagChanges.NumberOfAnkiNotesWithATagIn(branchToDelete);
      _ankiTagChanges.Delete(branchToDelete);
      return new DeletedTagBranch(branch, notesWithATagInIt.Count, ankiNotesWithATagInIt);
   }

   List<JPNote> NotesFoundBy(JsonObject search) => _collection.NotesMatching(NoteConditionJson.ConditionFrom(search));

   /// <summary>
   /// The tag named <paramref name="tagName"/>, refusing one that can't be a tag a person manages (<see cref="Tags.WhyItCannotBeATagAPersonManages"/>),<br/>
   /// such as a tag JAStudio maintains, which is generated, as the editor refuses it.
   /// </summary>
   static Tag TagManagedByHand(string tagName) =>
      Tags.WhyItCannotBeATagAPersonManages(tagName) is {} whyNot
         ? throw new ArgumentException(whyNot, nameof(tagName))
         : Tag.FromName(tagName);
}
