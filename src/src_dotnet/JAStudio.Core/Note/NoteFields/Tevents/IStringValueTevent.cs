using Compze.Tessaging.TessageTypes;
using JAStudio.Core.Note.Vocabulary.Tevents;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Note.NoteFields.Tevents;

/// <summary>
/// The tevents of a note's text field (<see cref="WritableStringValue"/>), a shared tomponent: they carry no note, and each of the field's places<br/>
/// in a note adopts them under its own name, as <see cref="IVocabTevent.Mnemonic{TTevent}"/> does.
/// </summary>
public interface IStringValueTevent : ITevent
{
   public static class PropertyUpdated
   {
      public interface Value : IStringValueTevent
      {
         string Value { get; }
      }
   }
}
