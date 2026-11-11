namespace JAStudio.Core.Reservoir.Sources.Subtitles;

public sealed partial class SubtitleFile
{
   /// <summary>A line the subtitles show, as the file has it: the time it's shown at, and its text, formatting and all.</summary>
   /// <param name="StartTime">The time it's shown at, as the file writes it, such as <c>00:01:02,345</c>.</param>
   sealed record ShownLine(string StartTime, string Text);
}
