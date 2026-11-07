using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.NoteFields;

public class TagFlagField
{
   readonly JPNote _note;
   public readonly Tag Tag;
   readonly bool _cachedIsSet;

   public TagFlagField(JPNote note, Tag tag)
   {
      _note = note;
      Tag = tag;
      // Cache tag state during construction - invalidated when matching_configuration is recreated
      _cachedIsSet = note.Tags.Contains(tag);
   }

   public bool IsSet() => _cachedIsSet;

   public void SetTo(bool set)
   {
      if(set)
      {
         _note.Tags.Set(Tag);
      } else
      {
         _note.Tags.Unset(Tag);
      }
   }

   public override string ToString() => $"{Tag.Name}: {IsSet()}";
}
