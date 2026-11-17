using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Sentences.Tevents;

public abstract class SentenceTevent : NoteTevent, ISentenceTevent
{
   protected SentenceTevent() {}

   SentenceTevent(SentenceId sentenceId) : base(sentenceId) {}

   public class CorpusQuestionUpdated(string corpusQuestion) : SentenceTevent, ISentenceTevent.PropertyUpdated.CorpusQuestion
   {
      public string CorpusQuestion { get; } = corpusQuestion;
   }

   public class CorpusAnswerUpdated(string corpusAnswer) : SentenceTevent, ISentenceTevent.PropertyUpdated.CorpusAnswer { public string CorpusAnswer { get; } = corpusAnswer; }
   public class CommentsUpdated(string comments) : SentenceTevent, ISentenceTevent.PropertyUpdated.Comments { public string Comments { get; } = comments; }
   public class SourceNameUpdated(string sourceName) : SentenceTevent, ISentenceTevent.PropertyUpdated.SourceName { public string SourceName { get; } = sourceName; }
   public class ReadingUpdated(string reading) : SentenceTevent, ISentenceTevent.PropertyUpdated.Reading { public string Reading { get; } = reading; }

   public class Created(SentenceId sentenceId, string question) : SentenceTevent(sentenceId), ISentenceTevent.Created
   {
      public string Question { get; } = question;
   }

   public abstract class SourceData : SentenceTevent, ISentenceTevent.SourceData
   {
      public class SourceQuestionUpdated(string question) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Question { public string Question { get; } = question; }
      public class SourceAnswerUpdated(string answer) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Answer { public string Answer { get; } = answer; }
      public class SourceCommentsUpdated(string comments) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Comments { public string Comments { get; } = comments; }

      public class SourceExternalIdUpdated(string externalId) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.ExternalId
      {
         public string ExternalId { get; } = externalId;
      }
   }
}

public class SentenceTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), ISentenceTevent<TTevent> where TTevent : INoteTevent;
