namespace JAStudio.Anki.AnkisFunctions.OverHttp;

public sealed partial class AnkiOverHttp
{
   sealed class ItsMedia(AnkisHttpServer server) : IAnkisMedia
   {
      readonly AnkisHttpServer _server = server;

      public string AddFile(string filePath) => _server.Call<string>("media_add_file", ("file_path", filePath));

      public string Folder => _server.Call<string>("anki_media_dir");
   }
}
