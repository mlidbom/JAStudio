using System;
using System.Collections.Generic;
using System.Linq;
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

   /// <summary>
   /// The name of the work or deck the sentence came from, as its source gave it, such as <c>Shirokuma Cafe</c>. Empty when the source gave none.<br/>
   /// JAStudio only carries it: nothing interprets it.
   /// </summary>
   /// <remarks>
   /// Declared last on purpose: the corpus's snapshot cache (MemoryPack) reads members by their position, and old snapshots read correctly only while<br/>
   /// new members come after the existing ones.
   /// </remarks>
   public string SourceName { get; init; } = string.Empty;

   protected override NoteId CreateTypedId() => new SentenceId(Id);

   protected override void PopulateAnkiNoteFields(Dictionary<string, string> fields)
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
