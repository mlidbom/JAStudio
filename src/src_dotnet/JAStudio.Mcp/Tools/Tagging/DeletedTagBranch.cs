namespace JAStudio.Mcp.Tools.Tagging;

/// <summary>What <see cref="TagTools.DeleteATagBranch"/> deleted: the branch, and how many notes had a tag in it.</summary>
/// <param name="Branch">The path of the branch deleted, such as <c>tmp</c> or <c>anki::tmp</c>.</param>
/// <param name="NotesThatHadATagInIt">How many of JAStudio's notes had a tag in the branch, or, for a branch below <c>anki::</c>, a study-note with one.</param>
/// <param name="AnkiNotesThatHadATagInIt">
/// For a branch below <c>anki::</c>, how many Anki notes of any note type had a tag in it: those it was deleted from. Null for a branch of the<br/>
/// notes' own tags.
/// </param>
public record DeletedTagBranch(string Branch, int NotesThatHadATagInIt, int? AnkiNotesThatHadATagInIt);
