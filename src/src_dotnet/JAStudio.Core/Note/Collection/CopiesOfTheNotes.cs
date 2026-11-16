using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Teventive.Taggregates;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// The check that a copy of a note loses nothing of it: a copy made through the note's snapshot, its DTO, as a taggregate store makes the copy a unit<br/>
/// of work changes. Whatever a copy loses is lost at the note's next edit. Compze's <see cref="SnapshotRoundTrip"/> compares the note and its copy<br/>
/// field by field, in every part, and their snapshots.
/// </summary>
public static class CopiesOfTheNotes
{
   /// <summary>Each note of <paramref name="collection"/> whose copy differs from it, with the differences, checked on many threads.</summary>
   public static IReadOnlyList<ACopyThatDiffers> ThatDifferIn(JPCollection collection) =>
   [
      .. collection.Vocab.All().Cast<JPNote>()
                   .Concat(collection.Kanji.All())
                   .Concat(collection.Sentences.All())
                   .AsParallel()
                   .Select(DifferencesOfACopyOf)
                   .OfType<ACopyThatDiffers>()
   ];

   static ACopyThatDiffers? DifferencesOfACopyOf(JPNote note)
   {
      var differences = note switch
      {
         VocabNote vocab => SnapshotRoundTrip.DifferencesOf(vocab, () => new VocabNote(vocab.Services), CorpusNoteDto.HoldTheSameState),
         KanjiNote kanji => SnapshotRoundTrip.DifferencesOf(kanji, () => new KanjiNote(kanji.Services), CorpusNoteDto.HoldTheSameState),
         SentenceNote sentence => SnapshotRoundTrip.DifferencesOf(sentence, () => new SentenceNote(sentence.Services), CorpusNoteDto.HoldTheSameState),
         _ => throw new ArgumentException($"{note.GetType().FullName} isn't a class of note.")
      };

      return differences.Count > 0 ? new ACopyThatDiffers(note, differences) : null;
   }

   /// <summary>A note whose copy differs from it, and how: each a line that names the field by its path, such as <c>VocabNote.Forms._forms[2]</c>.</summary>
   public sealed record ACopyThatDiffers(JPNote Note, IReadOnlyList<string> Differences);
}
