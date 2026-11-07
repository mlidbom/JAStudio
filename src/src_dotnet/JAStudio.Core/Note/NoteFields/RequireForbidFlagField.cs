using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.NoteFields;

public class RequireForbidFlagField
{
   readonly JPNote _note;
   readonly Tag _requiredTag;
   readonly Tag _forbiddenTag;

   public int RequiredWeight { get; }
   public int ForbiddenWeight { get; }

   public RequireForbidFlagField(JPNote note, int requiredWeight, int forbiddenWeight, Tag requiredTag, Tag forbiddenTag)
   {
      _note = note;
      _requiredTag = requiredTag;
      _forbiddenTag = forbiddenTag;
      RequiredWeight = requiredWeight;
      ForbiddenWeight = forbiddenWeight;

      // Cache tag states during construction - invalidated when matching_configuration is recreated
      IsConfiguredRequired = note.Tags.Contains(requiredTag);
      IsConfiguredForbidden = note.Tags.Contains(forbiddenTag);
   }

   public bool IsConfiguredRequired { get; }

   public bool IsConfiguredForbidden { get; }

   public int MatchWeight => IsRequired ? RequiredWeight : IsForbidden ? ForbiddenWeight : 0;

   public string Name => _requiredTag.Name.Replace(Tags.Vocab.Matching.Requires.FolderName, "");

   public virtual bool IsRequired => IsConfiguredRequired;
   public bool IsForbidden => IsConfiguredForbidden;
   public bool IsActive => IsConfiguredRequired || IsConfiguredForbidden;

   public void SetForbidden(bool value)
   {
      if(value)
      {
         _note.Tags.Set(_forbiddenTag);
         _note.Tags.Unset(_requiredTag);
      } else
      {
         _note.Tags.Unset(_forbiddenTag);
      }
   }

   public void SetRequired(bool value)
   {
      if(value)
      {
         _note.Tags.Set(_requiredTag);
         _note.Tags.Unset(_forbiddenTag);
      } else
      {
         _note.Tags.Unset(_requiredTag);
      }
   }

   public override string ToString() => $"{Name} required: {IsRequired}, forbidden: {IsForbidden}";
}
