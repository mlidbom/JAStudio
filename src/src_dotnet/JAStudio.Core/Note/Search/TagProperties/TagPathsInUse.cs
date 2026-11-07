using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.Search.TagProperties;

/// <summary>
/// The tag paths a <see cref="TagCondition"/> can be on: every tag some note has, and every parent of one, such as <c>source::anime</c>.<br/>
/// The tag condition's editor offers them, filtered by the parts of a path typed so far (<see cref="Matching"/>).<br/>
/// Made by <see cref="JPCollection.TagPathsInUse"/>.
/// </summary>
public class TagPathsInUse
{
   const string LevelSeparator = "::";

   internal TagPathsInUse(IEnumerable<JPNote> notes) =>
      All = [..notes.SelectMany(it => it.Tags)
                    .Select(it => it.Name)
                    .SelectMany(TheTagAndItsParents)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)];

   /// <summary>Every tag path in use, in alphabetical order, so that a tag comes right before the tags in its branch.</summary>
   public IReadOnlyList<string> All { get; }

   /// <summary>
   /// The tag paths that contain every part of <paramref name="typedParts"/>, ignoring case, where the parts are separated by spaces.<br/>
   /// So <c>nat s3 05</c> finds <c>source::anime::natsume::s3::05</c>. With no parts typed, every path matches.
   /// </summary>
   public IReadOnlyList<string> Matching(string typedParts)
   {
      var parts = typedParts.Split(' ', StringSplitOptions.RemoveEmptyEntries);
      return [..All.Where(path => parts.All(part => path.Contains(part, StringComparison.OrdinalIgnoreCase)))];
   }

   static IEnumerable<string> TheTagAndItsParents(string tagPath)
   {
      var levels = tagPath.Split(LevelSeparator);
      return Enumerable.Range(1, levels.Length).Select(levelCount => string.Join(LevelSeparator, levels.Take(levelCount)));
   }
}
