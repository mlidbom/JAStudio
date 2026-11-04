namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

public class DerivedFromField
{
   readonly RelatedVocabData _data;
   readonly NoteGuard _guard;

   public DerivedFromField(RelatedVocabData data, NoteGuard guard)
   {
      _data = data;
      _guard = guard;
   }

   public string Get() => _data.DerivedFrom.Get();

   public void Set(string value)
   {
      _guard.Update(() => _data.DerivedFrom.Set(value));
   }

   public override string ToString() => Get();
}
