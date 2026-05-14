using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.Serialization;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

[MemoryPackable]
public partial class SentenceData : CorpusObjectData
{
   static readonly ISentenceConfigurationSerializer ConfigSerializer = ISentenceConfigurationSerializer.Instance;
   static readonly ParsingResultSerializer ParsingSerializer = new();

   public string SourceQuestion { get; init; } = string.Empty;
   public string UserQuestion { get; init; } = string.Empty;
   public string ActiveQuestion { get; init; } = string.Empty;
   public string SourceAnswer { get; init; } = string.Empty;
   public string UserAnswer { get; init; } = string.Empty;
   public string ActiveAnswer { get; init; } = string.Empty;
   public string SourceComments { get; init; } = string.Empty;
   public string UserComments { get; init; } = string.Empty;
   public string Reading { get; init; } = string.Empty;
   public string ExternalId { get; init; } = string.Empty;
   public string Audio { get; init; } = string.Empty;
   public string Screenshot { get; init; } = string.Empty;
   public SentenceConfigSubData Configuration { get; init; } = new();
   public SentenceParsingResultSubData? ParsingResult { get; init; }

   protected override NoteId CreateTypedId() => new SentenceId(Id);

   protected override void PopulateFields(Dictionary<string, string> fields)
   {
      fields[SentenceNoteFields.SourceQuestion] = SourceQuestion;
      fields[SentenceNoteFields.UserQuestion] = UserQuestion;
      fields[SentenceNoteFields.ActiveQuestion] = ActiveQuestion;
      fields[SentenceNoteFields.SourceAnswer] = SourceAnswer;
      fields[SentenceNoteFields.UserAnswer] = UserAnswer;
      fields[SentenceNoteFields.ActiveAnswer] = ActiveAnswer;
      fields[SentenceNoteFields.SourceComments] = SourceComments;
      fields[SentenceNoteFields.UserComments] = UserComments;
      fields[SentenceNoteFields.Reading] = Reading;
      fields[SentenceNoteFields.Id] = ExternalId;
      fields[SentenceNoteFields.Audio] = Audio;
      fields[SentenceNoteFields.Screenshot] = Screenshot;
      fields[SentenceNoteFields.Configuration] = SerializeConfiguration();
      fields[SentenceNoteFields.ParsingResult] = SerializeParsingResult();
   }

   string SerializeConfiguration()
   {
      var config = new SentenceConfiguration(
         Configuration.HighlightedWords.ToList(),
         new WordExclusionSet(() => {}, Configuration.IncorrectMatches.Select(FromExclusionSubData).ToList()),
         new WordExclusionSet(() => {}, Configuration.HiddenMatches.Select(FromExclusionSubData).ToList()));
      return ConfigSerializer.Serialize(config);
   }

   string SerializeParsingResult()
   {
      if(ParsingResult == null) return string.Empty;

      var result = new ParsingResult(
         ParsingResult.ParsedWords.Select(FromParsedMatchSubData).ToList(),
         ParsingResult.Sentence,
         ParsingResult.ParserVersion);
      return ParsingSerializer.Serialize(result);
   }

   static WordExclusion FromExclusionSubData(WordExclusionSubData d) =>
      d.Index == -1 ? WordExclusion.Global(d.Word) : WordExclusion.AtIndex(d.Word, d.Index);

   static ParsedMatch FromParsedMatchSubData(ParsedMatchSubData d) =>
      new(d.Variant, d.StartIndex, d.IsDisplayed, d.ParsedForm, d.VocabId.HasValue ? new VocabId(d.VocabId.Value) : null);

   /// Creates SentenceData from raw Anki NoteData (for NoteCache and Python interop paths).
   public static SentenceData FromAnkiNoteData(NoteData data) => FromAnki(new AnkiSentenceNote(data));

   public static SentenceData FromAnki(AnkiSentenceNote anki) =>
      new()
      {
         Id = (anki.Id ?? SentenceId.New()).Value,
         Tags = [..anki.Tags],
         SourceQuestion = anki.SourceQuestion,
         SourceAnswer = anki.SourceAnswer,
         SourceComments = anki.SourceComments,
         Reading = anki.Reading,
         ExternalId = anki.ExternalId,
         Audio = anki.Audio,
         Screenshot = anki.Screenshot,
      };

   /// Merges Anki-owned fields (every field exposed by <see cref="AnkiSentenceNote"/>, plus tags) into existing data, preserving all fields Anki does not store.
   public SentenceData MergeAnkiData(NoteData ankiData)
   {
      var anki = new AnkiSentenceNote(ankiData);
      return new SentenceData
             {
                Id = Id,
                Tags = [..anki.Tags],
                SourceQuestion = anki.SourceQuestion,
                UserQuestion = UserQuestion,
                ActiveQuestion = ActiveQuestion,
                SourceAnswer = anki.SourceAnswer,
                UserAnswer = UserAnswer,
                ActiveAnswer = ActiveAnswer,
                SourceComments = anki.SourceComments,
                UserComments = UserComments,
                Reading = anki.Reading,
                ExternalId = anki.ExternalId,
                Audio = anki.Audio,
                Screenshot = anki.Screenshot,
                Configuration = Configuration,
                ParsingResult = ParsingResult,
             };
   }

   public static ParsingResult CreateParsingResult(SentenceParsingResultSubData? data)
   {
      if(data == null) return new ParsingResult([], "", "");
      return new ParsingResult(
         data.ParsedWords.Select(FromParsedMatchSubData).ToList(),
         data.Sentence,
         data.ParserVersion);
   }

   public static SentenceConfiguration CreateConfiguration(SentenceConfigSubData? data, Action saveCallback)
   {
      if(data == null)
         return new SentenceConfiguration(
            [],
            WordExclusionSet.Empty(saveCallback),
            WordExclusionSet.Empty(saveCallback));
      return new SentenceConfiguration(
         data.HighlightedWords.ToList(),
         new WordExclusionSet(saveCallback, data.IncorrectMatches.Select(FromExclusionSubData).ToList()),
         new WordExclusionSet(saveCallback, data.HiddenMatches.Select(FromExclusionSubData).ToList()));
   }
}

[MemoryPackable]
public partial class SentenceConfigSubData
{
   public List<string> HighlightedWords { get; init; } = [];
   public List<WordExclusionSubData> IncorrectMatches { get; init; } = [];
   public List<WordExclusionSubData> HiddenMatches { get; init; } = [];
}

[MemoryPackable]
public partial class WordExclusionSubData
{
   public string Word { get; init; } = string.Empty;
   public int Index { get; init; } = -1;
}

[MemoryPackable]
public partial class SentenceParsingResultSubData
{
   public string Sentence { get; init; } = string.Empty;
   public string ParserVersion { get; init; } = string.Empty;
   public List<ParsedMatchSubData> ParsedWords { get; init; } = [];
}

[MemoryPackable]
public partial class ParsedMatchSubData
{
   public string Variant { get; init; } = string.Empty;
   public int StartIndex { get; init; }
   public bool IsDisplayed { get; init; }
   public string ParsedForm { get; init; } = string.Empty;
   public Guid? VocabId { get; init; }
}
