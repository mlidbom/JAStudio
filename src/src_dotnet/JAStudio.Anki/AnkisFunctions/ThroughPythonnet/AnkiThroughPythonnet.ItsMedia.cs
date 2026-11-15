namespace JAStudio.Anki.AnkisFunctions.ThroughPythonnet;

public sealed partial class AnkiThroughPythonnet
{
   sealed class ItsMedia(AnkiThroughPythonnet anki) : IAnkisMedia
   {
      readonly AnkiThroughPythonnet _anki = anki;

      public string AddFile(string filePath) => _anki.Backend.Use(it => (string)it.media_add_file(filePath));

      public string Folder => _anki.Backend.Use(it => (string)it.anki_media_dir());
   }
}
