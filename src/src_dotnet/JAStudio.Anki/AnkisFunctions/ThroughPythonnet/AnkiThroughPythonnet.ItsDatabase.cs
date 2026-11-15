namespace JAStudio.Anki.AnkisFunctions.ThroughPythonnet;

public sealed partial class AnkiThroughPythonnet
{
   sealed class ItsDatabase(AnkiThroughPythonnet anki) : IAnkisDatabase
   {
      readonly AnkiThroughPythonnet _anki = anki;

      public string ReadJson(string select) => _anki.Backend.Use(it => (string)it.col_read_json(select));
   }
}
