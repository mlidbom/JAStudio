namespace JAStudio.Core.Note.NoteFields.Tevents;

/// <summary>The classes of the <see cref="IStringValueTevent"/>s.</summary>
public abstract class StringValueTevent : IStringValueTevent
{
   public class ValueUpdated(string value) : StringValueTevent, IStringValueTevent.PropertyUpdated.Value
   {
      public string Value { get; } = value;
   }
}
