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

   public T Value
   {
      get => _value;
      set
      {
         _value = value;

         _configDict[Name] = value!;

         _changed.OnNext(value);
      }
   }

   /// <summary>Notifies the new <see cref="Value"/> every time it is set, on the thread that set it.</summary>
   public IObservable<T> Changed => _changed;
}
