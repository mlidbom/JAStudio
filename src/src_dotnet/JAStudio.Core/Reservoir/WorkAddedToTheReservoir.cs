using JAStudio.Core.Reservoir.Sources;

namespace JAStudio.Core.Reservoir;

/// <summary>What adding a work to the reservoir did (<see cref="ReservoirFile.Add"/>).</summary>
/// <param name="Sentences">How many sentences the work gave.</param>
/// <param name="NewSentences">How many of them the reservoir didn't have yet, the work's own repeats not counted.</param>
public sealed record WorkAddedToTheReservoir(ReservoirSource Source, long Sentences, long NewSentences)
{
   public override string ToString() => $"{Source.Collection} / {Source.Work}: {Sentences} sentences, {NewSentences} new";
}
