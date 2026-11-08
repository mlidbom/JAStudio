using System.Linq;
using System.Text.Json;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Converters;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.SysUtils.Json;

namespace JAStudio.Core.Storage;

public class NoteSerializer
{
   internal static readonly JsonSerializerOptions JsonOptions = new()
                                                       {
                                                          WriteIndented = true,
                                                          PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                          Encoder = FullUnicodeJsonEncoder.Instance,
                                                          TypeInfoResolver = new NoteSerializationContext(new JsonSerializerOptions
                                                                                                          {
                                                                                                             PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                                                                             Encoder = FullUnicodeJsonEncoder.Instance,
                                                                                                          }),
                                                       };

   readonly NoteServices _noteServices;

   public NoteSerializer(NoteServices noteServices) => _noteServices = noteServices;

   internal NoteServices NoteServices => _noteServices;

   public string Serialize(KanjiNote note) => JsonSerializer.Serialize(KanjiNoteConverter.ToDto(note), JsonOptions);
   public string Serialize(VocabNote note) => JsonSerializer.Serialize(VocabNoteConverter.ToDto(note), JsonOptions);
   public string Serialize(SentenceNote note) => JsonSerializer.Serialize(SentenceNoteConverter.ToDto(note), JsonOptions);

   public string Serialize(AllNotesData data)
   {
      var container = new AllNotesDto
                      {
                         Kanji = data.Kanji.Select(KanjiNoteConverter.ToDto).ToList(),
                         Vocab = data.Vocab.Select(VocabNoteConverter.ToDto).ToList(),
                         Sentences = data.Sentences.Select(SentenceNoteConverter.ToDto).ToList(),
                      };
      return JsonSerializer.Serialize(container, JsonOptions);
   }

   public KanjiNote DeserializeKanji(string json)
   {
      var data = JsonSerializer.Deserialize<KanjiDto>(json, JsonOptions)
              ?? throw new JsonException("Failed to deserialize KanjiDto");
      return new KanjiNote(_noteServices, data);
   }

   public VocabNote DeserializeVocab(string json)
   {
      var data = JsonSerializer.Deserialize<VocabDto>(json, JsonOptions)
              ?? throw new JsonException("Failed to deserialize VocabDto");
      return new VocabNote(_noteServices, data);
   }

   public SentenceNote DeserializeSentence(string json)
   {
      var data = JsonSerializer.Deserialize<SentenceDto>(json, JsonOptions)
              ?? throw new JsonException("Failed to deserialize SentenceDto");
      return new SentenceNote(_noteServices, data);
   }

   public AllNotesData DeserializeAll(string json)
   {
      var container = JsonSerializer.Deserialize<AllNotesDto>(json, JsonOptions)
                   ?? throw new JsonException("Failed to deserialize AllNotesData");

      var kanji = container.Kanji.Select(data => new KanjiNote(_noteServices, data)).ToList();
      var vocab = container.Vocab.Select(data => new VocabNote(_noteServices, data)).ToList();
      var sentences = container.Sentences.Select(data => new SentenceNote(_noteServices, data)).ToList();

      return new AllNotesData(kanji, vocab, sentences);
   }
}
