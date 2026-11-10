using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// The tags of a note that a person manages (<see cref="NoteTags.NotMaintainedByJAStudio"/>): all but the jastudio-maintained-tags, which are generated<br/>
/// or have editors of their own. Its value is the tags' names in alphabetical order. Writing it leaves the jastudio-maintained-tags as they are.
/// </summary>
public sealed class EditableTagsField<TNote> : EditableField<TNote, IReadOnlyList<string>> where TNote : JPNote
{
   public EditableTagsField() : base("tags", "Tags", changesHowSentencesParse: false) {}

   public override IReadOnlyList<string> Read(TNote note) => [.. note.Tags.NotMaintainedByJAStudio().Select(it => it.Name)];

   /// <summary>Sets the tags named in <paramref name="value"/> and unsets the others a person manages, or, when one can't be such a tag, changes nothing and throws.</summary>
   public override void Write(TNote note, IReadOnlyList<string> value)
   {
      foreach(var name in value) ThrowIfItCannotBeATagAPersonManages(name);
      note.Tags.ReplaceTheTagsNotMaintainedByJAStudio([.. value.Select(Tag.FromName)]);
   }

   static void ThrowIfItCannotBeATagAPersonManages(string name)
   {
      if(Tags.WhyItCannotBeATagAPersonManages(name) is {} why) throw new ArgumentException($"'{name}': {why}");
   }
}
