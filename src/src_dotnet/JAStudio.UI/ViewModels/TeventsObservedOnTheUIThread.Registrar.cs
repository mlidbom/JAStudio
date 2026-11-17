using System;
using System.Collections.Generic;
using Compze.Tessaging.TessageBus;
using Compze.Tessaging.TessageTypes;

namespace JAStudio.UI.ViewModels;

public sealed partial class TeventsObservedOnTheUIThread
{
   /// <summary>
   /// Declares the observers of <see cref="TeventsObservedOnTheUIThread"/>, as Compze's <see cref="TeventObservationRegistrar"/> does, each called<br/>
   /// on the UI thread. It exists only inside the callback it is handed to, as Compze's does.
   /// </summary>
   public sealed class Registrar
   {
      readonly TeventsObservedOnTheUIThread _observed;
      readonly TeventObservationRegistrar _registrar;

      internal Registrar(TeventsObservedOnTheUIThread observed, TeventObservationRegistrar registrar)
      {
         _observed = observed;
         _registrar = registrar;
      }

      /// <summary>Observes each committed tevent compatible with <typeparamref name="TTevent"/> with <paramref name="observer"/>, on the UI thread.</summary>
      public Registrar ForTevent<TTevent>(Action<TTevent> observer) where TTevent : ITevent
      {
         _registrar.ForTevent<TTevent>(tevent => _observed.OnTheUIThread(() => observer(tevent)));
         return this;
      }

      /// <summary>
      /// Observes each committed unit of work's tevents compatible with <typeparamref name="TTevent"/>, as one list in the order they were published,<br/>
      /// with <paramref name="observer"/>, on the UI thread (<see cref="TeventObservationRegistrar.ForEachUnitOfWork{TTevent}(Action{IReadOnlyList{TTevent}, Compze.DependencyInjection.Abstractions.IScopeResolver})"/>).
      /// </summary>
      public Registrar ForEachUnitOfWork<TTevent>(Action<IReadOnlyList<TTevent>> observer) where TTevent : ITevent
      {
         _registrar.ForEachUnitOfWork<TTevent>(tevents => _observed.OnTheUIThread(() => observer(tevents)));
         return this;
      }
   }
}
