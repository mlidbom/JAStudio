using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Vocabulary;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Analysis;

/// <summary>
/// 唱える after を, as in 呪文を唱える, could be read as the potential form of the godan verb 唱う, which here is a form of three vocab: 唄う and 謡う,<br/>
/// transitive, and 歌う, which can also be intransitive. An intransitive verb takes no object marked by を, so the analysis keeps the ichidan verb<br/>
/// whole when the godan verb can be intransitive: when any of the vocab it is a form of can be, whichever of them comes first.
/// </summary>
/// <remarks>
/// The vocab's ids are new each time, so their order is too. When the first of them decided, as it once did, this failed whenever 歌う wasn't the<br/>
/// first: two times in three.
/// </remarks>
public class When_analyzing_呪文を唱える_with_唱う_a_form_of_several_godan_vocab : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   readonly List<string> _surfacesOfTheCandidateWords;

   public When_analyzing_呪文を唱える_with_唱う_a_form_of_several_godan_vocab()
   {
      CreateVerb("唱える", "to recite", "となえる", [], [PartOfSpeech.IchidanVerb, PartOfSpeech.Transitive]);
      CreateVerb("唄う", "to sing", "うたう", ["唱う"], [PartOfSpeech.GodanVerb, PartOfSpeech.Transitive]);
      CreateVerb("謡う", "to chant", "うたう", ["唱う"], [PartOfSpeech.GodanVerb, PartOfSpeech.Transitive]);
      CreateVerb("歌う", "to sing", "うたう", ["唱う"], [PartOfSpeech.GodanVerb, PartOfSpeech.Intransitive, PartOfSpeech.Transitive]);
      _surfacesOfTheCandidateWords = [.. CreateSentence("呪文を唱える").CreateAnalysis().Locations.SelectMany(it => it.CandidateWords).Select(it => it.SurfaceForm)];
   }

   [XF] public void a_candidate_word_is_唱える_whole() => _surfacesOfTheCandidateWords.Must().Contain("唱える");

   [XF] public void no_candidate_word_is_唱_the_stem_of_the_godan_verb() => _surfacesOfTheCandidateWords.Must().Satisfy(it => !it.Contains("唱"));

   void CreateVerb(string question, string answer, string reading, List<string> forms, List<PartOfSpeech> partsOfSpeech) =>
      VocabNote.Create(NoteServices, question, answer, [reading], forms).Edit(it => it.PartsOfSpeech.Set(partsOfSpeech));
}
