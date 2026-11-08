using System.Text.Json.Serialization;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// Source-generated JSON serialization context for all note data types.
/// Eliminates reflection-based serialization at runtime — the compiler generates
/// optimized read/write code for each type at build time.
/// Options (encoder, indenting, naming policy) are supplied at construction time
/// via NoteSerializer so the Encoder (not settable via attribute) can be configured.
/// </summary>
[JsonSerializable(typeof(KanjiDto))]
[JsonSerializable(typeof(VocabDto))]
[JsonSerializable(typeof(VocabQuestionDto))]
[JsonSerializable(typeof(VocabCompoundPartsDto))]
[JsonSerializable(typeof(EnteredFormDto))]
[JsonSerializable(typeof(VocabMatchingRulesDto))]
[JsonSerializable(typeof(VocabRelatedDto))]
[JsonSerializable(typeof(SentenceDto))]
[JsonSerializable(typeof(SentenceConfigDto))]
[JsonSerializable(typeof(WordExclusionDto))]
[JsonSerializable(typeof(SentenceParsingResultDto))]
[JsonSerializable(typeof(ParsedMatchDto))]
[JsonSerializable(typeof(AllNotesDto))]
[JsonSourceGenerationOptions(
   PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
   GenerationMode = JsonSourceGenerationMode.Default)]
partial class NoteSerializationContext : JsonSerializerContext;
