namespace JAStudio.Anki.AnkisFunctions;

/// <summary>
/// The database of the collection Anki has open, read through Anki's own connection to it: one group of the functions JAStudio calls in Anki<br/>
/// (see <see cref="IAnkisNotes"/> for the groups). <see cref="CollectionDatabase.AnkisOwnConnection"/> reads through it, and says why only SELECTs may run.
/// </summary>
public interface IAnkisDatabase
{
   /// <summary>The one value the SELECT <paramref name="select"/> returns, a JSON text. The add-on's <c>col_read_json</c>.</summary>
   string ReadJson(string select);
}
