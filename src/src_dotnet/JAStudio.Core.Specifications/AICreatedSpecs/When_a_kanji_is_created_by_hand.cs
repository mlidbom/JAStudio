using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A kanji created by hand, as the note menus' "Create" creates one (<see cref="KanjiNote.CreateByHand"/>), is tagged as our own, <see cref="Tags.Source.Jastudio"/>.</summary>
public class When_a_kanji_is_created_by_hand : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly KanjiNote _kanji;

   public When_a_kanji_is_created_by_hand() => _kanji = KanjiNote.CreateByHand(NoteServices, "⺌", "TODO");

   [XF] public void it_is_tagged_as_our_own() => _kanji.Tags.Contains(Tags.Source.Jastudio).Must().BeTrue();
}
