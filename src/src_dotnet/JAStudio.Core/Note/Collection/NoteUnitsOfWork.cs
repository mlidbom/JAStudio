using System;
using System.Threading;
using System.Transactions;
using Compze.Contracts;
using Compze.Contracts.Exceptions;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.Teventive.TaggregateStores;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// Runs the units of work in which JAStudio changes notes (<see cref="NoteUnitOfWork"/>): each in a unit of work of the state store's, which a<br/>
/// unit of work begun while it runs joins, such as one editing another note. Once the outermost has committed, what was to be done then is done<br/>
/// (<see cref="NoteUnitOfWork.OnceCommitted"/>), on the thread that ran it.
/// </summary>
/// <remarks>
/// A unit of work of the state store's that JAStudio didn't begin, such as one a Compze tessage handler begins, can't be joined: what was to be done<br/>
/// once the notes changed in it are committed would be done by no one. So changing a note in one fails.
/// </remarks>
/// <remarks>review-naming: NoteUnitsOfWork, after Compze's TaggregateUnitsOfWork, is a name taggregates-opus chose (2026-11-16).</remarks>
sealed class NoteUnitsOfWork
{
   readonly Lazy<IScopeFactory> _scopes;

   /// <summary>The unit of work running, if one is.</summary>
   /// <remarks>
   /// It flows with .NET's execution context, as the unit of work's transaction does: a Compze tevent handler runs inside the caller's unit of<br/>
   /// work, but not always on its thread, since an asynchronous handler before it in the dispatch moves it to another (jastudio-rep-opus, 2026-11-17).
   /// </remarks>
   readonly AsyncLocal<NoteUnitOfWork?> _running = new();

   /// <param name="resolver">Resolves the state store's units of work, at their first use.</param>
   public NoteUnitsOfWork(IRootResolver resolver) => _scopes = new Lazy<IScopeFactory>(resolver.Resolve<IScopeFactory>);

   /// <summary>Runs <paramref name="work"/> in the unit of work running, or else in a new one, which commits once it is done.</summary>
   public void Run(Action<NoteUnitOfWork> work)
   {
      if(_running.Value is {} running)
      {
         work(running);
         return;
      }

      Contract.State.Assert(Transaction.Current == null, () => "A note is changed in a unit of work JAStudio didn't begin, which would do nothing of what is to be done once it has committed: change notes only in JAStudio's units of work, such as note.Edit(it => ...).");
      var unitOfWork = _scopes.Value.ExecuteUnitOfWork(unitOfWorkResolver =>
      {
         var begun = new NoteUnitOfWork(unitOfWorkResolver.Resolve<ITaggregateStoreUpdater>());
         _running.Value = begun;
         try
         {
            work(begun);
         }
         finally
         {
            _running.Value = null;
         }

         return begun;
      });
      unitOfWork.Committed();
   }

   /// <summary>The unit of work running. Fails when none is.</summary>
   public NoteUnitOfWork Running => _running.Value ?? throw new StateAssertionFailedException("No unit of work of JAStudio's is running: a note is got for update only in one, such as note.Edit(it => ...).");
}
