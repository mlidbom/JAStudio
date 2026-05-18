using System.Collections.Generic;
using System.Linq;
using Compze.Internals.SystemCE.ReactiveCE;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

public class PerfectSynonyms
{
   readonly VocabNote _vocab;
   readonly RelatedVocabData _data;
   readonly NoteGuard _guard;

   public PerfectSynonyms(VocabNote vocab, RelatedVocabData data, NoteGuard guard)
   {
      _vocab = vocab;
      _data = data;
      _guard = guard;
      vocab.User.Answer.Subscribe(_ => PushAnswerToOtherSynonyms());
   }

   public List<VocabNote> Notes() => _vocab.Services.Collection.Vocab.WithAnyDisambiguationNameIn(_data.PerfectSynonyms).ToList();

   public void PushAnswerToOtherSynonyms()
   {
      foreach(var synonym in Notes())
      {
         synonym.User.Answer.Set(_vocab.GetAnswer());
      }
   }

   public HashSet<string> Get() => _data.PerfectSynonyms;

   void RemoveInternal(string synonym) => _guard.Update(() =>
   {
      _data.PerfectSynonyms.Remove(synonym);
      _vocab.RelatedNotes.Synonyms.Add(synonym);
   });

   void AddInternal(string synonym) => _guard.Update(() =>
   {
      if(synonym == _vocab.GetQuestion()) return;
      _data.PerfectSynonyms.Add(synonym);
      _vocab.RelatedNotes.Synonyms.Add(synonym);
   });

   HashSet<VocabNote> ResolveWholeWeb()
   {
      var found = new HashSet<VocabNote>();

      void RecurseInto(VocabNote syn)
      {
         found.Add(syn);
         foreach(var related in syn.RelatedNotes.PerfectSynonyms.Notes())
         {
            if(!found.Contains(related))
            {
               RecurseInto(related);
            }
         }
      }

      RecurseInto(_vocab);
      foreach(var synonym in Notes())
      {
         RecurseInto(synonym);
      }

      return found;
   }

   void EnsureAllPerfectSynonymsAreConnected()
   {
      var wholeWeb = ResolveWholeWeb();
      var allQuestions = wholeWeb.Select(syn => syn.GetQuestion()).ToHashSet();

      foreach(var synonym in wholeWeb)
      {
         foreach(var question in allQuestions)
         {
            synonym.RelatedNotes.PerfectSynonyms.AddInternal(question);
         }
      }
   }

   public void AddOverwritingTheAnswerOfTheAddedSynonym(string addedQuestion)
   {
      if(addedQuestion == _vocab.GetQuestion()) return;
      AddInternal(addedQuestion);
      EnsureAllPerfectSynonymsAreConnected();
      PushAnswerToOtherSynonyms();
   }

   public void Remove(string synonymToRemove)
   {
      foreach(var toRemove in _vocab.Services.Collection.Vocab.WithQuestion(synonymToRemove))
      {
         toRemove.RelatedNotes.PerfectSynonyms._data.PerfectSynonyms.Clear();
      }

      foreach(var syn in ResolveWholeWeb())
      {
         syn.RelatedNotes.PerfectSynonyms.RemoveInternal(synonymToRemove);
      }
   }

   public override string ToString() => string.Join(", ", _data.PerfectSynonyms);
}
