using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.Anki.CollectionDatabase;

/// <summary>
/// A note type as an Anki collection's database has it: its id, its name, the names of its fields, in the order a note holds their values<br/>
/// (<see cref="NoteFieldsAsAnkiStoresThem"/>), and the names of its card templates, in the order of the cards' ordinals.
/// </summary>
public sealed record NoteTypeInAnki(long Id, string Name, IReadOnlyList<string> FieldNames, IReadOnlyList<string> TemplateNames)
{
   /// <summary>Whether it's one of JAStudio's own note types (<see cref="NoteTypes"/>), whose notes are the study-notes.</summary>
   public bool IsJAStudiosOwn => NoteTypes.All.Contains(Name);

   /// <summary>The name of the template of the card with <paramref name="ordinal"/>, such as <c>Reading</c>.</summary>
   public string TemplateNameOfTheCardWith(int ordinal) => TemplateNames[ordinal];

   /// <summary>The fields of a note of this note type by name, from <paramref name="fields"/>, the values of the note's fields.</summary>
   public IReadOnlyDictionary<string, string> FieldsByNameIn(NoteFieldsAsAnkiStoresThem fields) => FieldNames.Index().ToDictionary(it => it.Item, it => fields.ValueAt(it.Index));
}
