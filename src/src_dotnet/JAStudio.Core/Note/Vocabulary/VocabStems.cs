using JAStudio.Core.LanguageServices;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabStems
{
   readonly VocabNote _vocab;

   public VocabStems(VocabNote vocab) => _vocab = vocab;

   public string? MasuStem()
   {
      var masuStem = Conjugator.GetIStemVocab(_vocab);
      return masuStem != _vocab.Question.Text ? masuStem : null;
   }
}
