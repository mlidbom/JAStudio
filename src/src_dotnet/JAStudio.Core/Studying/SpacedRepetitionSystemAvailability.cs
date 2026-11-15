using JAStudio.Core.Anki;

namespace JAStudio.Core.Studying;

/// <summary>
/// Whether there's a spaced repetition system for JAStudio to work with: the system where the student studies, which schedules the reviews.<br/>
/// Anki is the only one today. There is one while JAStudio runs inside Anki, and none while it runs without, as the headless JAStudio and the<br/>
/// standalone JAStudio do. What acts on what the spaced repetition system holds, such as importing Anki's notes or opening its windows<br/>
/// (<see cref="IAnkiWindows"/>), is offered only while there is. Everything else, creating notes included, works the same either way.
/// </summary>
public sealed class SpacedRepetitionSystemAvailability
{
   //review-naming:studying: SpacedRepetitionSystemAvailability was AnkiAvailability (Claude, 2026-11-15).
   SpacedRepetitionSystemAvailability(bool isAvailable) => IsAvailable = isAvailable;

   /// <summary>JAStudio runs inside Anki, or, in the specifications, beside a pretended one.</summary>
   public static readonly SpacedRepetitionSystemAvailability Available = new(true);

   /// <summary>JAStudio runs without a spaced repetition system.</summary>
   public static readonly SpacedRepetitionSystemAvailability Unavailable = new(false);

   public bool IsAvailable { get; }
}
