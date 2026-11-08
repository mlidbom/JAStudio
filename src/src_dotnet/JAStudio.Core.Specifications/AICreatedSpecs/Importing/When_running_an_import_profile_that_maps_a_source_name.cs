using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Importing;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note.Sentences;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Importing;

/// <summary>A JLab-style raw note, whose <c>Source</c> field names the show it's from, imported by a profile that maps that field to the source name.</summary>
public class When_running_an_import_profile_that_maps_a_source_name : ImportingSpecification, IAIGeneratedSpec
{
   const string JLabNoteType = "JLab Sentence";
   readonly SentenceNote _sentence;

   public When_running_an_import_profile_that_maps_a_source_name()
   {
      AnkiRawNotes.AddNoteType(new AnkiNoteType(JLabNoteType, ["Expression", "Meaning", "Source"]));
      AnkiRawNotes.Add(JLabNoteType, new Dictionary<string, string> { ["Expression"] = "ありがとう", ["Meaning"] = "Thank you", ["Source"] = "Shirokuma Cafe" });
      var profile = new ImportProfile
                    {
                       Name = "JLab",
                       AnkiNoteType = JLabNoteType,
                       FieldMapping = new SentenceFieldMapping { SourceQuestion = "Expression", SourceAnswer = "Meaning", SourceName = "Source" }
                    };

      Importer.Prepare(profile).ExecuteUnlessOutOfDate().Must().BeTrue();
      _sentence = Sentences.All().Single();
   }

   [XF] public void the_sentence_takes_its_source_name_from_the_mapped_field() => _sentence.SourceName.Value.Must().Be("Shirokuma Cafe");
}
