using System;

namespace JAStudio.Core.TaskRunners;

/// <summary>Shows the user an exception nothing else handled, such as one thrown by a background task.</summary>
public interface IFatalErrorHandler
{
   void Handle(Exception exception);

   /// <summary>Runs <paramref name="action"/>, and gives an exception it throws to <see cref="Handle"/> instead of letting it propagate.</summary>
   /// <remarks>
   /// For code whose caller would swallow the exception.<br/>
   /// Avalonia's bindings do that to a view model's property setter: <c>BindingExpression.WriteValueToSource</c> catches every exception the setter throws<br/>
   /// and silently drops the change. So a setter that writes to a note runs the write through this, and its exceptions reach the user.
   /// </remarks>
   void HandleExceptionsFrom(Action action)
   {
      try
      {
         action();
      }
      catch(Exception exception)
      {
         Handle(exception);
      }
   }
}
