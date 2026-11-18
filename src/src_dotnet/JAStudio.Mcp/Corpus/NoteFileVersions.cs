using System;

namespace JAStudio.Mcp.Corpus;

/// <summary>
/// Two versions of a note's file in the corpus repository, such as the file in a commit and the file in the checkout as it is now: what<br/>
/// <see cref="CorpusCheckout"/> gives for each note file that differs between two versions of the corpus.
/// </summary>
/// <param name="Path">The file's path, relative to the checkout, as git writes it: <c>vocab/81/81020a41-d636-4ee9-a158-2443d8750482.json</c>.</param>
/// <param name="NoteClass">The class of the note whose file it is, such as <see cref="JAStudio.Core.Note.Vocabulary.VocabNote"/>, as the memory image state store keeps it.</param>
/// <param name="Before">The file's contents in the version compared from. Null when that version doesn't have the file: the note was added since.</param>
/// <param name="After">The file's contents in the version compared to. Null when that version doesn't have the file: the note was deleted.</param>
public record NoteFileVersions(string Path, Type NoteClass, string? Before, string? After);
