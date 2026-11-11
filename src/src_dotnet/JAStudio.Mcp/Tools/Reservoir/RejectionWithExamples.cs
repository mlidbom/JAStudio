namespace JAStudio.Mcp.Tools.Reservoir;

/// <summary>A reason rejecting a vocab's matches, as the analysis gives it, and the sentences where it does.</summary>
public sealed record RejectionWithExamples(string Reason, SentencesWithExamples Sentences);
