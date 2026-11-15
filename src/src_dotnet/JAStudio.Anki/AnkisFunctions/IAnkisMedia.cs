namespace JAStudio.Anki.AnkisFunctions;

/// <summary>
/// The media of the collection Anki has open, the files in its media folder: one group of the functions JAStudio calls in Anki (see<br/>
/// <see cref="IAnkisNotes"/> for the groups).
/// </summary>
public interface IAnkisMedia
{
   /// <summary>
   /// Copies the file at <paramref name="filePath"/> into Anki's media folder, so that Anki plays it and its media sync takes it to other devices,<br/>
   /// and returns the name Anki keeps it under. That's the file's own name, unless Anki already has a different file by that name. The add-on's<br/>
   /// <c>media_add_file</c>.
   /// </summary>
   string AddFile(string filePath);

   /// <summary>The path of Anki's media folder. The add-on's <c>anki_media_dir</c>.</summary>
   string Folder { get; }
}
