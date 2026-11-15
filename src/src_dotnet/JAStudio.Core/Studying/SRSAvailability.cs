namespace JAStudio.Core.Studying;

/// <summary>
/// Whether there's an SRS, a spaced repetition system, for JAStudio to work with, and whether it can be used right now: the system where the<br/>
/// student studies, which schedules the reviews. Anki is the only one today. There is one while JAStudio runs inside Anki, and none while it runs<br/>
/// without, as the headless JAStudio and the standalone JAStudio do. What acts on what the spaced repetition system holds, such as importing<br/>
/// Anki's notes or opening its windows (<see cref="ISRSWindows"/>), is offered only while there is. Everything else, creating notes included,<br/>
/// works the same either way.
/// </summary>
/// <remarks>
/// rationale:violation-approved:Magnus-2026-11-15: "SRS" for the spaced repetition system, in this name and every other: see<br/>
/// dev_docs/code_standard_exceptions/srs-for-the-spaced-repetition-system.md.
/// </remarks>
/// <remarks>
/// review-naming:studying: SRSAvailability was AnkiAvailability (Claude, 2026-11-15). WhyItCannotBeUsedNow and CanBeUsedNow are new the same<br/>
/// day, for JAStudio's sync with the spaced repetition system (<see cref="SRSSync"/>).
/// </remarks>
public abstract partial class SRSAvailability
{
   /// <summary>JAStudio runs without a spaced repetition system.</summary>
   public static readonly SRSAvailability Unavailable = new None();

   /// <summary>Whether there is a spaced repetition system: what acts on what it holds is offered only while there is.</summary>
   public abstract bool IsAvailable { get; }

   /// <summary>
   /// Why what the spaced repetition system holds can't be read or changed right now, in words for a person, or null when it can. Anki's can't<br/>
   /// while no Anki profile is open, and while Anki's own sync, with AnkiWeb, is under way.
   /// </summary>
   public abstract string? WhyItCannotBeUsedNow { get; }

   /// <summary>Whether what the spaced repetition system holds can be read and changed right now (<see cref="WhyItCannotBeUsedNow"/>).</summary>
   public bool CanBeUsedNow => WhyItCannotBeUsedNow == null;
}
