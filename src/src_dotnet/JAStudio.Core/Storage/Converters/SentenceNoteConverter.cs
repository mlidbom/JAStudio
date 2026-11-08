using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Storage.Converters;

static class SentenceNoteConverter
{
   public static SentenceDto ToDto(SentenceNote note)
   {
      var config = note.Configuration.Configuration;
      var parsingResult = note.GetParsingResult();

      return new SentenceDto
             {
                Id = note.GetId().Value,
                SourceQuestion = note.SourceData.Question.Value,
                UserQuestion = note.CorpusQuestion.Value,
                ActiveQuestion = note.GetQuestion(),
                SourceAnswer = note.SourceData.Answer.Value,
                UserAnswer = note.CorpusAnswer.Value,
                ActiveAnswer = note.GetAnswer(),
                SourceComments = note.SourceData.Comments.Value,
                UserComments = note.Comments.Value,
                Reading = note.Reading.Value,
                ExternalId = note.SourceData.ExternalId.Value,
                Audio = note.Audio.RawValue(),
                Screenshot = note.Screenshot.RawValue(),
                Configuration = ToConfigurationDto(config),
                ParsingResult = ToParsingResultDto(parsingResult),
                SourceName = note.SourceName.Value,
                Tags = note.Tags.ToStringList(),
             };
   }

   static SentenceConfigDto ToConfigurationDto(SentenceConfiguration config) =>
      new()
      {
         HighlightedWords = config.HighlightedWords.ToList(),
         IncorrectMatches = config.IncorrectMatches.Get().Select(ToWordExclusionDto).ToList(),
         HiddenMatches = config.HiddenMatches.Get().Select(ToWordExclusionDto).ToList(),
      };

   static WordExclusionDto ToWordExclusionDto(WordExclusion exclusion) =>
      new()
      {
         Word = exclusion.Word,
         Index = exclusion.Index,
      };

   static SentenceParsingResultDto? ToParsingResultDto(ParsingResult? result)
   {
      if(result == null || string.IsNullOrEmpty(result.Sentence))
         return null;

      return new SentenceParsingResultDto
             {
                Sentence = result.Sentence,
                ParserVersion = result.ParserVersion,
                ParsedWords = result.ParsedWords.Select(ToParsedMatchDto).ToList(),
             };
   }

   static ParsedMatchDto ToParsedMatchDto(ParsedMatch match) =>
      new()
      {
         Variant = match.Variant,
         StartIndex = match.StartIndex,
         IsDisplayed = match.IsDisplayed,
         ParsedForm = match.ParsedForm,
         VocabId = match.VocabId?.Value,
      };
}
