namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsDatabase(AnkisHttpServer server) : IAnkisDatabase
   {
      readonly AnkisHttpServer _server = server;

      /// <summary>The add-on answers with the JSON the SELECT built as JSON, not as a string holding it, so it's returned as the text it arrived as.</summary>
      public string ReadJson(string select) => _server.CallForItsJsonText("col_read_json", ("select", select));
   }
}
