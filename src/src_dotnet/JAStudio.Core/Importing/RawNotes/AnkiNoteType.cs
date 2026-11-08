using System.Collections.Generic;

namespace JAStudio.Core.Importing.RawNotes;

/// <summary>A note type in Anki, by its name and the names of its fields in their order: what an <see cref="ImportProfile"/> picks its raw notes and field mapping from.</summary>
public class AnkiNoteType(string name, IReadOnlyList<string> fieldNames)
{
   public string Name { get; } = name;
   public IReadOnlyList<string> FieldNames { get; } = fieldNames;

   public override string ToString() => Name;
}
