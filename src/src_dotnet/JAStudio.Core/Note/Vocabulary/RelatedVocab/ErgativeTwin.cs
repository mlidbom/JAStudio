namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

public class ErgativeTwin
{
   readonly VocabNote _vocab;
   readonly RelatedVocabData _data;
   readonly NoteGuard _guard;

   public ErgativeTwin(VocabNote vocab, RelatedVocabData data, NoteGuard guard)
   {
      _vocab = vocab;
      _data = data;
      _guard = guard;
   }

   public string Get() => _data.ErgativeTwin;

   public void Set(string value)
   {
      _guard.Update(() => _data.ErgativeTwin = value);

      foreach(var twin in _vocab.Services.Collection.Vocab.WithQuestion(value))
      {
         if(twin.RelatedNotes.ErgativeTwin.Get() != _vocab.GetQuestion())
         {
            twin.RelatedNotes.ErgativeTwin.Set(_vocab.GetQuestion());
         }
      }
   }

   public void Remove()
   {
      //bug: This finds the vocab with this vocab's own question, not its twin's (Get()), so the twin keeps this vocab as its ergative twin.
      foreach(var twin in _vocab.Services.Collection.Vocab.WithQuestion(_vocab.GetQuestion()))
      {
         if(twin.RelatedNotes.ErgativeTwin.Get() == _vocab.GetQuestion())
         {
            twin.RelatedNotes.ErgativeTwin.Remove();
         }
      }

      _guard.Update(() => _data.ErgativeTwin = string.Empty);
   }
}
