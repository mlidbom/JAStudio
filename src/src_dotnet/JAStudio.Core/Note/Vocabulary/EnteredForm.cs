namespace JAStudio.Core.Note.Vocabulary;

/// <summary>
/// One of a vocab's forms besides its question, as entered: its <see cref="Text"/>, and whether the vocab owns it.<br/>
/// A vocab owns a form when the text, wherever it's found, is this vocab rather than one of the other vocab that match it.
/// </summary>
/// <remarks>See <see cref="VocabNoteForms.OwnedForms"/> for what owning a form does to matching.</remarks>
public record EnteredForm(string Text, bool IsOwned)
{
   /// <summary>A form the vocab matches without owning it.</summary>
   public static EnteredForm NotOwned(string text) => new(text, IsOwned: false);

   /// <summary>A form the vocab owns.</summary>
   public static EnteredForm Owned(string text) => new(text, IsOwned: true);
}
