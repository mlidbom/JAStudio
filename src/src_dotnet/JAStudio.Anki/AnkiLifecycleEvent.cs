namespace JAStudio.Anki;

/// <summary>
/// What happens in Anki that decides whether JAStudio may use the collection Anki has open: a profile opening or closing, and a sync starting or<br/>
/// finishing. Python passes each on from Anki's hooks to <c>JAStudioAnkiAppRoot.HandleAnkiLifecycleEvent</c>.
/// </summary>
public enum AnkiLifecycleEvent
{
   /// <summary>An Anki profile opened, and its collection with it: JAStudio may use it, and catches up with it.</summary>
   ProfileOpened,

   /// <summary>The Anki profile is about to close, and its collection with it: JAStudio may no longer use it.</summary>
   ProfileClosing,

   /// <summary>A sync is about to start: JAStudio may not use the collection until it finishes, since the sync holds it, and a full sync replaces it.</summary>
   SyncStarting,

   /// <summary>The sync finished: JAStudio may use the collection again.</summary>
   SyncCompleted
}
