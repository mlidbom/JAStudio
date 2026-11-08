namespace JAStudio.Core.Note.NoteFields;

public class SentenceQuestionField
{
   public const string WordBreakTag = "<wbr>";

   readonly IWritableStringValue _corpusQuestion;
   readonly IWritableStringValue _sourceQuestion;

   public SentenceQuestionField(IWritableStringValue corpusQuestion, IWritableStringValue sourceQuestion)
   {
      _corpusQuestion = corpusQuestion;
      _sourceQuestion = sourceQuestion;
   }

   string SentenceQuestionFieldRawValue()
   {
      var corpusQuestion = _corpusQuestion.Value;
      return string.IsNullOrEmpty(corpusQuestion) ? _sourceQuestion.Value : corpusQuestion;
   }

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

      if(_corpusQuestion.HasValue())
      {
         _corpusQuestion.Set(newValue);
      } else
      {
         _sourceQuestion.Set(newValue);
      }
   }
}
