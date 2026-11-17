using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// A note's own tags: the corpus's, stored with the note. A change publishes <see cref="INoteTevent.PropertyUpdated.Tags"/>, the whole new set, which every<br/>
/// note applies (<see cref="Apply"/>).
/// </summary>
public class NoteTags : IEnumerable<Tag>
{
   readonly JPNote _note;
   IReadOnlySet<Tag> _tags;

   public NoteTags(JPNote note, List<string>? tags = null)
   {
      _note = note;
      _tags = (tags ?? []).Select(Tag.FromName).ToHashSet();
   }

   public bool Contains(Tag value) => _tags.Contains(value);

   /// <summary>Gives the note <paramref name="tag"/>, one of its own, never an Anki tag (<see cref="Tag.IsAnAnkiTag"/>), which goes to its study-note in Anki instead (<see cref="AnkiTagChanges"/>).</summary>
   public void Set(Tag tag)
   {
      Contract.Argument.Assert(!tag.IsAnAnkiTag, () => $"{tag.Name} is an Anki tag, which goes to the note's study-note in Anki.");
      SetTheTags([.. _tags, tag]);
   }

   public void Unset(Tag tag) => SetTheTags(_tags.Where(it => it != tag));

   public void Toggle(Tag tag, bool on)
   {
      if(on)
      {
         Set(tag);
      } else
      {
         Unset(tag);
      }
   }

   /// <summary>
   /// Renames the note's tags in <paramref name="branch"/> as renaming the branch to <paramref name="renamed"/> renames them (<see cref="TagBranch.TheNameRenamedTo"/>),<br/>
   /// writing the note once. A renamed tag the note already has stays once.
   /// </summary>
   public void RenameTheBranch(TagBranch branch, TagBranch renamed)
   {
      var inTheBranch = _tags.Where(branch.Contains).ToList();
      if(inTheBranch.Count == 0) return;

      SetTheTags([.. _tags.Except(inTheBranch), .. inTheBranch.Select(it => Tag.FromName(branch.TheNameRenamedTo(renamed, it.Name)))]);
   }

   /// <summary>Removes the note's tags in <paramref name="branch"/>: the tag at its path and every tag below it, writing the note once.</summary>
   public void RemoveTheBranch(TagBranch branch) => SetTheTags(_tags.Where(it => !branch.Contains(it)));

   /// <summary>Makes <paramref name="tags"/> the note's own tags: what their tevent's applier does.</summary>
   internal void Apply(IReadOnlySet<Tag> tags) => _tags = tags;

   void SetTheTags(IEnumerable<Tag> tags) => _note.PublishIfTheSetChanged(_tags, tags.ToHashSet(), static it => new NoteTevent.TagsUpdated(it));

   public IEnumerator<Tag> GetEnumerator() => _tags.GetEnumerator();

   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

   public List<string> ToStringList()
   {
      //bug: the corpus stores the tags in this order, which depends on the current culture: a machine with another culture would write the tags of
      //   every note it saves in another order. Ordinal order fixes it, with every note's file rewritten, which the main checkout's session makes as a
      //   change to the corpus. A vocab's related vocab have the same bug (dev_docs/taggregates/shared/what-the-notes-hold.md, "Noticed along the way").
      return [.. this.Select(t => t.Name).OrderBy(n => n)];
   }

   /// <summary>All the note's tags, in alphabetical order: those a person manages and those JAStudio maintains (<see cref="Tags.IsMaintainedByJAStudio"/>).</summary>
   public IReadOnlyList<Tag> InAlphabeticalOrder() => [..this.OrderBy(it => it.Name, StringComparer.OrdinalIgnoreCase)];

   /// <summary>
   /// The note's tags that a person manages, in alphabetical order: all but the jastudio-maintained-tags (<see cref="Tags.IsMaintainedByJAStudio"/>),<br/>
   /// which are generated. These are the tags added and removed by hand.
   /// </summary>
   public IReadOnlyList<Tag> NotMaintainedByJAStudio() => [..InAlphabeticalOrder().Where(it => !Tags.IsMaintainedByJAStudio(it.Name))];

   /// <summary>
   /// Makes <paramref name="tags"/>, none of which is a jastudio-maintained-tag, the note's tags that a person manages (<see cref="NotMaintainedByJAStudio"/>):<br/>
   /// unsets the others a person manages and sets these, leaving the jastudio-maintained-tags as they are.
   /// </summary>
   public void ReplaceTheTagsNotMaintainedByJAStudio(IReadOnlyList<Tag> tags)
   {
      Contract.Argument.Assert(!tags.Any(it => Tags.IsMaintainedByJAStudio(it.Name)), () => $"{string.Join(", ", tags.Select(it => it.Name))} includes a jastudio-maintained-tag.");

      foreach(var removed in NotMaintainedByJAStudio().Except(tags)) Unset(removed);
      foreach(var tag in tags) Set(tag);
   }
}
