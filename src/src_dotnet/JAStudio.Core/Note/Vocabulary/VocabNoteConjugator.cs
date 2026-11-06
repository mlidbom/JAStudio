using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteConjugator
{
   public VocabNoteConjugator(VocabNote vocab) => Vocab = vocab;

   VocabNote Vocab { get; }

   List<string> GetStemsForForm(string form)
   {
      return Conjugator.GetWordStems(form, Vocab.PartsOfSpeech.IsIchidan(), Vocab.PartsOfSpeech.IsGodan())
                       .Where(stem => stem != form)
                       .ToList();
   }

   public List<string> GetStemsForPrimaryForm() =>
      GetStemsForForm(Vocab.GetQuestion())
        .Distinct()
        .ToList();

   public List<string> GetStemsForAllForms() =>
      Vocab.Forms.MatchedFormsSet()
           .SelectMany(GetStemsForForm)
           .Distinct()
           .ToList();
}
