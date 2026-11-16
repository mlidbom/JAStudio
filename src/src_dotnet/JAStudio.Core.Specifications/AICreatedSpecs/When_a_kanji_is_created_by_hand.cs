using Compze.Must;
using Compze.Teventive.TaggregateStores;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Tagging;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>A kanji created by hand, as the note menus' "Create" creates one (<see cref="KanjiNote.CreateByHand"/>), is tagged as our own, <see cref="Tags.Source.Jastudio"/>.</summary>
public class When_a_kanji_is_created_by_hand : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly ITaggregateReference<KanjiNote> _kanjiReference;
   KanjiNote _kanji => _kanjiReference.Current;

   public When_a_kanji_is_created_by_hand() => _kanjiReference = KanjiNote.CreateByHand(NoteServices, "⺌", "TODO").Reference();

   [XF] public void it_is_tagged_as_our_own() => _kanji.Tags.Must().Contain(Tags.Source.Jastudio);
}
