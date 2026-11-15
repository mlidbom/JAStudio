namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsDatabase(AnkiOverHttp anki) : IAnkisDatabase
   {
      readonly AnkiOverHttp _anki = anki;

      /// <summary>The add-on answers with the JSON the SELECT built as JSON, not as a string holding it, so it's returned as the text it arrived as.</summary>
      public string ReadJson(string select) => _anki.CallForItsJsonText("col_read_json", ("select", select));
   }
}
