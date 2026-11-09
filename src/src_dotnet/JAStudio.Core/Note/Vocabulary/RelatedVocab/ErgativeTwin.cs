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
      //bug: The twin this vocab had before keeps this vocab as its ergative twin. Remove() unlinks it, but writing the editor's
      //   field, even to nothing, calls only this.
      _guard.Update(() => _data.ErgativeTwin = value);

      foreach(var twin in _vocab.Services.Collection.Vocab.WithQuestion(value))
      {
         if(twin.RelatedNotes.ErgativeTwin.Get() != _vocab.GetQuestion())
         {
            twin.RelatedNotes.ErgativeTwin.Set(_vocab.GetQuestion());
         }
      }
   }

   /// <summary>Removes this vocab's ergative twin, and this vocab as the twin's ergative twin.</summary>
   /// <remarks>This vocab's is cleared before the twin's, or the twin's <see cref="Remove"/> would find this vocab still its twin, and remove again.</remarks>
   public void Remove()
   {
      var twinsQuestion = Get();
      _guard.Update(() => _data.ErgativeTwin = string.Empty);

      foreach(var twin in _vocab.Services.Collection.Vocab.WithQuestion(twinsQuestion))
      {
         if(twin.RelatedNotes.ErgativeTwin.Get() == _vocab.GetQuestion())
         {
            twin.RelatedNotes.ErgativeTwin.Remove();
         }
      }
   }
}
