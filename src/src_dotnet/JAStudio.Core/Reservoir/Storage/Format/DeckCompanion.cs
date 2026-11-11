using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Reservoir.Storage.Media;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.Core.Reservoir.Storage.Format;

/// <summary>
/// The companion of a stored deck's collection database, <c>deck.json</c> beside it: the package it came from, how its note types map to<br/>
/// sentences, and its media files, each by the hash the <see cref="MediaStore"/> stores it under.
/// </summary>
/// <param name="FormatVersion">The version of the format the deck is stored in (<see cref="Manifest.CurrentFormatVersion"/>).</param>
/// <param name="CollectionDatabase">The file name of the deck's collection database beside it, as in the package, such as <c>collection.anki2</c>.</param>
/// <param name="NoteTypes">How each of the deck's note types maps to a sentence.</param>
/// <param name="Media">Every media file of the deck's package.</param>
public sealed record DeckCompanion(
   int FormatVersion,
   Package Package,
   string CollectionDatabase,
   IReadOnlyList<NoteTypeMapping> NoteTypes,
   IReadOnlyList<MediaFile> Media)
{
   /// <summary>
   /// The companion of the deck in <paramref name="package"/>, stored as <paramref name="collectionDatabase"/>, its note types mapped as<br/>
   /// <paramref name="deck"/> recognized their fields, and its media stored as <paramref name="media"/>.
   /// </summary>
   public static DeckCompanion Of(Package package, FileInfo collectionDatabase, AnkiDeck deck, StoredDeckMedia media) =>
      new(Manifest.CurrentFormatVersion,
          package,
          collectionDatabase.Name,
          [.. deck.FieldRoles.Select(NoteTypeMapping.Of)],
          [.. media.Files.Select(MediaFile.Of)]);
}
