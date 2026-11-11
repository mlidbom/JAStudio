using System;
using System.Collections.Generic;

namespace JAStudio.Core.Reservoir.Keeping.Format;

/// <summary>
/// A note of a kept deck, as a line of <c>notes.jsonl</c>: every field as the deck has it, its tags, and the sentence it gives, if any, shaped as<br/>
/// a sentence note's source data. The id is the one the sentence takes into the corpus.
/// </summary>
/// <param name="AnkiNoteId">The note's id in the deck's collection database.</param>
/// <param name="Fields">Every field of the note, by name, in its note type's order, as the deck has it.</param>
/// <param name="SourceData">The sentence the note gives, by its note type's mapping, or null when it gives none.</param>
public sealed record KeptNoteLine(Guid Id, long AnkiNoteId, string NoteType, IReadOnlyList<string> Tags, IReadOnlyDictionary<string, string> Fields, KeptSentenceSourceData? SourceData);
