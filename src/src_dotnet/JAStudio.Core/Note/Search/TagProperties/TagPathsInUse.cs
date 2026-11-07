using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;

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

   /// <summary>The tag paths that contain every part of <paramref name="typedParts"/>, as <see cref="TypedTagPathParts"/> matches them.</summary>
   public IReadOnlyList<string> Matching(string typedParts) => new TypedTagPathParts(typedParts).Matching(All);

   static IEnumerable<string> TheTagAndItsParents(string tagPath)
   {
      var levels = tagPath.Split(LevelSeparator);
      return Enumerable.Range(1, levels.Length).Select(levelCount => string.Join(LevelSeparator, levels.Take(levelCount)));
   }
}
