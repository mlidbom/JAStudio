using System;
using System.Collections.Generic;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

/// <summary>
/// The tevents of one relationship that goes both ways (<see cref="MutualWordRelationship"/>), such as the synonyms, each made of the relationship's whole<br/>
/// new set of words: that its words are now the set, and that a word was added or removed, by an edit of the vocab or as the mirror of another<br/>
/// vocab's edit.
/// </summary>
/// <param name="Updated">That its words are now the set, as a rename of a word makes them.</param>
/// <param name="Added">That an edit of the vocab added the word, which a participation handler mirrors in the vocab the word refers to.</param>
/// <param name="Removed">That an edit of the vocab removed the word, which a participation handler mirrors in the vocab the word refers to.</param>
/// <param name="AddedAsTheMirrorOf">That the word, the name of another vocab, was added as the mirror of that vocab's adding this one. Nothing mirrors it back.</param>
/// <param name="RemovedAsTheMirrorOf">That the word, the name of another vocab, was removed as the mirror of that vocab's removing this one. Nothing mirrors it back.</param>
sealed record MutualWordRelationshipTevents(
   Func<IReadOnlySet<WordReference>, NoteTevent> Updated,
   Func<WordReference, IReadOnlySet<WordReference>, NoteTevent> Added,
   Func<WordReference, IReadOnlySet<WordReference>, NoteTevent> Removed,
   Func<WordReference, IReadOnlySet<WordReference>, NoteTevent> AddedAsTheMirrorOf,
   Func<WordReference, IReadOnlySet<WordReference>, NoteTevent> RemovedAsTheMirrorOf);
