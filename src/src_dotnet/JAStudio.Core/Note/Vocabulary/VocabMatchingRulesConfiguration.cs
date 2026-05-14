using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabMatchingRulesConfigurationRequiresForbidsFlags
{
   readonly VocabNote _vocab;
   readonly List<RequireForbidFlagField> _allFlags = [];

   public RequireForbidFlagField MasuStem { get; }
   public RequireForbidFlagField Godan { get; }
   public RequireForbidFlagField Ichidan { get; }
   public RequireForbidFlagField Irrealis { get; }
   public RequireForbidFlagField PrecedingAdverb { get; }
   public RequireForbidFlagField PastTenseStem { get; }
   public RequireForbidFlagField DictionaryFormStem { get; }
   public RequireForbidFlagField DictionaryFormPrefix { get; }
   public RequireForbidFlagField TeFormStem { get; }
   public RequireForbidFlagField TeFormPrefix { get; }
   public RequireForbidFlagField IchidanImperative { get; }
   public RequireForbidFlagField GodanPotential { get; }
   public RequireForbidFlagField GodanImperative { get; }
   public RequireForbidFlagField GodanImperativePrefix { get; }
   public RequireForbidFlagField SingleToken { get; }
   public RequireForbidFlagField SentenceEnd { get; }
   public RequireForbidFlagField SentenceStart { get; }
   public RequireForbidFlagField Surface { get; }
   public YieldLastTokenToOverlappingCompound YieldLastToken { get; }

   public VocabMatchingRulesConfigurationRequiresForbidsFlags(VocabNote vocab)
   {
      _vocab = vocab;

      MasuStem = AddFlag(50, 1, Tags.Vocab.Matching.Requires.MasuStem, Tags.Vocab.Matching.Forbids.MasuStem);
      Godan = AddFlag(50, 1, Tags.Vocab.Matching.Requires.Godan, Tags.Vocab.Matching.Forbids.Godan);
      Ichidan = AddFlag(50, 1, Tags.Vocab.Matching.Requires.Ichidan, Tags.Vocab.Matching.Forbids.Ichidan);
      Irrealis = AddFlag(50, 1, Tags.Vocab.Matching.Requires.Irrealis, Tags.Vocab.Matching.Forbids.Irrealis);
      PrecedingAdverb = AddFlag(30, 1, Tags.Vocab.Matching.Requires.PrecedingAdverb, Tags.Vocab.Matching.Forbids.PrecedingAdverb);
      PastTenseStem = AddFlag(30, 1, Tags.Vocab.Matching.Requires.PastTenseStem, Tags.Vocab.Matching.Forbids.PastTenseStem);
      DictionaryFormStem = AddFlag(30, 1, Tags.Vocab.Matching.Requires.DictionaryFormStem, Tags.Vocab.Matching.Forbids.DictionaryFormStem);
      DictionaryFormPrefix = AddFlag(30, 1, Tags.Vocab.Matching.Requires.DictionaryFormPrefix, Tags.Vocab.Matching.Forbids.DictionaryFormPrefix);
      TeFormStem = AddFlag(20, 1, Tags.Vocab.Matching.Requires.TeFormStem, Tags.Vocab.Matching.Forbids.TeFormStem);
      TeFormPrefix = AddFlag(20, 1, Tags.Vocab.Matching.Requires.TeFormPrefix, Tags.Vocab.Matching.Forbids.TeFormPrefix);
      IchidanImperative = AddFlag(30, 1, Tags.Vocab.Matching.Requires.IchidanImperative, Tags.Vocab.Matching.Forbids.IchidanImperative);
      GodanPotential = AddFlag(30, 1, Tags.Vocab.Matching.Requires.GodanPotential, Tags.Vocab.Matching.Forbids.GodanPotential);
      GodanImperative = AddFlag(30, 1, Tags.Vocab.Matching.Requires.GodanImperative, Tags.Vocab.Matching.Forbids.GodanImperative);
      GodanImperativePrefix = AddFlag(30, 1, Tags.Vocab.Matching.Requires.GodanImperativePrefix, Tags.Vocab.Matching.Forbids.GodanImperativePrefix);
      SingleToken = AddFlag(30, 1, Tags.Vocab.Matching.Requires.SingleToken, Tags.Vocab.Matching.Requires.Compound);
      SentenceEnd = AddFlag(10, 1, Tags.Vocab.Matching.Requires.SentenceEnd, Tags.Vocab.Matching.Forbids.SentenceEnd);
      SentenceStart = AddFlag(10, 1, Tags.Vocab.Matching.Requires.SentenceStart, Tags.Vocab.Matching.Forbids.SentenceStart);
      Surface = AddFlag(10, 1, Tags.Vocab.Matching.Requires.Surface, Tags.Vocab.Matching.Forbids.Surface);
      YieldLastToken = new YieldLastTokenToOverlappingCompound(vocab);
      _allFlags.Add(YieldLastToken);
   }

   RequireForbidFlagField AddFlag(int requiredWeight, int forbiddenWeight, Tag requiredTag, Tag forbiddenTag)
   {
      var flag = new RequireForbidFlagField(_vocab, requiredWeight, forbiddenWeight, requiredTag, forbiddenTag);
      _allFlags.Add(flag);
      return flag;
   }

   public int MatchWeight => _allFlags.Sum(f => f.MatchWeight);
}

public class VocabMatchingRulesConfigurationBoolFlags
{
   readonly VocabNote _vocab;
   public IsInflectingWord IsInflectingWord { get; }

   public VocabMatchingRulesConfigurationBoolFlags(VocabNote vocab)
   {
      _vocab = vocab;
      IsInflectingWord = new IsInflectingWord(vocab);
   }

   public TagFlagField IsPoisonWord => new(_vocab, Tags.Vocab.Matching.IsPoisonWord);
   public TagFlagField MatchWithPrecedingVowel => new(_vocab, Tags.Vocab.Matching.Todo.WithPrecedingVowel);
   public TagFlagField QuestionOverridesForm => new(_vocab, Tags.Vocab.QuestionOverridesForm);
   public TagFlagField IsCompositionallyTransparentCompound => new(_vocab, Tags.Vocab.IsCompositionallyTransparentCompound);
}
