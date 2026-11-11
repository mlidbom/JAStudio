using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// A deck as a source of reservoir sentences: the notes of an Anki package's database, of every note type whose fields hold sentences, the<br/>
/// fields recognized by <see cref="DeckFieldRoles"/> from a sample of each note type's notes.
/// </summary>
public sealed class AnkiDeck
{
   const int NotesSampledToRecognizeTheFields = 200;

   readonly AnkiPackageDatabase _database;

   public AnkiDeck(AnkiPackageDatabase database)
   {
      _database = database;
      FieldRoles = [.. database.NoteTypes.Select(noteType => DeckFieldRoles.RecognizedIn(noteType, [.. database.NotesOf(noteType).Take(NotesSampledToRecognizeTheFields)]))];
   }

   /// <summary>The roles recognized in the fields of each of the deck's note types.</summary>
   public IReadOnlyList<DeckFieldRoles> FieldRoles { get; }

   /// <summary>Every sentence the deck's notes hold, read as they're enumerated.</summary>
   public IEnumerable<SourceSentence> Sentences() =>
      FieldRoles.SelectMany(roles => _database.NotesOf(roles.NoteType).Select(roles.SentenceOf)).OfType<SourceSentence>();
}
