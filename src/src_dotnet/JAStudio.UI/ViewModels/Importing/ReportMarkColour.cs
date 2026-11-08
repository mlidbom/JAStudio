namespace JAStudio.UI.ViewModels.Importing;

/// <summary>The colour of a <see cref="ReportMark"/>: what the mark means at a glance.</summary>
enum ReportMarkColour
{
   /// <summary>As it should be, or will be imported.</summary>
   Green,

   /// <summary>Not checked, nothing wrong, or imported by an earlier run.</summary>
   Grey,

   /// <summary>Already in JAStudio, so skipped.</summary>
   Orange,

   /// <summary>A problem, which stops the whole run.</summary>
   Red
}
