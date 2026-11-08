using System.Text.Json.Serialization;
using JAStudio.Core.Note.CorpusData;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// Source-generated JSON serialization context for all note data types.
/// Eliminates reflection-based serialization at runtime — the compiler generates
/// optimized read/write code for each type at build time.
/// Options (encoder, indenting, naming policy) are supplied at construction time
/// via NoteSerializer so the Encoder (not settable via attribute) can be configured.
/// </summary>
[JsonSerializable(typeof(KanjiData))]
[JsonSerializable(typeof(VocabData))]
[JsonSerializable(typeof(VocabCompoundPartsSubData))]
[JsonSerializable(typeof(EnteredFormSubData))]
[JsonSerializable(typeof(VocabMatchingRulesSubData))]
[JsonSerializable(typeof(VocabRelatedSubData))]
[JsonSerializable(typeof(SentenceData))]
[JsonSerializable(typeof(SentenceConfigSubData))]
[JsonSerializable(typeof(WordExclusionSubData))]
[JsonSerializable(typeof(SentenceParsingResultSubData))]
[JsonSerializable(typeof(ParsedMatchSubData))]
[JsonSerializable(typeof(AllNotesContainer))]
[JsonSourceGenerationOptions(
   PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
   GenerationMode = JsonSourceGenerationMode.Default)]
partial class NoteSerializationContext : JsonSerializerContext;
