using System;
using System.Threading.Tasks;

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

   /// <summary>Starts <paramref name="asyncWork"/> without waiting for it to finish, and gives an exception it throws to <see cref="Handle"/>.</summary>
   /// <remarks>
   /// For work a UI event starts and nothing awaits, such as the search the search editor runs again after each change.<br/>
   /// An exception from such work would otherwise go unnoticed.
   /// </remarks>
   async void StartAndHandleExceptionsFrom(Func<Task> asyncWork)
   {
      try
      {
         await asyncWork();
      }
      catch(Exception exception)
      {
         Handle(exception);
      }
   }
}
