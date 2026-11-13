using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
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
                CorpusQuestion = note.CorpusQuestion.Value,
                CorpusAnswer = note.CorpusAnswer.Value,
                Comments = note.Comments.Value,
                Reading = note.Reading.Value,
                Audio = [..note.Audio.All.Select(AudioReferenceDto.From)],
                Screenshots = [..note.Screenshots.All.Select(ImageReferenceDto.From)],
                Configuration = ToConfigurationDto(config),
                ParsingResult = ToParsingResultDto(parsingResult),
                SourceName = note.SourceName.Value,
                SourceData = note.SourceData.ToDto(),
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
