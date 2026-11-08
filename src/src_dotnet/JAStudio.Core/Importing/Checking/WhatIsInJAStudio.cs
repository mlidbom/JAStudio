using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.NoteFields;
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

   /// <summary>Whether a sentence has <paramref name="rawNote"/>'s external id, or a note uses one of its media files.</summary>
   public bool Has(RawNote rawNote, SentenceFieldMapping mapping) =>
      SentencesWithTheExternalIdOf(rawNote, mapping).Count > 0 || mapping.MediaOf(rawNote).Any(it => _notesByTheFilesTheyUse[it.FileName].Any());

   /// <summary>Why <paramref name="rawNote"/>, which isn't tagged as imported, is in JAStudio already, and what else is there of it.</summary>
   public List<RawNoteFinding> FindingsAboutANoteThatIsInJAStudio(RawNote rawNote, SentenceFieldMapping mapping) =>
   [
      .. SentencesWithTheExternalIdOf(rawNote, mapping).Select(it => RawNoteFinding.AlreadyInJAStudio($"A sentence in the corpus has the external id {mapping.ExternalIdOf(rawNote)}: {Describe(it)}.")),
      .. mapping.MediaOf(rawNote).Select(FindingAboutTheFile).OfType<RawNoteFinding>()
   ];

   /// <summary>What's in JAStudio of <paramref name="rawNote"/>, which an earlier run imported: since notes and files live apart, any of it may be gone.</summary>
   public List<RawNoteFinding> FindingsAboutAnImportedNote(RawNote rawNote, SentenceFieldMapping mapping)
   {
      var findings = new List<RawNoteFinding>();
      var externalId = mapping.ExternalIdOf(rawNote);
      var sentences = SentencesWithTheExternalIdOf(rawNote, mapping);

      if(sentences.Count > 0) findings.AddRange(sentences.Select(it => RawNoteFinding.Information($"A sentence in the corpus has the external id {externalId}: {Describe(it)}.")));
      else if(externalId != "") findings.Add(RawNoteFinding.Information($"No sentence in the corpus has the external id {externalId} any more."));

      findings.AddRange(mapping.MediaOf(rawNote).Select(it => FindingAboutTheFile(it) is {} finding
                                                                 ? RawNoteFinding.Information(finding.Description)
                                                                 : RawNoteFinding.Information($"The file {it.FileName} isn't in the media layer.")));
      return findings;
   }

   /// <summary>Which notes use <paramref name="file"/>, if it's in the media layer, or null when it isn't.</summary>
   RawNoteFinding? FindingAboutTheFile(MediaReference file)
   {
      if(!_mediaFileIndex.ContainsByOriginalFileName(file.FileName)) return null;

      var notesUsingIt = _notesByTheFilesTheyUse[file.FileName].ToList();
      return notesUsingIt.Count > 0
                ? RawNoteFinding.AlreadyInJAStudio($"The file {file.FileName} is in the media layer, used by {string.Join(", and by ", notesUsingIt.Select(Describe))}.")
                : RawNoteFinding.Information($"The file {file.FileName} is in the media layer, but no note uses it.");
   }

   List<SentenceNote> SentencesWithTheExternalIdOf(RawNote rawNote, SentenceFieldMapping mapping) =>
      mapping.ExternalIdOf(rawNote) is var externalId && externalId != "" ? [.. _sentencesByExternalId[externalId]] : [];

   static string Describe(JPNote note) => note switch
   {
      SentenceNote => $"the sentence \"{note.GetQuestion()}\"",
      VocabNote => $"the vocab \"{note.GetQuestion()}\"",
      _ => $"the note \"{note.GetQuestion()}\""
   };
}
