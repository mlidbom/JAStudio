using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Tagging;

public class NoteTags : IEnumerable<Tag>
{
   readonly JPNote _note;
   readonly HashSet<Tag> _tags = [];

   public NoteTags(JPNote note, List<string>? tags = null)
   {
      _note = note;

      if(tags != null)
      {
         foreach(var tagName in tags)
         {
            _tags.Add(Tag.FromName(tagName));
         }
      }
   }

   /// <summary>
   /// Lets the note refresh everything it derives from its tags, then flushes it.<br/>
   /// The refresh comes first because flushing caches a snapshot of the note, and that snapshot must reflect the new tags.
   /// </summary>
   void Persist()
   {
      _note.OnTagsUpdated();
      _note.Flush();
   }

   public bool Contains(Tag value) => _tags.Contains(value);

   public void Set(Tag tag)
   {
      if(!Contains(tag))
      {
         _tags.Add(tag);
         Persist();
      }
   }

   public void Unset(Tag tag)
   {
      if(Contains(tag))
      {
         _tags.Remove(tag);
         Persist();
      }
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

   public IEnumerator<Tag> GetEnumerator() => _tags.GetEnumerator();

   IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

   public List<string> ToStringList()
   {
      return this.Select(t => t.Name).OrderBy(n => n).ToList();
   }
}
