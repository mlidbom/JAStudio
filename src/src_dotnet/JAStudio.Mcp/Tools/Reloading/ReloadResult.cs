namespace JAStudio.Mcp.Tools.Reloading;

/// <summary>What <see cref="ReloadTools.ReloadTheCollection"/> reports: how long the reload took, and the notes it found changed on disk.</summary>
public record ReloadResult(double SecondsTaken, NotesChangedOnDisk NotesChangedOnDisk);
