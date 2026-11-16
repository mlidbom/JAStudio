using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Copies;

/// <summary>
/// What <see cref="CopyTools.FindWhatCopiesOfTheNotesLose"/> reports: how many notes it checked, how many of their copies differ from them, how long<br/>
/// that took, and each field that differs.
/// </summary>
public record WhatCopiesLose(int NotesChecked, int NotesWhoseCopyDiffers, double SecondsTaken, IReadOnlyList<AFieldACopyLoses> Fields);

/// <summary>A field in which a copy of a note differs from the note, by its path, such as <c>VocabNote.Forms._forms[]</c>: in how many notes, with an example.</summary>
public record AFieldACopyLoses(string Field, int Notes, string AnExample);
