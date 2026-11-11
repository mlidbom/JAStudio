using System;
using static System.FormattableString;

namespace JAStudio.Core.Reservoir.Keeping;

/// <summary>What keeping a deck did (<see cref="ReservoirKeeping.Keep"/>): how much it kept, how much of its media the store had already, and how long its media took.</summary>
/// <param name="MediaFilesNewToTheStore">How many of its media files the store didn't have yet, from an earlier deck or the same one.</param>
/// <param name="MediaTime">How long reading, normalizing, hashing and storing its media took.</param>
public sealed record KeptDeck(
   string Name,
   int Notes,
   int Sentences,
   long CollectionDatabaseLength,
   long NoteLinesLength,
   int MediaFiles,
   long MediaLength,
   int MediaFilesNewToTheStore,
   int MediaFilesTheDeckRefersToButLacks,
   TimeSpan MediaTime)
{
   public override string ToString() =>
      Invariant($"{Name}: {Notes:N0} notes, {Sentences:N0} sentences; database {CollectionDatabaseLength / 1e6:F1} MB, note lines {NoteLinesLength / 1e6:F1} MB; ")
    + Invariant($"media {MediaFiles:N0} files, {MediaLength / 1e6:F0} MB, {MediaFilesNewToTheStore:N0} new to the store, {MediaFilesTheDeckRefersToButLacks:N0} referred to but missing; ")
    + Invariant($"media took {MediaTime.TotalSeconds:F1} s, {MediaFiles / MediaTime.TotalSeconds:F0} files and {MediaLength / 1e6 / MediaTime.TotalSeconds:F0} MB a second");
}
