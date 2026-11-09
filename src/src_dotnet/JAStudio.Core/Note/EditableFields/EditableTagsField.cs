using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// The tags of a note that a person manages (<see cref="NoteTags.NotDefinedByJAStudio"/>): all but the jastudio-defined-tags, which are generated<br/>
/// or have editors of their own. Its value is the tags' names in alphabetical order. Writing it leaves the jastudio-defined-tags as they are.
/// </summary>
public sealed class EditableTagsField<TNote> : EditableField<TNote, IReadOnlyList<string>> where TNote : JPNote
{
   public EditableTagsField() : base("tags", "Tags", changesHowSentencesParse: false) {}

   public override IReadOnlyList<string> Read(TNote note) => [.. note.Tags.NotDefinedByJAStudio().Select(it => it.Name)];

   /// <summary>Sets the tags named in <paramref name="value"/> and unsets the others a person manages, or, when one can't be such a tag, changes nothing and throws.</summary>
   public override void Write(TNote note, IReadOnlyList<string> value)
   {
      foreach(var name in value) ThrowIfItCannotBeATagAPersonManages(name);
      note.Tags.ReplaceTheTagsNotDefinedByJAStudio([.. value.Select(Tag.FromName)]);
   }

   static void ThrowIfItCannotBeATagAPersonManages(string name)
   {
      //refactor: TagPickerViewModel.CanBeANewTag checks the same two rules. One method in Core, such as Tags.WhyItCannotBeATagAPersonManages, should say why for both.
      if(Tag.WhyItCannotBeTheNameOfATag(name) is {} why) throw new ArgumentException($"'{name}': {why}");
      if(Tags.IsDefinedByJAStudio(name)) throw new ArgumentException($"'{name}' is a jastudio-defined-tag, which JAStudio generates, or which has an editor of its own.");
   }
}
