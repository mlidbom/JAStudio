using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Media;

namespace JAStudio.Core.Importing.Checking;

/// <summary>
/// What JAStudio holds that a <see cref="RawNote"/> can match, for an <see cref="ImportCheck"/>: the corpus's sentences by their external ids,<br/>
/// the media layer's files, and the notes using each file. Notes and files live apart, so a file can be in the media layer with no note using it.
/// </summary>
class WhatIsInJAStudio
{
   readonly MediaFileIndex _mediaFileIndex;
   readonly ILookup<string, SentenceNote> _sentencesByExternalId;
   readonly ILookup<string, JPNote> _notesByTheFilesTheyUse;

   public WhatIsInJAStudio(JPCollection collection, MediaFileIndex mediaFileIndex)
   {
      _mediaFileIndex = mediaFileIndex;
      _sentencesByExternalId = collection.Sentences.All().Where(it => it.ExternalId.Value != "").ToLookup(it => it.ExternalId.Value);
      _notesByTheFilesTheyUse = collection.Vocab.All().Cast<JPNote>()
                                          .Concat(collection.Sentences.All())
                                          .SelectMany(note => note.MediaReferences.Select(file => (file.FileName, note)))
                                          .ToLookup(it => it.FileName, it => it.note, StringComparer.OrdinalIgnoreCase);
   }

   /// <summary>The sentences with the external id <paramref name="externalId"/>: none for an empty one.</summary>
   public List<SentenceNote> SentencesWithTheExternalId(string externalId) => externalId == "" ? [] : [.. _sentencesByExternalId[externalId]];

   /// <summary>The file named <paramref name="fileName"/> in the media layer, or null when there's none.</summary>
   public MediaAttachment? InTheMediaLayer(string fileName) => _mediaFileIndex.TryGetByOriginalFileName(fileName);

   /// <summary>The notes whose media fields reference the file named <paramref name="fileName"/>.</summary>
   public List<JPNote> NotesUsing(string fileName) => [.. _notesByTheFilesTheyUse[fileName]];

   /// <summary>Whether a sentence has <paramref name="rawNote"/>'s external id, or a note uses one of its media files.</summary>
   public bool HasAnythingOf(RawNote rawNote, SentenceFieldMapping mapping) =>
      SentencesWithTheExternalId(mapping.ExternalIdOf(rawNote)).Count > 0 || mapping.MediaOf(rawNote).Any(it => NotesUsing(it.FileName).Count > 0);

   /// <summary>A note as the report names it, such as <c>the sentence "ずっと1人で…"</c>.</summary>
   public static string Describe(JPNote note) => note switch
   {
      SentenceNote => $"the sentence \"{note.GetQuestion()}\"",
      VocabNote => $"the vocab \"{note.GetQuestion()}\"",
      _ => $"the note \"{note.GetQuestion()}\""
   };

   public static string Describe(IEnumerable<JPNote> notes) => string.Join(", and ", notes.Select(Describe));
}
