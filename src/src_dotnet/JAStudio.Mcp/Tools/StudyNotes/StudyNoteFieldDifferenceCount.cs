namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>A field JAStudio writes into study-notes, by its name in Anki, and in how many of the study-notes compared it differs.</summary>
public record StudyNoteFieldDifferenceCount(string Field, int StudyNotes);
