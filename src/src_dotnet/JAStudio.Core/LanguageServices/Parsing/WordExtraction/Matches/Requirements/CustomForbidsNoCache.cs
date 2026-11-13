namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches.Requirements;

/// <summary>
/// Base class for fused Forbids + MatchStateTest implementations that cannot cache state.
/// Used for display_requirements that must evaluate state freshly each time.
/// Uses composition with VocabMatchInspector for match context.
/// Subclasses must implement: InternalIsInState.
/// </summary>
public abstract class CustomForbidsNoCache : MatchRequirement
{
   protected VocabMatchInspector Inspector { get; }

   protected CustomForbidsNoCache(VocabMatchInspector inspector) => Inspector = inspector;

   /// <summary>
   /// Whether the match is currently in this state. NOT cached!
   /// </summary>
   public bool IsInState => InternalIsInState();

   /// <summary>
   /// Description of this state for error messages.
   /// </summary>
   public abstract string Description { get; }

   public override bool IsFulfilled => !IsInState;

   public override string FailureReason => !IsFulfilled ? $"forbids::{Description}" : "";

   /// <summary>
   /// Internal implementation of state checking. Override this in subclasses.
   /// </summary>
   protected abstract bool InternalIsInState();
}
