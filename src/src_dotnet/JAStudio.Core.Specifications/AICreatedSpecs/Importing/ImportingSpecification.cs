using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.Checking;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

/// <summary>
/// Specifications of importing raw notes from a pretend Anki (<see cref="InMemoryAnkiRawNotes"/>) with the import profile for Immersion Kit's sentence cards,<br/>
/// whose note type and field mapping are those of <c>dev_docs/import-profiles.md</c>. The raw notes' media files go in the test environment's Anki media folder.
/// </summary>
public abstract class ImportingSpecification : SpecificationStartingWithAnEmptyCollection
{
   protected const string ImmersionKitNoteType = "Immersion Kit Sentence";
   protected const string AudioFolder = "commercial-01/anime/immersion-kit/audio";
   protected const string ScreenshotFolder = "commercial-01/anime/immersion-kit/screenshots";

   /// <summary>An MP3 file that ReplayGain's measurement says isn't at the reference loudness, and how mp3gain 1.4.6 normalizes it.</summary>
   protected static readonly byte[] SentenceAudio = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "AICreatedSpecs", "Importing", "RawNoteMedia", "sentence-audio.mp3"));
   protected static readonly byte[] SentenceAudioNormalizedByMp3Gain = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "AICreatedSpecs", "Importing", "RawNoteMedia", "sentence-audio.normalized-by-mp3gain.mp3"));

   protected ImportingSpecification()
   {
      Directory.CreateDirectory(AnkiMediaFolder);
      AnkiRawNotes.AddNoteType(new AnkiNoteType(ImmersionKitNoteType, ["Expression", "English", "Reading", "Screenshot", "Audio Sentence", "ID"]));
   }

   protected InMemoryAnkiRawNotes AnkiRawNotes => GetService<InMemoryAnkiRawNotes>();
   protected Importer Importer => GetService<Importer>();
   protected SentenceCollection Sentences => GetService<SentenceCollection>();
   protected string AnkiMediaFolder => GetService<IEnvironmentPaths>().AnkiMediaDir;
   protected string MediaLayerFolder => GetService<IEnvironmentPaths>().MediaDir;

   protected readonly ImportProfile ImmersionKitProfile = new()
   {
      Name = "Immersion Kit",
      AnkiNoteType = ImmersionKitNoteType,
      FieldMapping = new SentenceFieldMapping
                     {
                        SourceQuestion = "Expression",
                        SourceAnswer = "English",
                        Reading = "Reading",
                        ExternalId = "ID",
                        Audio = "Audio Sentence",
                        Screenshot = "Screenshot"
                     },
      TagsToAdd = ["source::immersion_kit"],
      AudioFolder = AudioFolder,
      ScreenshotFolder = ScreenshotFolder
   };

   /// <summary>Adds an Immersion Kit sentence card to the pretend Anki, with its audio and screenshot in Anki's media folder.</summary>
   protected RawNote AddImmersionKitSentenceCard(string expression, string id, string audioFile, string screenshotFile, string english = "English", string reading = "reading")
   {
      File.WriteAllBytes(Path.Combine(AnkiMediaFolder, audioFile), SentenceAudio);
      File.WriteAllBytes(Path.Combine(AnkiMediaFolder, screenshotFile), [0xFF, 0xD8, 0xFF]);
      return AddImmersionKitSentenceCardWithoutItsMediaFiles(expression, id, $"[sound:{audioFile}]", $"<img src=\"{screenshotFile}\">", english, reading);
   }

   /// <summary>Adds an Immersion Kit sentence card to the pretend Anki, with <paramref name="audioField"/> and <paramref name="screenshotField"/> as they are, and no media files.</summary>
   protected RawNote AddImmersionKitSentenceCardWithoutItsMediaFiles(string expression, string id, string audioField, string screenshotField, string english = "English", string reading = "reading") =>
      AnkiRawNotes.Add(ImmersionKitNoteType,
                       new Dictionary<string, string>
                       {
                          ["Expression"] = expression,
                          ["English"] = english,
                          ["Reading"] = reading,
                          ["Screenshot"] = screenshotField,
                          ["Audio Sentence"] = audioField,
                          ["ID"] = id
                       });

   /// <summary>What <paramref name="run"/>'s check found about <paramref name="rawNote"/>.</summary>
   protected static CheckedRawNote CheckOf(ImportRun run, RawNote rawNote) => run.CheckedRawNotes.Single(it => it.RawNote.AnkiNoteId == rawNote.AnkiNoteId);

   protected static bool AFindingSays(CheckedRawNote checkedRawNote, string text) => checkedRawNote.Findings.Any(it => it.Description.Contains(text));
}
