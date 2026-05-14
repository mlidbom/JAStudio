using System;

namespace JAStudio.PythonInterop.Utilities;

public class PythonObjectWrapper(dynamic pythonObject)
{
   readonly dynamic _pythonObject = pythonObject;

   public TValue Use<TValue>(Func<dynamic, TValue> func) => PythonEnvironment.Use(() => func(_pythonObject));
   // ReSharper disable once UnusedMember.Global
   public void Use(Action<dynamic> func) => PythonEnvironment.Use(() => func(_pythonObject));
}
