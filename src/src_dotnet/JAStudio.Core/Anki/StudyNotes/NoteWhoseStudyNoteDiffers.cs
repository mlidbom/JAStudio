using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>A note whose study-note in Anki differs from the one JAStudio would write for it now, with the fields that differ.</summary>
public record NoteWhoseStudyNoteDiffers(JPNote Note, IReadOnlyList<StudyNoteFieldDifference> Fields)
{
   /// <summary>
   /// Whether all that differs is the markup of the note's audio: the fields that differ are audio fields (<see cref="AnkiFieldNames.AudioFields"/>), and<br/>
   /// between them they hold the same audio files in the same order in Anki as JAStudio would write, so only the markup around the files, such as<br/>
   /// a space or an <c>&amp;nbsp;</c>, or which of the fields holds which file, differs.
   /// </summary>
   public bool OnlyTheMarkupOfItsAudioDiffers => OnlyAudioFieldsDiffer && TheyHoldTheSameAudioFilesInTheSameOrderInAnkiAsJAStudioWouldWrite;

   /// <summary>Whether every field that differs is an audio field (<see cref="AnkiFieldNames.AudioFields"/>).</summary>
   bool OnlyAudioFieldsDiffer => Fields.All(it => AnkiFieldNames.AudioFields.Contains(it.Field));

   /// <summary>Whether, between them, the fields that differ hold the same audio files in the same order in Anki as JAStudio would write.</summary>
   bool TheyHoldTheSameAudioFilesInTheSameOrderInAnkiAsJAStudioWouldWrite =>
      AudioFilesIn(it => it.InAnki ?? "").SequenceEqual(AudioFilesIn(it => it.JAStudioWrites));

   /// <summary>The names of the audio files in the fields that differ, in the order of <see cref="AnkiFieldNames.AudioFields"/>, each field's value read by <paramref name="valueOf"/>.</summary>
   IEnumerable<string> AudioFilesIn(Func<StudyNoteFieldDifference, string> valueOf) =>
      Fields.OrderBy(it => AnkiFieldNames.AudioFields.ToList().IndexOf(it.Field))
            .SelectMany(it => AnkiMediaMarkup.AudioIn(valueOf(it)))
            .Select(it => it.FileName);
}
