using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Storage.Converters;

static class SentenceNoteConverter
{
   public static SentenceData ToCorpusData(SentenceNote note)
   {
      var config = note.Configuration.Configuration;
      var parsingResult = note.GetParsingResult();

      return new SentenceData
             {
                Id = note.GetId().Value,
                SourceQuestion = note.SourceQuestion.Value,
                UserQuestion = note.User.Question.Value,
                ActiveQuestion = note.ActiveQuestion.Value,
                SourceAnswer = note.SourceAnswer.Value,
                UserAnswer = note.User.Answer.Value,
                ActiveAnswer = note.ActiveAnswer.Value,
                SourceComments = note.SourceComments.Value,
                UserComments = note.User.Comments.Value,
                Reading = note.Reading.Value,
                ExternalId = note.ExternalId.Value,
                Audio = note.Audio.RawValue(),
                Screenshot = note.Screenshot.RawValue(),
                Configuration = ToConfigSubData(config),
                ParsingResult = ToParsingResultSubData(parsingResult),
                Tags = note.Tags.ToStringList(),
             };
   }

   static SentenceConfigSubData ToConfigSubData(SentenceConfiguration config) =>
      new()
      {
         HighlightedWords = config.HighlightedWords.ToList(),
         IncorrectMatches = config.IncorrectMatches.Get().Select(ToWordExclusionSubData).ToList(),
         HiddenMatches = config.HiddenMatches.Get().Select(ToWordExclusionSubData).ToList(),
      };

   static WordExclusionSubData ToWordExclusionSubData(WordExclusion exclusion) =>
      new()
      {
         Word = exclusion.Word,
         Index = exclusion.Index,
      };

   static SentenceParsingResultSubData? ToParsingResultSubData(ParsingResult? result)
   {
      if(result == null || string.IsNullOrEmpty(result.Sentence))
         return null;

      return new SentenceParsingResultSubData
             {
                Sentence = result.Sentence,
                ParserVersion = result.ParserVersion,
                ParsedWords = result.ParsedWords.Select(ToMatchSubData).ToList(),
             };
   }

   static ParsedMatchSubData ToMatchSubData(ParsedMatch match) =>
      new()
      {
         Variant = match.Variant,
         StartIndex = match.StartIndex,
         IsDisplayed = match.IsDisplayed,
         ParsedForm = match.ParsedForm,
         VocabId = match.VocabId?.Value,
      };
}
