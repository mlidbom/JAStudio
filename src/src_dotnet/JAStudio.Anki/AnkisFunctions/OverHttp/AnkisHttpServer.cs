using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// The HTTP server the Anki add-on runs for JAStudio (Python's <c>HttpServerForJAStudio</c>), as JAStudio calls it: every request carries the<br/>
/// server's token, and every call of a function names the collection JAStudio works with, which Anki refuses the call over when it has another<br/>
/// collection open.
/// </summary>
/// <remarks>
/// Before its first call it compares the add-on's version of the contract with <see cref="ContractVersion"/>, and refuses to call an add-on that<br/>
/// speaks another (<see cref="AnkiSpeaksAnotherContractException"/>).
/// </remarks>
sealed class AnkisHttpServer : IDisposable
{
   /// <summary>
   /// The version of the contract this JAStudio speaks: the functions it calls in Anki, their arguments and their answers. It changes whenever one<br/>
   /// of them does, together with the add-on's <c>CONTRACT_VERSION</c>.
   /// </summary>
   const int ContractVersion = 1;

   readonly HttpClient _http;
   readonly FileInfo _collection;
   readonly Lazy<int> _contractVersionOfTheAddOn;

   /// <param name="access">The server's port and token.</param>
   /// <param name="collection">The file of the collection JAStudio works with, which each call names.</param>
   public AnkisHttpServer(AnkisHttpServerAccess access, FileInfo collection)
   {
      _collection = collection;
      _http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{access.Port}/") };
      _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access.Token);
      _contractVersionOfTheAddOn = new Lazy<int>(AskTheAddOnForItsContractVersion, LazyThreadSafetyMode.PublicationOnly);
   }

   /// <summary>Calls the add-on's function <paramref name="function"/> with <paramref name="arguments"/>, by the names the function has them by, and returns its answer.</summary>
   public TResult Call<TResult>(string function, params (string Name, object Value)[] arguments) =>
      ResultOfTheCall(function, arguments, result => result.Deserialize<TResult>() ?? throw new InvalidOperationException($"{function} answered with null."));

   /// <summary>Calls the add-on's function <paramref name="function"/>, which answers with nothing, with <paramref name="arguments"/>, by the names the function has them by.</summary>
   public void Call(string function, params (string Name, object Value)[] arguments) => ResultOfTheCall(function, arguments, _ => 0);

   /// <summary>
   /// Calls the add-on's function <paramref name="function"/>, whose answer is JSON it didn't build itself, such as what a SELECT built, and returns<br/>
   /// that JSON as the text it is.
   /// </summary>
   public string CallForItsJsonText(string function, params (string Name, object Value)[] arguments) => ResultOfTheCall(function, arguments, result => result.GetRawText());

   public void Dispose() => _http.Dispose();

   TResult ResultOfTheCall<TResult>(string function, (string Name, object Value)[] arguments, Func<JsonElement, TResult> read)
   {
      AssertThatTheAddOnSpeaksTheSameContract();
      using var request = new HttpRequestMessage(HttpMethod.Post, $"call/{function}?collection={Uri.EscapeDataString(_collection.FullName)}");
      request.Content = TheJsonObjectOf(arguments);
      return ResultOf(request, whatIsCalled: function, read);
   }

   /// <summary>
   /// <paramref name="arguments"/> as the JSON object the add-on takes a call's arguments as, serialized before it's sent, so that the request says<br/>
   /// how long its body is: the add-on's server, Python's <c>http.server</c>, reads that many bytes, and nothing of a body sent in chunks.
   /// </summary>
   static ByteArrayContent TheJsonObjectOf((string Name, object Value)[] arguments) =>
      new(JsonSerializer.SerializeToUtf8Bytes(arguments.ToDictionary(it => it.Name, it => it.Value)))
      {
         Headers = { ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" } }
      };

   void AssertThatTheAddOnSpeaksTheSameContract()
   {
      if(_contractVersionOfTheAddOn.Value != ContractVersion) throw new AnkiSpeaksAnotherContractException(_contractVersionOfTheAddOn.Value, ContractVersion);
   }

   int AskTheAddOnForItsContractVersion()
   {
      using var request = new HttpRequestMessage(HttpMethod.Get, "contract");
      return ResultOf(request, whatIsCalled: "The request for the add-on's contract", result => result.GetProperty("version").GetInt32());
   }

   /// <summary>The <c>result</c> of Anki's answer to <paramref name="request"/>, read by <paramref name="read"/>. An answer that is a refusal or an error is thrown.</summary>
   TResult ResultOf<TResult>(HttpRequestMessage request, string whatIsCalled, Func<JsonElement, TResult> read)
   {
      using var response = _http.Send(request);
      using var answer = JsonDocument.Parse(response.Content.ReadAsStream());
      var body = answer.RootElement;

      if(response.StatusCode == HttpStatusCode.OK) return read(body.GetProperty("result"));
      if(body.TryGetProperty("refused", out var why)) throw new AnkiRefusedTheCallException(why.GetString() ?? "", CollectionAnkiHasOpenIn(body));
      throw new CallToAnkiFailedException(whatIsCalled, response.StatusCode, body.GetProperty("error").GetString() ?? "");

      static string? CollectionAnkiHasOpenIn(JsonElement refusal) => refusal.TryGetProperty("collectionOpen", out var collectionOpen) ? collectionOpen.GetString() : null;
   }
}
