using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

public class ConfusedWithField
{
   readonly RelatedVocabData _data;
   readonly NoteGuard _guard;

   public ConfusedWithField(RelatedVocabData data, NoteGuard guard)
   {
      _data = data;
      _guard = guard;
   }

   public HashSet<string> Get() => _data.ConfusedWith;

   public void Add(string value) => _guard.Update(() => _data.ConfusedWith.Add(value));

   public void Remove(string value) => _guard.Update(() => _data.ConfusedWith.Remove(value));

   public override string ToString() => string.Join(", ", _data.ConfusedWith);
}
