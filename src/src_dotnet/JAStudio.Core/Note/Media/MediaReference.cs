namespace JAStudio.Core.Note.Media;

/// <summary>
/// A note's reference to one of its media files, an <see cref="AudioReference"/> or an <see cref="ImageReference"/>: the file's name, by which the media<br/>
/// layer and Anki's media folder both find it, such as <c>文句_b.mp3</c>, and the facts known about the file.
/// </summary>
/// <remarks>
/// The facts are on the note's reference rather than with the file in the media layer, so that a note's file holds everything about it and its diffs<br/>
/// stay readable (Magnus, 2026-11-10). A file several notes share, such as a Core 2000 image on a vocab and on its example sentence, carries the same<br/>
/// facts on each. A fact that isn't known is left out. See <c>dev_docs/corpus-data-model.md</c>, Media.
/// </remarks>
public abstract record MediaReference(string FileName)
{
   /// <summary>
   /// Whether we own the file's copyright, or are at least entirely free to use it however we like: true only when that's proven, as for the TTS audio<br/>
   /// we generated. A file is someone else's unless we know for sure it's ours. See <c>corpus-data-model.md</c>, "Whether a file is ours".
   /// </summary>
   public bool IsOurs { get; init; }
}
