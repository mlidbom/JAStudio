namespace JAStudio.Core.Note.NoteFields;

/// <summary>A text, <paramref name="primary"/>, or while it's empty, another, <paramref name="fallback"/>: such as the corpus's answer, else the source's.</summary>
class FallbackStringField(string primary, string fallback)
{
   public string Get() => primary is not "" ? primary : fallback;
}
