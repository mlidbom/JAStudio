using System;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;

/// <summary>
/// One setting of a sample vocab's <see cref="VocabNoteMatchingConfiguration"/>, such as requiring a godan verb's stem before it,<br/>
/// as a <see cref="VocabSpec"/> lists them: <c>matching: [IsInflectingWord, Requires.TeFormStemStart]</c>.
/// </summary>
public class VocabMatchingSetting
{
   readonly Action<VocabNoteMatchingConfiguration> _applyTo;

   VocabMatchingSetting(Action<VocabNoteMatchingConfiguration> applyTo) => _applyTo = applyTo;

   public void ApplyTo(VocabNoteMatchingConfiguration configuration) => _applyTo(configuration);

   public static readonly VocabMatchingSetting IsInflectingWord = Flag(it => it.IsInflectingWord);
   public static readonly VocabMatchingSetting IsPoisonWord = Flag(it => it.IsPoisonWord);
   public static readonly VocabMatchingSetting QuestionOverridesForm = Flag(it => it.QuestionOverridesForm);
   public static readonly VocabMatchingSetting IsCompositionallyTransparentCompound = Flag(it => it.IsCompositionallyTransparentCompound);
   public static readonly VocabMatchingSetting YieldLastTokenToOverlappingCompound = Choose(RequireForbidChoice.Required, it => it.YieldLastToken);

   public static class Requires
   {
      public static readonly VocabMatchingSetting MasuStem = Required(it => it.MasuStem);
      public static readonly VocabMatchingSetting Godan = Required(it => it.Godan);
      public static readonly VocabMatchingSetting Ichidan = Required(it => it.Ichidan);
      public static readonly VocabMatchingSetting Irrealis = Required(it => it.Irrealis);
      public static readonly VocabMatchingSetting PastTenseStem = Required(it => it.PastTenseStem);
      public static readonly VocabMatchingSetting DictionaryFormStart = Required(it => it.DictionaryFormStart);
      public static readonly VocabMatchingSetting DictionaryFormHead = Required(it => it.DictionaryFormHead);
      public static readonly VocabMatchingSetting TeFormStemStart = Required(it => it.TeFormStemStart);
      public static readonly VocabMatchingSetting TeFormStemHead = Required(it => it.TeFormStemHead);
      public static readonly VocabMatchingSetting IchidanImperative = Required(it => it.IchidanImperative);
      public static readonly VocabMatchingSetting GodanPotential = Required(it => it.GodanPotential);
      public static readonly VocabMatchingSetting GodanImperative = Required(it => it.GodanImperative);
      public static readonly VocabMatchingSetting SingleToken = Required(it => it.SingleToken);
      public static readonly VocabMatchingSetting SentenceEnd = Required(it => it.SentenceEnd);
      public static readonly VocabMatchingSetting SentenceStart = Required(it => it.SentenceStart);
      public static readonly VocabMatchingSetting Surface = Required(it => it.Surface);

      static VocabMatchingSetting Required(Func<VocabMatchingRequireForbidFlags, RequireForbidFlagField> rule) => Choose(RequireForbidChoice.Required, rule);
   }

   public static class Forbids
   {
      public static readonly VocabMatchingSetting Irrealis = Forbidden(it => it.Irrealis);
      public static readonly VocabMatchingSetting AdverbHead = Forbidden(it => it.AdverbHead);
      public static readonly VocabMatchingSetting PastTenseStem = Forbidden(it => it.PastTenseStem);
      public static readonly VocabMatchingSetting DictionaryFormStart = Forbidden(it => it.DictionaryFormStart);
      public static readonly VocabMatchingSetting TeFormStemStart = Forbidden(it => it.TeFormStemStart);
      public static readonly VocabMatchingSetting TeFormStemHead = Forbidden(it => it.TeFormStemHead);
      public static readonly VocabMatchingSetting IchidanImperative = Forbidden(it => it.IchidanImperative);
      public static readonly VocabMatchingSetting GodanPotential = Forbidden(it => it.GodanPotential);
      public static readonly VocabMatchingSetting GodanImperative = Forbidden(it => it.GodanImperative);
      public static readonly VocabMatchingSetting SentenceEnd = Forbidden(it => it.SentenceEnd);
      public static readonly VocabMatchingSetting SentenceStart = Forbidden(it => it.SentenceStart);

      static VocabMatchingSetting Forbidden(Func<VocabMatchingRequireForbidFlags, RequireForbidFlagField> rule) => Choose(RequireForbidChoice.Forbidden, rule);
   }

   static VocabMatchingSetting Flag(Func<VocabMatchingFlags, FlagField> flag) => new(it => flag(it.Flags).SetTo(true));

   static VocabMatchingSetting Choose(RequireForbidChoice choice, Func<VocabMatchingRequireForbidFlags, RequireForbidFlagField> rule) => new(it => rule(it.RequiresForbids).Choose(choice));
}
