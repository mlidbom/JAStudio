using System;
using System.Collections.Generic;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackUnion(0, typeof(VocabDto))]
[MemoryPackUnion(1, typeof(KanjiDto))]
[MemoryPackUnion(2, typeof(SentenceDto))]
public abstract partial class CorpusNoteDto : IIdentifiableByGuid
{
   public Guid Id { get; init; }
   public List<string> Tags { get; init; } = [];

   protected abstract NoteId CreateTypedId();

   protected abstract void PopulateAnkiNoteFields(Dictionary<string, string> fields);

   /// Converts this typed data into AnkiNoteData
   public AnkiNoteData ToNoteData()
   {
      var fields = new Dictionary<string, string>();
      PopulateAnkiNoteFields(fields);
      fields[AnkiFieldNames.JasNoteId] = Id.ToString();
      return new AnkiNoteData(CreateTypedId(), fields, Tags);
   }
}
