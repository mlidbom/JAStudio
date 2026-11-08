using System.Collections.Generic;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;

namespace JAStudio.Core.Importing;

/// <summary>
/// A saved description of one kind of import, such as Immersion Kit's sentence cards: which <see cref="RawNote"/>s in Anki it takes,<br/>
/// how their fields become a corpus sentence's, which tags the sentences get, and where their media goes in the media layer.<br/>
/// Applying it is an <see cref="ImportRun"/>, prepared by <see cref="Importer"/>. See <c>dev_docs/import-profiles.md</c>.
/// </summary>
/// <remarks>
/// "Profile" isn't an Anki profile: the separate Anki profile imports happen in is "the Anki profile for importing".<br/>
/// An import profile creates sentences only. Vocab would come with a source of vocab to import.
/// </remarks>
public class ImportProfile
{
   public string Name { get; set; } = "";

   /// <summary>The note type of the raw notes the profile takes, such as "Immersion Kit Sentence". Never one of JAStudio's own <see cref="NoteTypes"/>.</summary>
   public string AnkiNoteType { get; set; } = "";

   /// <summary>An Anki search that narrows the selection within <see cref="AnkiNoteType"/>, such as <c>deck:Natsume</c>, or empty to take every note of the type.</summary>
   public string AnkiSearch { get; set; } = "";

   public SentenceFieldMapping FieldMapping { get; set; } = new();

   /// <summary>Tags every imported sentence gets, such as <c>source::immersion_kit</c>, besides the tags the raw note already has.</summary>
   public List<string> TagsToAdd { get; set; } = [];

   /// <summary>The folder of the media layer, relative to its root, that the audio of <see cref="SentenceFieldMapping.Audio"/> is stored in.</summary>
   public string AudioFolder { get; set; } = "";

   /// <summary>The folder of the media layer, relative to its root, that the images of <see cref="SentenceFieldMapping.Screenshot"/> are stored in.</summary>
   public string ScreenshotFolder { get; set; } = "";

   /// <summary>Whether a run deletes the raw notes from Anki afterwards. Otherwise it tags them <see cref="RawNote.ImportedTag"/>.</summary>
   public bool DeletesTheRawNotesAfterwards { get; set; }

   /// <summary>Whether each imported sentence gets a note in the open Anki profile to study. Otherwise it's added to the corpus only.</summary>
   public bool CreatesAnkiNotesToStudy { get; set; }

   /// <summary>
   /// Whether the profile runs on its own, through <see cref="AutomaticImportRuns"/>, when Anki signals that raw notes of its <see cref="AnkiNoteType"/> may have arrived:<br/>
   /// when Anki finished importing a file, such as an .apkg, or a note of the type was added, as AnkiConnect adds them. Otherwise it runs only by hand.
   /// </summary>
   public bool RunsAutomatically { get; set; }

   /// <summary>A copy that later changes to this profile don't reach, such as the one an <see cref="ImportRun"/> runs, so that it does what it was checked to do.</summary>
   public ImportProfile Copy() => new()
   {
      Name = Name,
      AnkiNoteType = AnkiNoteType,
      AnkiSearch = AnkiSearch,
      FieldMapping = FieldMapping.Copy(),
      TagsToAdd = [..TagsToAdd],
      AudioFolder = AudioFolder,
      ScreenshotFolder = ScreenshotFolder,
      DeletesTheRawNotesAfterwards = DeletesTheRawNotesAfterwards,
      CreatesAnkiNotesToStudy = CreatesAnkiNotesToStudy,
      RunsAutomatically = RunsAutomatically
   };
}
