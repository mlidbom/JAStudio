using System;
using System.Collections.Generic;
using Compze.Internals.SystemCE.ReactiveCE;

namespace JAStudio.Core.Configuration;

public class ConfigurationValue<T>
{
   T _value;
   readonly Dictionary<string, object> _configDict;
   readonly SimpleObservable<T> _changed = new();

   public string Title { get; }
   public string Name { get; }

   public ConfigurationValue(string name, string title, T defaultValue, Func<object, T> converter, Dictionary<string, object> configDict)
   {
      Title = title;
      Name = name;
      _configDict = configDict;

      _value = configDict.TryGetValue(name, out var value)
                  ? converter(value)
                  : defaultValue;
   }

   /// <summary>The value. Setting it to the value it already has changes nothing: nothing is saved, and <see cref="Changed"/> notifies nothing.</summary>
   /// <remarks>
   /// The options dialog sets every value as it opens, to show them. Until 2026-11-09 each set was a change, so opening it saved the configuration<br/>
   /// once for each value, and every page open in the workspace reloaded as many times, since pages reload when the configuration changes.
   /// </remarks>
   public T Value
   {
      get => _value;
      set
      {
         if(EqualityComparer<T>.Default.Equals(_value, value)) return;

         _value = value;

         _configDict[Name] = value!;

         _changed.OnNext(value);
      }
   }

   /// <summary>Notifies the new <see cref="Value"/> every time it changes, on the thread that changed it.</summary>
   public IObservable<T> Changed => _changed;
}
