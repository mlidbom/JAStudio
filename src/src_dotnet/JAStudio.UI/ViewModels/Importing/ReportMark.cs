using System;
using JAStudio.Core.Importing.Checking;

namespace JAStudio.UI.ViewModels.Importing;

/// <summary>
/// A mark in the import profiles window's report: ✓, – or ✗ in a <see cref="ReportMarkColour"/>, with its details on hover.<br/>
/// It marks a raw note's outcome, or one of its <see cref="FieldCheck"/>s, so a long report can be scanned by colour.
/// </summary>
class ReportMark(string symbol, ReportMarkColour colour, string hoverText)
{
   readonly ReportMarkColour _colour = colour;

   public string Symbol { get; } = symbol;
   public string HoverText { get; } = hoverText;

   public bool IsGreen => _colour == ReportMarkColour.Green;
   public bool IsGrey => _colour == ReportMarkColour.Grey;
   public bool IsOrange => _colour == ReportMarkColour.Orange;
   public bool IsRed => _colour == ReportMarkColour.Red;

   /// <summary>The mark of <paramref name="check"/>, with the field's value and the full finding on hover.</summary>
   public static ReportMark Of(FieldCheck check)
   {
      return check.Mark switch
      {
         CheckMark.Fine => new ReportMark("✓", ReportMarkColour.Green, HoverText()),
         CheckMark.Neutral => new ReportMark("–", ReportMarkColour.Grey, HoverText()),
         CheckMark.AlreadyInJAStudio => new ReportMark("✗", ReportMarkColour.Orange, HoverText()),
         CheckMark.Problem => new ReportMark("✗", ReportMarkColour.Red, HoverText()),
         _ => throw new ArgumentOutOfRangeException(nameof(check), check.Mark, null)
      };

      string HoverText() => check.Value == "" ? check.Finding : $"{check.Value}{Environment.NewLine}{check.Finding}";
   }

   /// <summary>The mark of <paramref name="outcome"/>, with its name on hover.</summary>
   public static ReportMark Of(RawNoteOutcome outcome) => outcome switch
   {
      RawNoteOutcome.WillBeImported => new ReportMark("✓", ReportMarkColour.Green, NameOf(outcome)),
      RawNoteOutcome.ImportedByAnEarlierRun => new ReportMark("✓", ReportMarkColour.Grey, NameOf(outcome)),
      RawNoteOutcome.AlreadyInJAStudio => new ReportMark("✗", ReportMarkColour.Orange, NameOf(outcome)),
      RawNoteOutcome.Problem => new ReportMark("✗", ReportMarkColour.Red, NameOf(outcome)),
      _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
   };

   /// <summary>The name the window gives <paramref name="outcome"/>.</summary>
   public static string NameOf(RawNoteOutcome outcome) => outcome switch
   {
      RawNoteOutcome.WillBeImported => "Will be imported",
      RawNoteOutcome.ImportedByAnEarlierRun => "Imported by an earlier run",
      RawNoteOutcome.AlreadyInJAStudio => "Skipped: already in JAStudio",
      RawNoteOutcome.Problem => "Problem",
      _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome, null)
   };
}
