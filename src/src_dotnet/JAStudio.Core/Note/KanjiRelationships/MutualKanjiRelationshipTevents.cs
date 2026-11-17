using System;
using System.Collections.Generic;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// The tevents of one of a kanji's relationships to other kanji (<see cref="MutualKanjiRelationship"/>), such as its kanji with a similar meaning, each<br/>
/// made of the relationship's whole new list of kanji: that its kanji are now the list, and that a kanji was added or removed, by an edit of the kanji<br/>
/// or as the mirror of another kanji's edit.
/// </summary>
/// <param name="Updated">That its kanji are now the list.</param>
/// <param name="Added">That an edit of the kanji added the kanji, which a participation handler mirrors in the kanji added.</param>
/// <param name="Removed">That an edit of the kanji removed the kanji, which a participation handler mirrors in the kanji removed.</param>
/// <param name="AddedAsTheMirrorOf">That the kanji was added as the mirror of its adding this one. Nothing mirrors it back.</param>
/// <param name="RemovedAsTheMirrorOf">That the kanji was removed as the mirror of its removing this one. Nothing mirrors it back.</param>
sealed record MutualKanjiRelationshipTevents(
   Func<IReadOnlyList<string>, NoteTevent> Updated,
   Func<string, IReadOnlyList<string>, NoteTevent> Added,
   Func<string, IReadOnlyList<string>, NoteTevent> Removed,
   Func<string, IReadOnlyList<string>, NoteTevent> AddedAsTheMirrorOf,
   Func<string, IReadOnlyList<string>, NoteTevent> RemovedAsTheMirrorOf);
