namespace JAStudio.Core.Note.NoteFields;

public class SentenceQuestionField
{
   public const string WordBreakTag = "<wbr>";

   readonly IWritableStringValue _userField;
   readonly IWritableStringValue _sourceField;

   public SentenceQuestionField(IWritableStringValue primaryField, IWritableStringValue fallbackField)
   {
      _userField = primaryField;
      _sourceField = fallbackField;
   }

   string SentenceQuestionFieldRawValue()
   {
      var userValue = _userField.Value;
      return string.IsNullOrEmpty(userValue) ? _sourceField.Value : userValue;
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

      if(_userField.HasValue())
      {
         _userField.Set(newValue);
      } else
      {
         _sourceField.Set(newValue);
      }
   }
}
