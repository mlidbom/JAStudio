using System;
using Compze.Tessaging.TessageBus;
using JAStudio.Core;
using JAStudio.Core.TaskRunners;

namespace JAStudio.UI.ViewModels;

/// <summary>
/// The tevents a view model observes, to show what they change: each of its observers' calls is handed from the tevents' observation to the<br/>
/// UI thread, which the view model's state changes on. The view model declares its observers as it makes this (<see cref="Registrar"/>), and<br/>
/// they observe until this is disposed.
/// </summary>
/// <remarks>
/// Compze may start a call of an observer just after its observation's disposing returns, since that doesn't wait for a call already taken<br/>
/// (<see cref="IDynamicTeventObservers.ObserveTevents"/>). Disposed on the UI thread, as a view model is when its view closes, this runs no call after<br/>
/// that: each call checks, on the UI thread, whether it has been disposed.
/// </remarks>
/// <remarks>review-naming: TeventsObservedOnTheUIThread is a name taggregates-opus chose (2026-11-17).</remarks>
public sealed partial class TeventsObservedOnTheUIThread : IDisposable
{
   readonly IUIThreadDispatcher _uiThread;
   readonly IDisposable _observation;
   volatile bool _disposed;

   /// <summary>Observes the tevents <paramref name="observe"/> declares observers of, through the observation <paramref name="services"/> has.</summary>
   public TeventsObservedOnTheUIThread(TemporaryServiceCollection services, Action<Registrar> observe)
   {
      _uiThread = services.Resolve<IUIThreadDispatcher>();
      _observation = services.Resolve<IDynamicTeventObservers>().ObserveTevents(registrar => observe(new Registrar(this, registrar)));
   }

   public void Dispose()
   {
      _disposed = true;
      _observation.Dispose();
   }

   /// <summary>Runs <paramref name="call"/>, an observer's call, on the UI thread, unless this is disposed by then.</summary>
   void OnTheUIThread(Action call) => _uiThread.PostToUIThread(() =>
   {
      if(!_disposed) call();
   });
}
