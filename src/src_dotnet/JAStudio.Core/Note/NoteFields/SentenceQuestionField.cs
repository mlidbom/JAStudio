using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Note.NoteFields;

/// <summary>
/// The question a sentence shows: its <see cref="SentenceNote.CorpusQuestion"/>, or its source's question while it has none, with the word-break<br/>
/// tags that split a word for the tokenizer shown as invisible spaces, or left out.
/// </summary>
public class SentenceQuestionField
{
   public const string WordBreakTag = "<wbr>";

   readonly SentenceNote _sentence;

   public SentenceQuestionField(SentenceNote sentence) => _sentence = sentence;

   string SentenceQuestionFieldRawValue() => _sentence.CorpusQuestion is not "" ? _sentence.CorpusQuestion : _sentence.SourceData.Question;

   public string WithInvisibleSpace() =>
      StringExtensions.StripHtmlMarkup(
         SentenceQuestionFieldRawValue().Replace(WordBreakTag, StringExtensions.InvisibleSpace.ToString()));

   public string WithoutInvisibleSpace() => WithInvisibleSpace().Replace(StringExtensions.InvisibleSpace.ToString(), string.Empty);

   public void SplitTokenWithWordBreakTag(string section)
   {
      if(section.Length < 2)
      {
         return;
      }

      var rawValue = SentenceQuestionFieldRawValue();
      var newSection = $"{section[0]}{WordBreakTag}{section.Substring(1)}";
      var newValue = rawValue.Replace(section, newSection);

      if(_sentence.CorpusQuestion is not "")
      {
         _sentence.SetCorpusQuestion(newValue);
      } else
      {
         _sentence.SourceData.SetQuestion(newValue);
      }
   }
}
