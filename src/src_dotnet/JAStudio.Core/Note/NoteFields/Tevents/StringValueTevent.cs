namespace JAStudio.Core.Note.NoteFields.Tevents;

/// <summary>The base class of the classes of the <see cref="IStringValueTevent"/>s.</summary>
public abstract class StringValueTevent : IStringValueTevent
{
   public class Set(string value) : StringValueTevent, IStringValueTevent.Set
   {
      public string Value { get; } = value;
   }
}
