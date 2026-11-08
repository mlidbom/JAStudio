using System.Collections.Generic;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Importing.Checking;

/// <summary>A <see cref="RawNote"/> as an <see cref="ImportRun"/>'s check saw it: what the run does with it, and every <see cref="RawNoteFinding"/> behind that.</summary>
public class CheckedRawNote(RawNote rawNote, string question, string externalId, RawNoteOutcome outcome, IReadOnlyList<RawNoteFinding> findings)
{
   public RawNote RawNote { get; } = rawNote;

   /// <summary>The raw note's question, from the field the profile maps to the sentence's source question.</summary>
   public string Question { get; } = question;

   /// <summary>The raw note's external id, from the field the profile maps to it, or empty.</summary>
   public string ExternalId { get; } = externalId;

   public RawNoteOutcome Outcome { get; } = outcome;
   public IReadOnlyList<RawNoteFinding> Findings { get; } = findings;

   public override string ToString() => $"{RawNote} ({Question}): {Outcome}";
}
