using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using JAStudio.Core.Anki;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// The HTTP server the Anki add-on runs for JAStudio (Python's <c>HttpServerForJAStudio</c>), as JAStudio calls it with one access to it: every<br/>
/// request carries the server's token, and every call of a function names the collection JAStudio works with, which Anki refuses the call over<br/>
/// when it has another collection open.
/// </summary>
/// <remarks>
/// Before its first request it compares the add-on's version of the contract with <see cref="AnkiAddOnContract.Version"/>, and refuses to call an<br/>
/// add-on that speaks another (<see cref="AnkiSpeaksAnotherContractException"/>).
/// </remarks>
/// <remarks>
/// An access outlives its server when Anki ends without stopping its serving: nothing then listens on the server's port, and a request fails<br/>
/// with <see cref="NoAnkiServesJAStudioException"/>, within <see cref="HowLongTheServerHasToTakeTheConnection"/>.
/// </remarks>
/// <remarks>A request waits for its answer for as long as Anki takes, as a call into Anki's own process does.</remarks>
sealed class AnkisHttpServer : IDisposable
{
   /// <summary>
   /// How long the server has to take a connection before JAStudio gives it up as gone. A server that listens, on this computer, takes it in under a<br/>
   /// millisecond. Windows otherwise keeps trying a port nothing listens on for two seconds, measured 2026-11-15, before it says so.
   /// </summary>
   static readonly TimeSpan HowLongTheServerHasToTakeTheConnection = TimeSpan.FromSeconds(1);

   readonly HttpClient _http;
   readonly Lazy<int> _contractVersionOfTheAddOn;

   public AnkisHttpServer(AnkisHttpServerAccess access)
   {
      Access = access;
      _http = new HttpClient(new SocketsHttpHandler { ConnectTimeout = HowLongTheServerHasToTakeTheConnection })
              {
                 BaseAddress = new Uri($"http://127.0.0.1:{access.Port}/"),
                 Timeout = Timeout.InfiniteTimeSpan
              };
      _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access.Token);
      _contractVersionOfTheAddOn = new Lazy<int>(AskTheAddOnForItsContractVersion, LazyThreadSafetyMode.PublicationOnly);
   }

   /// <summary>The server's port and token, which this reaches it with.</summary>
   public AnkisHttpServerAccess Access { get; }

   /// <summary>The file of the collection Anki has open. Refused, as a call is, while JAStudio may not use Anki's collection.</summary>
   public FileInfo TheCollectionAnkiHasOpen()
   {
      AssertThatTheAddOnSpeaksTheSameContract();
      using var request = new HttpRequestMessage(HttpMethod.Get, "collection");
      return ResultOf(request,
                      whatIsCalled: "The request for the collection Anki has open",
                      result => new FileInfo(result.GetProperty("path").GetString() ?? throw new InvalidOperationException("Anki answered with no path for the collection it has open.")));
   }

   /// <summary>
   /// Calls the add-on's function <paramref name="function"/> with <paramref name="arguments"/>, by the names the function has them by, naming<br/>
   /// <paramref name="collection"/> as the collection JAStudio works with, and returns its answer as <paramref name="read"/> reads it.
   /// </summary>
   public TResult ResultOfTheCall<TResult>(string function, FileInfo collection, (string Name, object Value)[] arguments, Func<JsonElement, TResult> read)
   {
      AssertThatTheAddOnSpeaksTheSameContract();
      using var request = new HttpRequestMessage(HttpMethod.Post, $"call/{function}?collection={Uri.EscapeDataString(collection.FullName)}");
      request.Content = TheJsonObjectOf(arguments);
      return ResultOf(request, whatIsCalled: function, read);
   }

   public void Dispose() => _http.Dispose();

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
      if(_contractVersionOfTheAddOn.Value != AnkiAddOnContract.Version) throw new AnkiSpeaksAnotherContractException(_contractVersionOfTheAddOn.Value, AnkiAddOnContract.Version);
   }

   int AskTheAddOnForItsContractVersion()
   {
      using var request = new HttpRequestMessage(HttpMethod.Get, "contract");
      return ResultOf(request, whatIsCalled: "The request for the add-on's contract", result => result.GetProperty("version").GetInt32());
   }

   /// <summary>The <c>result</c> of Anki's answer to <paramref name="request"/>, read by <paramref name="read"/>. An answer that is a refusal or an error is thrown.</summary>
   TResult ResultOf<TResult>(HttpRequestMessage request, string whatIsCalled, Func<JsonElement, TResult> read)
   {
      using var response = AnswerTo(request);
      using var answer = JsonDocument.Parse(response.Content.ReadAsStream());
      var body = answer.RootElement;

      if(response.StatusCode == HttpStatusCode.OK) return read(body.GetProperty("result"));
      if(body.TryGetProperty("refused", out var why)) throw new AnkiRefusedTheCallException(why.GetString() ?? "", CollectionAnkiHasOpenIn(body));
      throw new CallToAnkiFailedException(whatIsCalled, response.StatusCode, body.GetProperty("error").GetString() ?? "");

      static string? CollectionAnkiHasOpenIn(JsonElement refusal) => refusal.TryGetProperty("collectionOpen", out var collectionOpen) ? collectionOpen.GetString() : null;
   }

   /// <summary>The server's answer to <paramref name="request"/>. That nothing listens on the server's port any more is <see cref="NoAnkiServesJAStudioException"/>.</summary>
   HttpResponseMessage AnswerTo(HttpRequestMessage request)
   {
      try
      {
         return _http.Send(request);
      }
      catch(Exception nothingListens) when(SaysThatNothingListensOnTheServersPort(nothingListens))
      {
         throw new NoAnkiServesJAStudioException("The Anki that served JAStudio no longer answers: it ended without saying so.", nothingListens);
      }
   }

   /// <summary>
   /// Whether <paramref name="failure"/> of a request is how a port nothing listens on shows: the system refused the connection, as Linux does at<br/>
   /// once, or the connection wasn't taken within <see cref="HowLongTheServerHasToTakeTheConnection"/>, as on Windows, which is the only time limit<br/>
   /// a request has.
   /// </summary>
   static bool SaysThatNothingListensOnTheServersPort(Exception failure) =>
      failure is HttpRequestException { HttpRequestError: HttpRequestError.ConnectionError } or OperationCanceledException { InnerException: TimeoutException };
}
