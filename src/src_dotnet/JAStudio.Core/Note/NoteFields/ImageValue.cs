using System.Collections.Generic;

namespace JAStudio.Core.Note.NoteFields;

public class WritableImageValue
{
   readonly NoteGuard _guard;
   string _value;

   public WritableImageValue(string initialValue, NoteGuard guard)
   {
      _value = initialValue;
      _guard = guard;
   }

   public string RawValue() => _value;

   public List<MediaReference> GetMediaReferences() => MediaFieldParsing.ParseImageReferences(_value);

   public void SetRawValue(string value) => _guard.Update(() => _value = value);

   public override string ToString() => _value;
}
