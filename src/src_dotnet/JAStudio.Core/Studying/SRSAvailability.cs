namespace JAStudio.Core.Studying;

/// <summary>
/// Whether there's an SRS, a spaced repetition system, for JAStudio to work with: the system where the student studies, which schedules the<br/>
/// reviews. Anki is the only one today. There is one while JAStudio runs inside Anki, and none while it runs without, as the headless<br/>
/// JAStudio and the standalone JAStudio do. What acts on what the spaced repetition system holds, such as importing Anki's notes or opening<br/>
/// its windows (<see cref="ISRSWindows"/>), is offered only while there is. Everything else, creating notes included, works the same either way.
/// </summary>
/// <remarks>
/// rationale:violation-approved:Magnus-2026-11-15: "SRS" for the spaced repetition system, in this name and every other: see<br/>
/// dev_docs/code_standard_exceptions/srs-for-the-spaced-repetition-system.md.
/// </remarks>
/// <remarks>review-naming:studying: SRSAvailability was AnkiAvailability (Claude, 2026-11-15).</remarks>
public sealed class SRSAvailability
{
   SRSAvailability(bool isAvailable) => IsAvailable = isAvailable;

   /// <summary>JAStudio runs inside Anki, or, in the specifications, beside a pretended one.</summary>
   public static readonly SRSAvailability Available = new(true);

   /// <summary>JAStudio runs without a spaced repetition system.</summary>
   public static readonly SRSAvailability Unavailable = new(false);

   public bool IsAvailable { get; }
}
