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
   const string LevelSeparator = "::";

   static readonly IMonitor Monitor = IMonitor.New();
   static readonly Dictionary<string, Tag> ByName = new(StringComparer.OrdinalIgnoreCase);

   public string Name { get; }

   Tag(string name) => Name = name;

   /// <summary>The tag named <paramref name="name"/>, ignoring case, created with that spelling if there is none yet.</summary>
   public static Tag FromName(string name) => Monitor.Locked(() =>
   {
      if(ByName.TryGetValue(name, out var existing)) return existing;

      var created = new Tag(name);
      ByName.Add(name, created);
      return created;
   });

   /// <summary>
   /// Why <paramref name="typedName"/>, typed by a person, can't be the name of a tag, or null when it can:<br/>
   /// a tag has no spaces, since Anki separates tags with spaces, and no empty levels, such as in <c>a::::b</c>.
   /// </summary>
   public static string? WhyItCannotBeTheNameOfATag(string typedName) =>
      typedName.Length == 0                                       ? "A tag needs a name."
      : typedName.Any(char.IsWhiteSpace)                          ? "A tag can't contain spaces."
      : typedName.Split(LevelSeparator).Any(string.IsNullOrEmpty) ? $"A tag can't have an empty level, as {typedName} has."
                                                                  : null;
}
