using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Importing;

/// <summary>
/// Which field of a <see cref="RawNote"/> each field of a corpus sentence takes its value from, by the raw note type's field names.<br/>
/// An empty name leaves the sentence's field empty. Media fields hold Anki's markup, <c>[sound:…]</c> and <c>&lt;img src="…"&gt;</c>, as the corpus does.
/// </summary>
public class SentenceFieldMapping
{
   public string SourceQuestion { get; set; } = "";
   public string SourceAnswer { get; set; } = "";
   public string Reading { get; set; } = "";

   /// <summary>The raw note's id in its source, such as Immersion Kit's <c>anime_the_garden_of_words_000000346</c>, or a subs2srs id.</summary>
   public string ExternalId { get; set; } = "";

   /// <summary>The name of the work or deck the raw note came from, such as a JLab note's <c>Source</c>: see <see cref="SentenceDto.SourceName"/>.</summary>
   public string SourceName { get; set; } = "";

   public string SourceComments { get; set; } = "";
   public string Audio { get; set; } = "";
   public string Screenshot { get; set; } = "";

   public SentenceFieldMapping Copy() => (SentenceFieldMapping)MemberwiseClone();

   /// <summary>The names of the raw note type's fields this mapping takes values from.</summary>
   public IEnumerable<string> MappedAnkiFields() =>
      new[] { SourceQuestion, SourceAnswer, Reading, ExternalId, SourceName, SourceComments, Audio, Screenshot }.Where(it => it != "");

   /// <summary>The corpus sentence <paramref name="rawNote"/> becomes: its mapped fields, with its own tags and <paramref name="tagsToAdd"/>.</summary>
   public SentenceDto SentenceDataFrom(RawNote rawNote, IEnumerable<string> tagsToAdd) => new()
   {
      Id = Guid.NewGuid(),
      SourceQuestion = rawNote.Field(SourceQuestion),
      SourceAnswer = rawNote.Field(SourceAnswer),
      Reading = rawNote.Field(Reading),
      ExternalId = rawNote.Field(ExternalId),
      SourceName = rawNote.Field(SourceName),
      SourceComments = rawNote.Field(SourceComments),
      Audio = rawNote.Field(Audio),
      Screenshot = rawNote.Field(Screenshot),
      Tags = [.. rawNote.Tags.Concat(tagsToAdd).Distinct()]
   };

   public string QuestionOf(RawNote rawNote) => rawNote.Field(SourceQuestion);
   public string ExternalIdOf(RawNote rawNote) => rawNote.Field(ExternalId);

   /// <summary>The audio files the <see cref="Audio"/> field of <paramref name="rawNote"/> references, in Anki's media folder.</summary>
   public List<MediaReference> AudioOf(RawNote rawNote) => MediaFieldParsing.ParseAudioReferences(rawNote.Field(Audio));

   /// <summary>The image files the <see cref="Screenshot"/> field of <paramref name="rawNote"/> references, in Anki's media folder.</summary>
   public List<MediaReference> ScreenshotsOf(RawNote rawNote) => MediaFieldParsing.ParseImageReferences(rawNote.Field(Screenshot));

   /// <summary>Every media file <paramref name="rawNote"/> references in its mapped media fields.</summary>
   public List<MediaReference> MediaOf(RawNote rawNote) => [.. AudioOf(rawNote), .. ScreenshotsOf(rawNote)];
}
