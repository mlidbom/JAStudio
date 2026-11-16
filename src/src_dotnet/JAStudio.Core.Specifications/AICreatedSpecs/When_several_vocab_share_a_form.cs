using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A lookup in the collection gives the notes filed under a key in the order of their ids, whatever order the state store files them in, which<br/>
/// it promises nothing about: what a lookup gives is then a function of the notes' state alone, and a full rebuild reproduces the corpus exactly.
/// </summary>
/// <remarks>
/// The eight vocab get random ids as they are created, so the order they are created and filed in matches the order of their ids only once in<br/>
/// 40,320 runs.
/// </remarks>
public class When_several_vocab_share_a_form : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   const string TheSharedForm = "共有";

   readonly IReadOnlyList<NoteId> _idsInTheOrderTheyWereCreated;

   public When_several_vocab_share_a_form() =>
      _idsInTheOrderTheyWereCreated = [.. new[] { "一", "二", "三", "四", "五", "六", "七", "八" }.Select(question => CreateVocab(question, "a word", [], [TheSharedForm]).GetId())];

   [XF] public void looking_them_up_by_the_form_gives_every_one() =>
      NoteServices.Collection.Vocab.WithForm(TheSharedForm).Select(it => it.GetId().Value).Order().Must().SequenceEqual(_idsInTheOrderTheyWereCreated.Select(it => it.Value).Order());

   [XF] public void looking_them_up_by_the_form_gives_them_in_the_order_of_their_ids() =>
      NoteServices.Collection.Vocab.WithForm(TheSharedForm).Select(it => it.GetId().Value).Must().SequenceEqual(_idsInTheOrderTheyWereCreated.Select(it => it.Value).Order());
}
