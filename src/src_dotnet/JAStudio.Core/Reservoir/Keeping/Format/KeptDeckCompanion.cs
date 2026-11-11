using System.Collections.Generic;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// The companion of a kept deck's collection database, <c>deck.json</c> beside it: the package it came from, how its note types map to<br/>
/// sentences, and its media files, each by the hash the <see cref="MediaStore"/> keeps it under.
/// </summary>
/// <param name="CollectionDatabase">The file name of the deck's collection database beside it, as in the package, such as <c>collection.anki2</c>.</param>
public sealed record KeptDeckCompanion(
   int FormatVersion,
   KeptPackage Package,
   string CollectionDatabase,
   IReadOnlyList<KeptNoteTypeMapping> NoteTypes,
   IReadOnlyList<KeptMediaFile> Media);
