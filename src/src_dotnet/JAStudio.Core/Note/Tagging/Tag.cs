using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;

namespace JAStudio.Core.Note.Tagging;

/// <summary>
/// A label on a note, such as <c>source::anime::natsume::s3::01</c>, named by its tag path, whose levels <c>::</c> separates.<br/>
/// There is one <see cref="Tag"/> object per tag, shared by every note that has it.
/// </summary>
/// <remarks>
/// Tags ignore case, as in Anki: a name that differs from an existing tag's only in case gets that tag, with its existing spelling.<br/>
/// So JAStudio can't create two tags that Anki would treat as one.
/// </remarks>
public class Tag
{
   /// <summary>What separates the levels of a tag path, as in <c>source::anime</c>.</summary>
   public const string LevelSeparator = "::";

   static readonly IMonitor Monitor = IMonitor.New();
   static readonly Dictionary<string, Tag> ByName = new(StringComparer.OrdinalIgnoreCase);

   public string Name { get; }

   /// <summary>
   /// The paths of the branches this tag is in, from the top level down, its own last: <c>source</c> and <c>source::anime</c> for <c>source::anime</c>.<br/>
   /// A note with this tag is in each of these branches.
   /// </summary>
   public IReadOnlyList<string> PathsOfTheBranchesItIsIn { get; }

   Tag(string name)
   {
      Name = name;
      PathsOfTheBranchesItIsIn = [..new TagBranch(name).PathsAboveIt, name];
   }

   /// <summary>The tag named <paramref name="name"/>, ignoring case, created with that spelling if there is none yet.</summary>
   public static Tag FromName(string name) => Monitor.Locked(() =>
   {
      if(ByName.TryGetValue(name, out var existing)) return existing;

      var created = new Tag(name);
      ByName.Add(name, created);
      return created;
   });

   /// <summary>
   /// Why <paramref name="typedName"/>, typed by a person, can't be the name of a note's own tag, or null when it can: it isn't well formed<br/>
   /// (<see cref="WhyItIsNotWellFormed"/>), or it's in the anki branch (<see cref="Tags.Anki"/>), which holds the notes' tags in Anki.
   /// </summary>
   public static string? WhyItCannotBeTheNameOfATag(string typedName) =>
      WhyItIsNotWellFormed(typedName)
      ?? (Tags.Anki.Branch.Contains(typedName) ? Tags.Anki.WhyItIsReadOnly : null);

   /// <summary>
   /// Why <paramref name="typedText"/>, typed by a person as a tag's name or a level of one, isn't well formed, or null when it is:<br/>
   /// it has no spaces, since Anki separates tags with spaces, and no empty levels, such as in <c>a::::b</c>.
   /// </summary>
   public static string? WhyItIsNotWellFormed(string typedText) =>
      typedText.Length == 0                                       ? "A tag needs a name."
      : typedText.Any(char.IsWhiteSpace)                          ? "A tag can't contain spaces."
      : typedText.Split(LevelSeparator).Any(string.IsNullOrEmpty) ? $"A tag can't have an empty level, as {typedText} has."
                                                                  : null;
}
