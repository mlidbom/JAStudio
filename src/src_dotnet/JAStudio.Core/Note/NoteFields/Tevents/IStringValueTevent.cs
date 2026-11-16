using Compze.Tessaging.TessageTypes;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.NoteFields.Tevents;

/// <summary>
/// Something that happened to a note's text field that a person edits as a whole, such as a vocab's mnemonic (<see cref="WritableStringValue"/>).<br/>
/// A note has several such fields, so a field's tevents carry no note: each field's place in its note adopts them into the note's tevents,<br/>
/// naming the field, as <see cref="IVocabTevent.Mnemonic{TTevent}"/> does. A text field has only its value, so its only tevent says the value changed.
/// </summary>
public interface IStringValueTevent : ITevent
{
   /// <summary>Which property of the field got which value: what the field's tevent appliers apply.</summary>
   public static class PropertyUpdated
   {
      /// <summary>The field's text became <see cref="Value"/>. Never published when the text already was that.</summary>
      public interface Value : IStringValueTevent
      {
         string Value { get; }
      }
   }
}
