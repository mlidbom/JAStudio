using System.Collections.Generic;
using System.IO;
using JAStudio.Core;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.TestUtils;

namespace JAStudio.UI.Views.Specifications.AICreatedSpecs.Importing;

/// <summary>
/// Specifications of the import windows, with a pretend Anki (<see cref="InMemoryAnkiRawNotes"/>) that has a note type of Immersion Kit's sentence cards,<br/>
/// and a saved <see cref="ImportProfile"/> for it, <see cref="Profile"/>, that maps no screenshot.
/// </summary>
public abstract class ImportWindowsSpecification : SpecificationOpeningWindows
{
   const string ImmersionKitNoteType = "Immersion Kit Sentence";

   protected ImportWindowsSpecification()
   {
      Directory.CreateDirectory(AnkiMediaFolder);
      AnkiRawNotes.AddNoteType(new AnkiNoteType(ImmersionKitNoteType, ["Expression", "English", "Audio Sentence", "ID"]));

      var savedImportProfiles = GetService<SavedImportProfiles>();
      Profile = savedImportProfiles.AddNew();
      Profile.Name = "Immersion Kit";
      Profile.AnkiNoteType = ImmersionKitNoteType;
      Profile.FieldMapping = new SentenceFieldMapping { SourceQuestion = "Expression", SourceAnswer = "English", ExternalId = "ID", Audio = "Audio Sentence" };
      Profile.TagsToAdd = ["source::immersion_kit"];
      Profile.AudioFolder = "commercial-01/anime/immersion-kit/audio";
      savedImportProfiles.SaveTheChangeOf(Profile);
   }

   protected ImportProfile Profile { get; }

   InMemoryAnkiRawNotes AnkiRawNotes => GetService<InMemoryAnkiRawNotes>();
   string AnkiMediaFolder => GetService<IEnvironmentPaths>().AnkiMediaDir;

   /// <summary>Adds a sentence card to the pretend Anki, with its audio file in Anki's media folder: an MP3 file's first bytes, which the check doesn't look past.</summary>
   protected void AddASentenceCard(string expression, string id, string audioFile)
   {
      File.WriteAllBytes(Path.Combine(AnkiMediaFolder, audioFile), "ID3"u8.ToArray());
      AddASentenceCardWithoutItsAudioFile(expression, id, audioFile);
   }

   /// <summary>Adds a sentence card to the pretend Anki whose audio file isn't in Anki's media folder.</summary>
   protected void AddASentenceCardWithoutItsAudioFile(string expression, string id, string audioFile) =>
      AnkiRawNotes.Add(ImmersionKitNoteType,
                       new Dictionary<string, string>
                       {
                          ["Expression"] = expression,
                          ["English"] = "English",
                          ["Audio Sentence"] = $"[sound:{audioFile}]",
                          ["ID"] = id
                       });
}
