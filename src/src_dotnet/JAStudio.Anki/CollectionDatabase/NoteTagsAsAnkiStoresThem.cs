using System;
using System.Collections.Generic;

namespace JAStudio.Anki.CollectionDatabase;

/// <summary>
/// A note's tags as an Anki collection's database stores them, in the <c>tags</c> column of its <c>notes</c> table: one text of the tags'<br/>
/// names, separated by spaces, which Anki keeps out of a tag's name.
/// </summary>
public sealed class NoteTagsAsAnkiStoresThem
{
   const char Separator = ' ';

   /// <param name="tagsColumn">The text of the note's <c>tags</c> column.</param>
   public NoteTagsAsAnkiStoresThem(string tagsColumn) => Names = tagsColumn.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

   /// <summary>The names of the tags, such as <c>marked</c> and <c>source::wani::level::04</c>.</summary>
   public IReadOnlyList<string> Names { get; }
}
