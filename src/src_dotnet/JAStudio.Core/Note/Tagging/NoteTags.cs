using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Compze.Contracts;

namespace JAStudio.Core.Note.Tagging;

public class NoteTags : IEnumerable<Tag>
{
   readonly JPNote _note;
   readonly HashSet<Tag> _tags = [];

   public NoteTags(JPNote note, List<string>? tags = null)
   {
      _note = note;
      foreach(var tagName in tags ?? []) _tags.Add(Tag.FromName(tagName));
   }

   public bool Contains(Tag value) => _tags.Contains(value);

   public void Set(Tag tag)
   {
      if(_tags.Add(tag)) _note.Flush();
   }

   public void Unset(Tag tag)
   {
      if(_tags.Remove(tag)) _note.Flush();
   }

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

      _tags.ExceptWith(inTheBranch);
      _tags.UnionWith(inTheBranch.Select(it => Tag.FromName(branch.TheNameRenamedTo(renamed, it.Name))));
      _note.Flush();
   }

   /// <summary>Removes the note's tags in <paramref name="branch"/>: the tag at its path and every tag below it, writing the note once.</summary>
   public void RemoveTheBranch(TagBranch branch)
   {
      if(_tags.RemoveWhere(branch.Contains) > 0)
         _note.Flush();
   }

   public IEnumerator<Tag> GetEnumerator() => _tags.GetEnumerator();

   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

   public List<string> ToStringList()
   {
      return [.. this.Select(t => t.Name).OrderBy(n => n)];
   }

   /// <summary>
   /// The note's tags that a person manages, in alphabetical order: all but the jastudio-defined-tags (<see cref="Tags.IsDefinedByJAStudio"/>),<br/>
   /// which are generated or have editors of their own. These are the tags shown with the note, and added and removed by hand.
   /// </summary>
   public IReadOnlyList<Tag> NotDefinedByJAStudio() =>
      [..this.Where(it => !Tags.IsDefinedByJAStudio(it.Name)).OrderBy(it => it.Name, StringComparer.OrdinalIgnoreCase)];

   /// <summary>
   /// Makes <paramref name="tags"/>, none of which is a jastudio-defined-tag, the note's tags that a person manages (<see cref="NotDefinedByJAStudio"/>):<br/>
   /// unsets the others a person manages and sets these, leaving the jastudio-defined-tags as they are.
   /// </summary>
   public void ReplaceTheTagsNotDefinedByJAStudio(IReadOnlyList<Tag> tags)
   {
      Contract.Argument.Assert(!tags.Any(it => Tags.IsDefinedByJAStudio(it.Name)), () => $"{string.Join(", ", tags.Select(it => it.Name))} includes a jastudio-defined-tag.");

      foreach(var removed in NotDefinedByJAStudio().Except(tags)) Unset(removed);
      foreach(var tag in tags) Set(tag);
   }
}
