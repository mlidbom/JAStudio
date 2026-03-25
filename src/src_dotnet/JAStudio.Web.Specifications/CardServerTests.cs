using System.Net.Http;
using System.Threading.Tasks;
using JAStudio.Core.Specifications.Fixtures;
using Xunit;

#pragma warning disable xUnit1051
namespace JAStudio.Web.Specifications;

public class CardServerTests : IAsyncLifetime
{
   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(DataNeeded.Kanji | DataNeeded.Vocabulary);
   readonly CardServer _server = new();
   readonly HttpClient _http = new();

   public ValueTask InitializeAsync()
   {
      _server.Start();
      return ValueTask.CompletedTask;
   }

   public async ValueTask DisposeAsync()
   {
      _http.Dispose();
      await _server.StopAsync();
      _appScope.Dispose();
   }

   [Fact]
   public async Task Home_page_returns_200()
   {
      var response = await _http.GetAsync($"{_server.BaseUrl}/");
      response.EnsureSuccessStatusCode();

      var html = await response.Content.ReadAsStringAsync();
      Assert.Contains("JAStudio", html);
   }

   [Fact]
   public async Task Kanji_card_endpoint_renders_readings()
   {
      var collection = _appScope.CoreApp.Collection;
      var kanjiNote = collection.Kanji.WithKanji("病")!;
      var noteId = kanjiNote.GetId();

      var response = await _http.GetAsync($"{_server.BaseUrl}/card/kanji/back?NoteId={noteId}");
      response.EnsureSuccessStatusCode();

      var html = await response.Content.ReadAsStringAsync();

      // The response should contain the kanji readings rendered by Blazor SSR
      Assert.Contains("ビョウ", html, System.StringComparison.Ordinal);
   }

   [Fact]
   public async Task Kanji_card_front_renders_question()
   {
      var collection = _appScope.CoreApp.Collection;
      var kanjiNote = collection.Kanji.WithKanji("病")!;
      var noteId = kanjiNote.GetId();
      
      var response = await _http.GetAsync($"{_server.BaseUrl}/card/kanji/front?NoteId={noteId}");
      response.EnsureSuccessStatusCode();

      var html = await response.Content.ReadAsStringAsync();

      // The front side should contain the kanji character (Blazor encodes CJK as HTML entities)
      Assert.Contains("&#x75C5;", html, System.StringComparison.Ordinal);
      Assert.Contains("clipboard", html, System.StringComparison.Ordinal);
      // The front side should NOT contain back-side content like readings or mnemonics
      Assert.DoesNotContain("mnemonic", html, System.StringComparison.Ordinal);
   }

   [Fact]
   public async Task Blazor_web_js_is_served()
   {
      var response = await _http.GetAsync($"{_server.BaseUrl}/_framework/blazor.web.js");
      response.EnsureSuccessStatusCode();

      var content = await response.Content.ReadAsStringAsync();
      Assert.True(content.Length > 1000, "blazor.web.js should be a substantial script file");
   }

   [Fact]
   public async Task Vocab_card_front_renders_question()
   {
      var collection = _appScope.CoreApp.Collection;
      var vocabNote = collection.Vocab.WithQuestion("入れる")[0];
      var noteId = vocabNote.GetId();

      var response = await _http.GetAsync($"{_server.BaseUrl}/card/vocab/front?NoteId={noteId}");
      response.EnsureSuccessStatusCode();

      var html = await response.Content.ReadAsStringAsync();

      // The front side should contain the question text
      Assert.Contains("question", html, System.StringComparison.Ordinal);
   }
}
