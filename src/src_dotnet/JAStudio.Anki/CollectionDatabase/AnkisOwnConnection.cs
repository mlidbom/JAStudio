namespace JAStudio.Anki.CollectionDatabase;

/// <summary>
/// The database of the collection Anki has open, read through Anki's own connection to it, with SELECTs only, as JAStudio inside Anki reads it.<br/>
/// A read sees Anki's latest changes, and Anki keeps its undo history.
/// </summary>
/// <remarks>
/// Anki discards its undo history whenever anything but a SELECT runs through that connection, so only SELECTs may. Until 2026-11-10 JAStudio<br/>
/// read the collection's file instead, after having Anki checkpoint its write-ahead log into it, so the file had Anki's latest changes. The<br/>
/// checkpoint wasn't a SELECT, so after any change Anki's Edit > Undo did nothing.
/// </remarks>
/// <remarks>
/// While a SELECT runs, Anki's backend holds the collection, so Anki's own calls to it wait: under 80 ms for the longest of the reads after a<br/>
/// change in Anki, that of the study-notes, measured on Magnus's collection on 2026-11-10. Python's lock is free meanwhile, so Anki's UI only<br/>
/// waits if it needs the collection.
/// </remarks>
public sealed class AnkisOwnConnection : AnkiCollectionDatabase
{
   protected override string TheJsonTextOf(string select) => AnkiFacade.Col.ReadJson(select);
}
