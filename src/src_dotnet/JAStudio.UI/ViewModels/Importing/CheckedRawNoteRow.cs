using System;
using JAStudio.Core.Importing.Checking;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>
/// A row of the import profiles window's report: one <see cref="CheckedRawNote"/>. Its question is text; its outcome and its checked fields are<br/>
/// <see cref="ReportMark"/>s, with the details on hover; and <see cref="WhatIsWrong"/> has text only for an orange or red mark.
/// </summary>
class CheckedRawNoteRow(CheckedRawNote checkedRawNote)
{
   public RawNoteOutcome Outcome { get; } = checkedRawNote.Outcome;

   public string Question { get; } = checkedRawNote.Question.Value;

   /// <summary>Whether the question is empty on a note to import, which shows a red ✗ in its place.</summary>
   public bool QuestionIsAProblem { get; } = checkedRawNote.Question.Mark == CheckMark.Problem;

   public ReportMark QuestionMark { get; } = ReportMark.Of(checkedRawNote.Question);
   public ReportMark OutcomeMark { get; } = ReportMark.Of(checkedRawNote.Outcome);
   public ReportMark ExternalId { get; } = ReportMark.Of(checkedRawNote.ExternalId);
   public ReportMark Audio { get; } = ReportMark.Of(checkedRawNote.Audio);
   public ReportMark Screenshot { get; } = ReportMark.Of(checkedRawNote.Screenshot);

   /// <summary>A line for each orange or red mark, or nothing.</summary>
   public string WhatIsWrong { get; } = string.Join(Environment.NewLine, checkedRawNote.WhatIsWrong);
}
