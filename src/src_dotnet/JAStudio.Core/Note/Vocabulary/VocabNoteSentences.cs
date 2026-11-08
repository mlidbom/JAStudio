using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Note.Vocabulary;

public class SentenceCounts
{
   readonly Func<VocabNoteSentences> _parent;
   int _studyingReading;
   int _studyingListening;
   int _total;
   DateTime _lastUpdateTime;
   int _cacheSeconds;

   public SentenceCounts(Func<VocabNoteSentences> parent)
   {
      _parent = parent;
      _studyingReading = 0;
      _studyingListening = 0;
      _total = 0;
      _lastUpdateTime = DateTime.MinValue;
      _cacheSeconds = 0;
   }

   public int Total => UpToDateSelf()._total;
   public int StudyingReading => UpToDateSelf()._studyingReading;
   public int StudyingListening => UpToDateSelf()._studyingListening;

   SentenceCounts UpToDateSelf()
   {
      int HowLongToCacheFor()
      {
         if(_total < 10) return 60;
         if(_total < 100) return 600;
         return 6000;
      }

      int GetStudyingSentenceCount(string card = "")
      {
         return _parent().All().Count(it => it.IsStudying(card));
      }

      if((DateTime.Now - _lastUpdateTime).TotalSeconds > _cacheSeconds)
      {
         _lastUpdateTime = DateTime.Now;
         var sentences = _parent().WithOwnedForm();
         _studyingReading = GetStudyingSentenceCount(CardTypes.Reading);
         _studyingListening = GetStudyingSentenceCount(CardTypes.Listening);
         _total = sentences.Count;
         _cacheSeconds = HowLongToCacheFor();
      }

      return this;
   }
}

public class VocabNoteSentences
{
   SentenceCounts? _counts;

   public VocabNoteSentences(VocabNote vocab) => Vocab = vocab;

   VocabNote Vocab { get; }

   public SentenceCounts Counts()
   {
      _counts ??= new SentenceCounts(() => this);
      return _counts;
   }

   public List<SentenceNote> Studying()
   {
      return All().Where(it => it.IsStudying()).ToList();
   }

   public List<SentenceNote> All() => Vocab.Services.Collection.Sentences.WithVocab(Vocab);

   public List<SentenceNote> InvalidIn() => Vocab.Services.Collection.Sentences.WithVocabMarkedInvalid(Vocab);

   public List<SentenceNote> WithOwnedForm() => Vocab.Services.Collection.Sentences.WithVocabOwnedForm(Vocab);

   public List<SentenceNote> WithPrimaryForm() => Vocab.Services.Collection.Sentences.WithForm(Vocab.GetQuestion());

   public List<SentenceNote> Highlighted() => Vocab.Services.Collection.Sentences.WithHighlightedVocab(Vocab.GetQuestion());
}
