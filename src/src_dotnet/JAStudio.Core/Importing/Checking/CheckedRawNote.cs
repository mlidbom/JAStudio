using System.Collections.Generic;
using JAStudio.Core.Importing.RawNotes;

namespace JAStudio.Core.Importing.Checking;

/// <summary>
/// A <see cref="RawNote"/> as an <see cref="ImportRun"/>'s check saw it: what the run does with it, and a <see cref="FieldCheck"/> for each field<br/>
/// the check looked at. The report shows a row per checked raw note.
/// </summary>
public class CheckedRawNote(RawNote rawNote,
                            RawNoteOutcome outcome,
                            FieldCheck question,
                            FieldCheck externalId,
                            FieldCheck audio,
                            FieldCheck screenshot,
                            IReadOnlyList<string> whatIsWrongWithTheNoteAsAWhole)
{
   readonly IReadOnlyList<string> _whatIsWrongWithTheNoteAsAWhole = whatIsWrongWithTheNoteAsAWhole;

   public RawNote RawNote { get; } = rawNote;
   public RawNoteOutcome Outcome { get; } = outcome;

   /// <summary>The check of the field the profile maps to the sentence's source question: a problem only when it's empty on a note to import.</summary>
   public FieldCheck Question { get; } = question;

   public FieldCheck ExternalId { get; } = externalId;
   public FieldCheck Audio { get; } = audio;
   public FieldCheck Screenshot { get; } = screenshot;

   /// <summary>Every orange or red mark's reason, a short line each: the note's as a whole first, then each field's.</summary>
   public IReadOnlyList<string> WhatIsWrong =>
      [.. _whatIsWrongWithTheNoteAsAWhole, .. Question.WhatIsWrong, .. ExternalId.WhatIsWrong, .. Audio.WhatIsWrong, .. Screenshot.WhatIsWrong];

   public override string ToString() => $"{RawNote} ({Question.Value}): {Outcome}";
}
