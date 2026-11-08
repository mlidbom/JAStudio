using System.Collections.Generic;
using Compze.Contracts;

namespace JAStudio.Core.Importing.RawNotes;

/// <summary>
/// An Anki note that an <see cref="ImportProfile"/> takes into the corpus: a note of a note type of its own, such as "Immersion Kit Sentence"<br/>
/// or a subs2srs deck's, never one of JAStudio's. After an <see cref="ImportRun"/> it's deleted, or tagged <see cref="ImportedTag"/>.
/// </summary>
public class RawNote(long ankiNoteId, IReadOnlyDictionary<string, string> fieldsByName, IReadOnlyList<string> tags)
{
   /// <summary>The tag an <see cref="ImportRun"/> gives the raw notes it keeps, so that later runs leave them out. Removing it makes a note importable again.</summary>
   public const string ImportedTag = "jastudio::imported";

   readonly IReadOnlyDictionary<string, string> _fieldsByName = fieldsByName;

   public long AnkiNoteId { get; } = ankiNoteId;
   public IReadOnlyList<string> Tags { get; } = tags;

   /// <summary>The value of the field named <paramref name="ankiFieldName"/>, or empty for an empty name: a sentence field no Anki field is mapped to.</summary>
   public string Field(string ankiFieldName)
   {
      if(ankiFieldName == "") return "";
      Contract.Argument.Assert(_fieldsByName.ContainsKey(ankiFieldName), () => $"The raw note {AnkiNoteId} has no field named {ankiFieldName}.");
      return _fieldsByName[ankiFieldName];
   }

   public override string ToString() => $"Anki note {AnkiNoteId}";
}
