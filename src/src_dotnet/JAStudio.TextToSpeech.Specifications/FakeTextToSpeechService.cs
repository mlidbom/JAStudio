using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.TextToSpeech.Specifications;

/// <summary>
/// Stands in for a tts-provider's web service, which the specifications can't call: it answers every request with the answer it was given,<br/>
/// and records the requests, so that a specification can check what a tts-provider sent. Its <see cref="Client"/> is what the tts-provider calls through.
/// </summary>
sealed class FakeTextToSpeechService : HttpMessageHandler
{
   readonly List<RecordedRequest> _requests = [];
   Func<HttpResponseMessage> _answer = () => throw new InvalidOperationException("The fake service was given no answer.");

   public FakeTextToSpeechService() => Client = new HttpClient(this);

   public HttpClient Client { get; }

   /// <summary>The one request made. Throws when there were none, or several.</summary>
   public RecordedRequest Request => _requests.Single();

   /// <summary>Answers every request with <paramref name="status"/> and <paramref name="json"/> as the body.</summary>
   public void AnswersWith(HttpStatusCode status, string json) =>
      _answer = () => new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

   /// <summary>Answers every request with success and <paramref name="audio"/> as the body, as a service sends audio.</summary>
   public void AnswersWithAudio(byte[] audio) => _answer = () => new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(audio) };

   protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   {
      _requests.Add(await RecordedRequest.Of(request, cancellationToken));
      return _answer();
   }
}

/// <summary>A request a tts-provider sent to the <see cref="FakeTextToSpeechService"/>, read before the tts-provider disposes of it.</summary>
/// <param name="Body">The body as text, or empty when there was none.</param>
sealed record RecordedRequest(HttpMethod Method, string Uri, IReadOnlyDictionary<string, string> Headers, string ContentType, string Body)
{
   public static async Task<RecordedRequest> Of(HttpRequestMessage request, CancellationToken cancellation) =>
      new(request.Method,
          request.RequestUri?.ToString() ?? throw new InvalidOperationException("A request was sent with no address."),
          request.Headers.ToDictionary(it => it.Key, it => string.Join(" ", it.Value)),
          request.Content?.Headers.ContentType?.MediaType ?? "",
          request.Content is null ? "" : await request.Content.ReadAsStringAsync(cancellation));
}
