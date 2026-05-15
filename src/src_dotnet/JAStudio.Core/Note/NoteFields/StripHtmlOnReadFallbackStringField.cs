namespace JAStudio.Core.Note.NoteFields;

public class StripHtmlOnReadFallbackStringField
{
   readonly FallbackStringField _field;

   public StripHtmlOnReadFallbackStringField(IStringValue primaryField, IStringValue fallbackField) => _field = new FallbackStringField(primaryField, fallbackField);

   public string Get() => StringExtensions.StripHtmlMarkup(_field.Get().Replace("<wbr>", StringExtensions.InvisibleSpace.ToString()));
}
