using System;
using System.Collections.Generic;
using JAStudio.Core.Storage;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

[MemoryPackable(GenerateType.NoGenerate)]
[MemoryPackUnion(0, typeof(VocabData))]
[MemoryPackUnion(1, typeof(KanjiData))]
[MemoryPackUnion(2, typeof(SentenceData))]
public abstract partial class CorpusObjectData : IIdentifiableByGuid
{
   public Guid Id { get; init; }
   public List<string> Tags { get; init; } = [];

   protected abstract NoteId CreateTypedId();

   protected abstract void PopulateFields(Dictionary<string, string> fields);

   /// Converts this typed data into AnkiNoteData
   public AnkiNoteData ToNoteData()
   {
      var fields = new Dictionary<string, string>();
      PopulateFields(fields);
      fields[MyNoteFields.JasNoteId] = Id.ToString();
      return new AnkiNoteData(CreateTypedId(), fields, Tags);
   }
}
