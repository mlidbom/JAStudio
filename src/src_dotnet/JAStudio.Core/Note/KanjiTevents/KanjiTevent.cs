using System.Collections.Generic;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.KanjiTevents;

public abstract class KanjiTevent : NoteTevent, IKanjiTevent
{
   protected KanjiTevent() {}

   KanjiTevent(KanjiId kanjiId) : base(kanjiId) {}

   public class CorpusAnswerUpdated(string corpusAnswer) : KanjiTevent, IKanjiTevent.PropertyUpdated.CorpusAnswer { public string CorpusAnswer { get; } = corpusAnswer; }
   public class MnemonicUpdated(string mnemonic) : KanjiTevent, IKanjiTevent.PropertyUpdated.Mnemonic { public string Mnemonic { get; } = mnemonic; }

   public class KanjiReferencesUpdated(string kanjiReferences) : KanjiTevent, IKanjiTevent.PropertyUpdated.KanjiReferences
   {
      public string KanjiReferences { get; } = kanjiReferences;
   }

   public class SimilarMeaningUpdated(IReadOnlyList<string> similarMeaning) : KanjiTevent, IKanjiTevent.PropertyUpdated.SimilarMeaning
   {
      public IReadOnlyList<string> SimilarMeaning { get; } = similarMeaning;
   }

   public class ConfusedWithUpdated(IReadOnlyList<string> confusedWith) : KanjiTevent, IKanjiTevent.PropertyUpdated.ConfusedWith
   {
      public IReadOnlyList<string> ConfusedWith { get; } = confusedWith;
   }

   public class Created(KanjiId kanjiId, string question) : KanjiTevent(kanjiId), IKanjiTevent.Created
   {
      public string Question { get; } = question;
   }

   public abstract class SourceData : KanjiTevent, IKanjiTevent.SourceData
   {
      public class SourceAnswerUpdated(string answer) : SourceData, IKanjiTevent.SourceData.PropertyUpdated.Answer { public string Answer { get; } = answer; }

      public class SourceMeaningMnemonicUpdated(string meaningMnemonic) : SourceData, IKanjiTevent.SourceData.PropertyUpdated.MeaningMnemonic
      {
         public string MeaningMnemonic { get; } = meaningMnemonic;
      }

      public class SourceReadingMnemonicUpdated(string readingMnemonic) : SourceData, IKanjiTevent.SourceData.PropertyUpdated.ReadingMnemonic
      {
         public string ReadingMnemonic { get; } = readingMnemonic;
      }

      public class SourceMeaningInfoUpdated(string meaningInfo) : SourceData, IKanjiTevent.SourceData.PropertyUpdated.MeaningInfo
      {
         public string MeaningInfo { get; } = meaningInfo;
      }

      public class SourceReadingInfoUpdated(string readingInfo) : SourceData, IKanjiTevent.SourceData.PropertyUpdated.ReadingInfo
      {
         public string ReadingInfo { get; } = readingInfo;
      }
   }
}

public class KanjiTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), IKanjiTevent<TTevent> where TTevent : INoteTevent;
