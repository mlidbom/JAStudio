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
      DisambiguationName = value;
      Raw = RawFromDisambiguationName(value);
   }

   /// <summary>
   /// The <see cref="Raw"/> question for a <see cref="DisambiguationName"/>: the question without its disambiguation, as in <c>question:disambiguation</c>.<br/>
   /// <see cref="InvalidQuestionMessage"/> if it has more than one <see cref="DisambiguationMarker"/>, and <c>[EMPTY]</c> if it's empty.
   /// </summary>
   internal static string RawFromDisambiguationName(string disambiguationName)
   {
      var raw = !disambiguationName.Contains(DisambiguationMarker)
                   ? disambiguationName
                   : disambiguationName.Split(DisambiguationMarker) is [var question, _]
                      ? question
                      : InvalidQuestionMessage;

      return string.IsNullOrEmpty(raw) ? "[EMPTY]" : raw;
   }

   public bool IsValid => Raw != InvalidQuestionMessage;
   public bool IsDisambiguated => DisambiguationName.Contains(DisambiguationMarker);

   public string WithoutNoiseCharacters => Raw.Replace(Mine.VocabPrefixSuffixMarker, "");

   public VocabStems Stems() => new(_vocab);

   /// <summary>Sets the question. The question is always one of the vocab's forms, so the old question stops being one.</summary>
   public void Set(string value) => _guard.Update(() =>
   {
      InitValueRaw(value);
      _vocab.Forms.OnQuestionChanged();
   });

   public override string ToString() => Raw;
}
