using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences.ParsingCases;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Storage.Dto;

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

   public class ParsingResultUpdated(ParsingResult parsingResult) : SentenceTevent, ISentenceTevent.PropertyUpdated.ParsingResult
   {
      public ParsingResult ParsingResult { get; } = parsingResult;
   }

   public class AudioUpdated(IReadOnlyList<AudioReference> audio) : SentenceTevent, ISentenceTevent.PropertyUpdated.Audio { public IReadOnlyList<AudioReference> Audio { get; } = audio; }

   public class ScreenshotsUpdated(IReadOnlyList<ImageReference> screenshots) : SentenceTevent, ISentenceTevent.PropertyUpdated.Screenshots
   {
      public IReadOnlyList<ImageReference> Screenshots { get; } = screenshots;
   }

   public class ParsingCaseUpdated(ParsingCase? parsingCase) : SentenceTevent, ISentenceTevent.PropertyUpdated.ParsingCase
   {
      public ParsingCase? ParsingCase { get; } = parsingCase;
   }

   public class Created(SentenceId sentenceId, string question) : SentenceTevent(sentenceId), ISentenceTevent.Created
   {
      public string Question { get; } = question;
   }

   /// <summary>The imported sentence's creation, with its id <paramref name="state"/>'s.</summary>
   public class Imported(SentenceDto state) : SentenceTevent(new SentenceId(state.Id)), ISentenceTevent.Imported
   {
      public SentenceDto State { get; } = state;
   }

   public class Deleted : SentenceTevent, ISentenceTevent.Deleted;

   public abstract class SourceData : SentenceTevent, ISentenceTevent.SourceData
   {
      public class SourceQuestionUpdated(string question) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Question { public string Question { get; } = question; }
      public class SourceAnswerUpdated(string answer) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Answer { public string Answer { get; } = answer; }
      public class SourceCommentsUpdated(string comments) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Comments { public string Comments { get; } = comments; }

      public class SourceExternalIdUpdated(string externalId) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.ExternalId
      {
         public string ExternalId { get; } = externalId;
      }

      public class SourceAudioUpdated(IReadOnlyList<AudioReference> audio) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Audio
      {
         public IReadOnlyList<AudioReference> Audio { get; } = audio;
      }

      public class SourceScreenshotsUpdated(IReadOnlyList<ImageReference> screenshots) : SourceData, ISentenceTevent.SourceData.PropertyUpdated.Screenshots
      {
         public IReadOnlyList<ImageReference> Screenshots { get; } = screenshots;
      }
   }

   public abstract class Configuration : SentenceTevent, ISentenceTevent.Configuration
   {
      public class HighlightedWordsUpdated(IReadOnlyList<string> highlightedWords) : Configuration, ISentenceTevent.Configuration.PropertyUpdated.HighlightedWords
      {
         public IReadOnlyList<string> HighlightedWords { get; } = highlightedWords;
      }

      public class IncorrectMatchesUpdated(IReadOnlySet<WordExclusion> incorrectMatches) : Configuration, ISentenceTevent.Configuration.PropertyUpdated.IncorrectMatches
      {
         public IReadOnlySet<WordExclusion> IncorrectMatches { get; } = incorrectMatches;
      }

      public class HiddenMatchesUpdated(IReadOnlySet<WordExclusion> hiddenMatches) : Configuration, ISentenceTevent.Configuration.PropertyUpdated.HiddenMatches
      {
         public IReadOnlySet<WordExclusion> HiddenMatches { get; } = hiddenMatches;
      }
   }
}

public class SentenceTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), ISentenceTevent<TTevent> where TTevent : INoteTevent;
