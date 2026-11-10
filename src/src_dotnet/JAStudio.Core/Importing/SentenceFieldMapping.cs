using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Importing;

/// <summary>
/// Which field of a <see cref="RawNote"/> each field of a corpus sentence takes its value from, by the raw note type's field names.<br/>
/// An empty name leaves the sentence's field empty. The raw note's media fields hold Anki's markup, <c>[sound:…]</c> and <c>&lt;img src="…"&gt;</c>,<br/>
/// read with <see cref="AnkiMediaMarkup"/>.
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

   /// <summary>
   /// The corpus sentence <paramref name="rawNote"/> becomes: its mapped fields, the texts, audio and screenshots as the sentence's source data, with its own<br/>
   /// tags and <paramref name="tagsToAdd"/>.
   /// </summary>
   public SentenceNote SentenceFrom(RawNote rawNote, IEnumerable<string> tagsToAdd, NoteServices services)
   {
      var sentence = new SentenceNote(services, SentenceDtoFrom(rawNote, tagsToAdd));
      foreach(var audio in AudioOf(rawNote)) sentence.SourceData.Audio.Add(audio);
      foreach(var screenshot in ScreenshotsOf(rawNote)) sentence.SourceData.Screenshots.Add(screenshot);
      return sentence;
   }

   SentenceDto SentenceDtoFrom(RawNote rawNote, IEnumerable<string> tagsToAdd) => new()
   {
      Id = Guid.NewGuid(),
      Reading = rawNote.Field(Reading),
      SourceName = rawNote.Field(SourceName),
      SourceData = new SentenceSourceDataDto
                   {
                      Question = rawNote.Field(SourceQuestion),
                      Answer = rawNote.Field(SourceAnswer),
                      Comments = rawNote.Field(SourceComments),
                      ExternalId = rawNote.Field(ExternalId)
                   },
      Tags = [.. rawNote.Tags.Concat(tagsToAdd).Distinct()]
   };

   public string QuestionOf(RawNote rawNote) => rawNote.Field(SourceQuestion);
   public string ExternalIdOf(RawNote rawNote) => rawNote.Field(ExternalId);

   /// <summary>The audio files the <see cref="Audio"/> field of <paramref name="rawNote"/> references, in Anki's media folder.</summary>
   public IReadOnlyList<AudioReference> AudioOf(RawNote rawNote) => AnkiMediaMarkup.AudioIn(rawNote.Field(Audio));

   /// <summary>The image files the <see cref="Screenshot"/> field of <paramref name="rawNote"/> references, in Anki's media folder.</summary>
   public IReadOnlyList<ImageReference> ScreenshotsOf(RawNote rawNote) => AnkiMediaMarkup.ImagesIn(rawNote.Field(Screenshot));

   /// <summary>Every media file <paramref name="rawNote"/> references in its mapped media fields.</summary>
   public IReadOnlyList<MediaReference> MediaOf(RawNote rawNote) => [.. AudioOf(rawNote), .. ScreenshotsOf(rawNote)];
}
