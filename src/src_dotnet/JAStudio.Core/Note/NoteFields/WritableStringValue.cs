using System;
using Compze.Internals.SystemCE.ReactiveCE;

namespace JAStudio.Core.Note.NoteFields;

public class WritableStringValue : IWritableStringValue, IObservable<string>
{
   readonly NoteGuard _guard;
   readonly SimpleObservable<string> _observable = new();

   public WritableStringValue(string initialValue, NoteGuard guard)
   {
      Value = initialValue;
      _guard = guard;
   }

   public string Value { get; private set; }

   public void Set(string value)
   {
      var trimmed = value.Trim();
      if(trimmed != Value)
      {
         _guard.Update(() =>
         {
            Value = trimmed;
            _observable.OnNext(trimmed);
         });
      }
   }

   public void Empty() => Set(string.Empty);

   public bool HasValue() => !string.IsNullOrEmpty(Value);

   public IDisposable Subscribe(IObserver<string> observer) => _observable.Subscribe(observer);

   public override string ToString() => Value;
}
