namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsMedia(AnkiOverHttp anki) : IAnkisMedia
   {
      readonly AnkiOverHttp _anki = anki;

      public string AddFile(string filePath) => _anki.Call<string>("media_add_file", ("file_path", filePath));

      public string Folder => _anki.Call<string>("anki_media_dir");
   }
}
