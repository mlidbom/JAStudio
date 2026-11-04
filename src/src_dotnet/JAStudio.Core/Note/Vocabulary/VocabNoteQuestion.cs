using JAStudio.Core.LanguageServices;
using JAStudio.Core.Note.CorpusData;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabStems
{
   readonly VocabNote _vocab;

   public VocabStems(VocabNote vocab) => _vocab = vocab;

   public string? MasuStem()
   {
      var masuStem = Conjugator.GetIStemVocab(_vocab);
      return masuStem != _vocab.Question.Raw ? masuStem : null;
   }
}

public class VocabNoteQuestion
{
   public const string DisambiguationMarker = ":";
   public const string InvalidQuestionMessage = "INVALID QUESTION FORMAT. If you need to specify disambiguation, use question:disambiguation if not do NOT use : characters. More than one is invalid";

   readonly VocabNote _vocab;
   readonly NoteGuard _guard;
   public string Raw { get; private set; }
   public string DisambiguationName { get; private set; }

   public VocabNoteQuestion(VocabNote vocab, VocabData? data, NoteGuard guard)
   {
      _vocab = vocab;
      _guard = guard;
      Raw = string.Empty;
      DisambiguationName = string.Empty;
      InitValueRaw(data?.Question ?? string.Empty);
   }

   void InitValueRaw(string value)
   {
      if(value.Contains(DisambiguationMarker))
      {
         DisambiguationName = value;
         var parts = DisambiguationName.Split(DisambiguationMarker);

         Raw = parts.Length == 2 ? parts[0] : InvalidQuestionMessage;
      } else
      {
         Raw = value;
         DisambiguationName = value;
      }

      if(string.IsNullOrEmpty(Raw))
      {
         Raw = "[EMPTY]";
      }
   }

   public bool IsValid => Raw != InvalidQuestionMessage;
   public bool IsDisambiguated => DisambiguationName.Contains(DisambiguationMarker);

   public string WithoutNoiseCharacters => Raw.Replace(Mine.VocabPrefixSuffixMarker, "");

   public VocabStems Stems() => new VocabStems(_vocab);

   public void Set(string value) => _guard.Update(() =>
   {
      InitValueRaw(value);
      _vocab.Forms.EnsureContains(Raw);
   });

   public override string ToString() => Raw;
}
