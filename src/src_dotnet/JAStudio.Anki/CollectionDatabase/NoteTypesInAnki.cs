using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Anki.CollectionDatabase;

/// <summary>The note types of an Anki collection, read from its database (<see cref="AnkiCollectionDatabase"/>).</summary>
public sealed class NoteTypesInAnki
{
   const string SelectEvery = """
                              select json_group_array(json_object('id', id,
                                                                  'name', name,
                                                                  'fieldNames', (select json_group_array(name order by ord) from fields where ntid = notetypes.id),
                                                                  'templateNames', (select json_group_array(name order by ord) from templates where ntid = notetypes.id)))
                              from notetypes
                              """;

   readonly IReadOnlyList<NoteTypeInAnki> _every;

   NoteTypesInAnki(IReadOnlyList<NoteTypeInAnki> every) => _every = every;

   /// <summary>Reads every note type of the collection from <paramref name="database"/>.</summary>
   public static NoteTypesInAnki ReadFrom(AnkiCollectionDatabase database) => new(database.Read<NoteTypeInAnki>(SelectEvery));

   /// <summary>JAStudio's own note types (<see cref="NoteTypeInAnki.IsJAStudiosOwn"/>), whose notes are the study-notes, by id.</summary>
   public IReadOnlyDictionary<long, NoteTypeInAnki> OfStudyNotesById() => _every.Where(it => it.IsJAStudiosOwn).ToDictionary(it => it.Id);

   /// <summary>Every note type but JAStudio's own, whose notes are raw notes to import, in the order of their names, ignoring case.</summary>
   public IReadOnlyList<AnkiNoteType> ForRawNotes() =>
      [.. _every.Where(it => !it.IsJAStudiosOwn).OrderBy(it => it.Name, StringComparer.CurrentCultureIgnoreCase).Select(it => new AnkiNoteType(it.Name, it.FieldNames))];

   /// <summary>The note type named <paramref name="name"/>.</summary>
   public NoteTypeInAnki Named(string name) => _every.Single(it => it.Name == name);
}
