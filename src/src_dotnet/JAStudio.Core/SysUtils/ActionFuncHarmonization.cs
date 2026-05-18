using System;

namespace JAStudio.Core.SysUtils;

/// <summary>
/// Trivial shim replacing the removed <c>Compze.Utilities.SystemCE.ActionFuncHarmonization</c> namespace.
/// Lets <see cref="Action"/>/<see cref="Action{T}"/> overloads forward to <see cref="Func{TResult}"/>/<see cref="Func{T,TResult}"/> overloads
/// in <see cref="JAStudio.Core.TaskRunners.ITaskProgressRunner"/> without duplicating each method body.
/// </summary>
public static class ActionFuncHarmonization
{
   public static Func<int> AsFunc(this Action @this) => () => { @this(); return 0; };
   public static Func<TInput, int> AsFunc<TInput>(this Action<TInput> @this) => x => { @this(x); return 0; };
}
