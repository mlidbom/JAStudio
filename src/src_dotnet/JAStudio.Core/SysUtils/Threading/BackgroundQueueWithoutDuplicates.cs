using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.SysUtils.Threading;

/// <summary>
/// Processes the items added to it in the background, one at a time, in the order they were added. An item added again while it waits is processed once;<br/>
/// one added again while it's being processed is processed again afterwards.
/// </summary>
class BackgroundQueueWithoutDuplicates<TItem>(Action<TItem> process, BackgroundTaskManager backgroundTaskManager) where TItem : class
{
   readonly Action<TItem> _process = process;
   readonly BackgroundTaskManager _backgroundTaskManager = backgroundTaskManager;
   readonly IMonitor _monitor = IMonitor.New();
   readonly List<TItem> _waitingItems = [];
   bool _theWaitingItemsAreBeingProcessed;

   /// <summary>Adds the <paramref name="items"/> that aren't waiting already, and starts processing them unless the waiting items are being processed.</summary>
   public void Add(IEnumerable<TItem> items)
   {
      var itemsToAdd = items.ToList();
      var startsProcessingThem = _monitor.Locked(() =>
      {
         _waitingItems.AddRange(itemsToAdd.Except(_waitingItems));
         if(!ItemsWaitAndAreNotBeingProcessed) return false;

         _theWaitingItemsAreBeingProcessed = true;
         return true;
      });
      if(startsProcessingThem) _backgroundTaskManager.Run(ProcessTheWaitingItems); // Outside the lock, since the background task manager takes a lock of its own.
   }

   bool ItemsWaitAndAreNotBeingProcessed => _waitingItems.Count > 0 && !_theWaitingItemsAreBeingProcessed;

   /// <summary>Processes the waiting items until none waits. An item whose processing fails stops it, and the next <see cref="Add"/> starts it again.</summary>
   void ProcessTheWaitingItems()
   {
      try
      {
         while(TheNextWaitingItem() is {} item)
            _process(item);
      }
      catch
      {
         _monitor.Locked(() => _theWaitingItemsAreBeingProcessed = false);
         throw;
      }
   }

   /// <summary>Takes the next waiting item, or null when none waits, which ends the processing of the waiting items.</summary>
   TItem? TheNextWaitingItem() => _monitor.Locked(() =>
   {
      if(_waitingItems.Count == 0)
      {
         _theWaitingItemsAreBeingProcessed = false;
         return null;
      }

      var next = _waitingItems[0];
      _waitingItems.RemoveAt(0);
      return next;
   });
}
