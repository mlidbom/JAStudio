using Compze.Teventive.TaggregateStores;

namespace JAStudio.Core.Note;

/// <summary>
/// How code that keeps a note across edits keeps it: as the note's reference, <c>note.Reference()</c>, which gives the note as it is now on each<br/>
/// read, never the note as it was when the code got it.
/// </summary>
/// <remarks>
/// Once a taggregate store keeps the notes, a committed note never changes: an edit commits a changed copy in its place. A note kept across edits<br/>
/// then goes stale, and a reference doesn't. The design is in dev_docs/taggregates/shared/how-jastudio-edits-a-note.md.
/// </remarks>
public static class ReferringToANote
{
   /// <summary>The reference of <paramref name="note"/>, a note of the collection.</summary>
   /// <remarks>
   /// A classic extension method, not a C# 14 extension block: the Python stub generator writes an extension block's compiler-generated types, whose<br/>
   /// names aren't Python, into the stub of the whole namespace.
   /// </remarks>
   public static ITaggregateReference<TNote> Reference<TNote>(this TNote note) where TNote : JPNote => new NoteReference<TNote>(note);
}
