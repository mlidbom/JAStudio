using JAStudio.Core.Reservoir.Keeping.Media;
using static System.FormattableString;

namespace JAStudio.Core.Reservoir.Keeping;

/// <summary>What keeping a deck did (<see cref="ReservoirKeeping.Keep"/>): how much it kept, how much of its media the store had already, and how long its media took.</summary>
/// <param name="Name">The file name of the deck's package.</param>
/// <param name="Notes">How many notes the deck has, each kept as a line of <c>notes.jsonl</c>.</param>
/// <param name="Sentences">How many of its notes give a sentence.</param>
/// <param name="CollectionDatabaseLength">The length of the collection database kept, in bytes.</param>
/// <param name="NoteLinesLength">The length of <c>notes.jsonl</c>, in bytes.</param>
/// <param name="Media">The deck's media, as the store keeps it.</param>
/// <param name="MediaReferencesToFilesThePackageLacks">How many of its sentences' references to media files refer to one its package lacks.</param>
public sealed record KeptDeck(
   string Name,
   int Notes,
   int Sentences,
   long CollectionDatabaseLength,
   long NoteLinesLength,
   StoredDeckMedia Media,
   int MediaReferencesToFilesThePackageLacks)
{
   public override string ToString() =>
      Invariant($"{Name}: {Notes:N0} notes, {Sentences:N0} sentences; database {CollectionDatabaseLength / 1e6:F1} MB, note lines {NoteLinesLength / 1e6:F1} MB; ")
    + Invariant($"media {Media.Files.Count:N0} files, {Media.OriginalLength / 1e6:F0} MB, {Media.FilesNewToTheStore:N0} new to the store, {MediaReferencesToFilesThePackageLacks:N0} referred to but missing; ")
    + Invariant($"media took {Media.TimeTaken.TotalSeconds:F1} s, {Media.Files.Count / Media.TimeTaken.TotalSeconds:F0} files and {Media.OriginalLength / 1e6 / Media.TimeTaken.TotalSeconds:F0} MB a second");
}
