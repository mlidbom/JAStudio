namespace JAStudio.Core.Reservoir.Analysis.Numbering;

/// <summary>A failure reason, a reason a vocab's match was rejected, as a reservoir's analyses refer to it: by its number (<see cref="ReservoirAnalysisNumbers"/>).</summary>
/// <param name="Text">The reason as the analysis gives it, such as <c>forbids::poison_word</c>.</param>
public sealed record NumberedFailureReason(int Number, string Text);
