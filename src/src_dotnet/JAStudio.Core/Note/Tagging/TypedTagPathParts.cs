using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// The parts of a tag path typed so far to find a tag, separated by spaces, such as <c>nat s3 05</c>.<br/>
/// A tag path matches when it contains every part, anywhere and ignoring case, so <c>nat s3 05</c> finds <c>source::anime::natsume::s3::05</c>.<br/>
/// With no parts typed, every path matches.
/// </summary>
public sealed class TypedTagPathParts(string typed)
{
   readonly string[] _parts = typed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

   public bool AreAllIn(string tagPath) => _parts.All(part => tagPath.Contains(part, StringComparison.OrdinalIgnoreCase));

   /// <summary>The paths among <paramref name="tagPaths"/> that contain every part, in the order they came in.</summary>
   public IReadOnlyList<string> Matching(IEnumerable<string> tagPaths) => [..tagPaths.Where(AreAllIn)];
}
