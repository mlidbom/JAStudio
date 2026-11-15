using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JAStudio.TextToSpeech;

/// <summary>
/// A request a tts-provider refused, such as with a wrong key or region, or with the free characters used up.<br/>
/// Its message names the tts-provider, the HTTP status and what the service said about it.
/// </summary>
public sealed class TextToSpeechRequestFailedException(string message) : Exception(message)
{
   /// <summary>
   /// Whether the service refused the request for the rate of requests, with HTTP 429 Too Many Requests: more requests in a while than the account<br/>
   /// allows, or, with Azure, a service still scaling up to them. Asking again after a wait may succeed (<see cref="TextToSpeechVoice.SpeakAsync"/>).
   /// </summary>
   public bool IsARefusalForTheRateOfRequests { get; init; }

   /// <summary>Throws, naming <paramref name="providerName"/>, unless <paramref name="response"/> says the request succeeded.</summary>
   internal static async Task ThrowUnlessItSucceeded(HttpResponseMessage response, string providerName, CancellationToken cancellation)
   {
      if(response.IsSuccessStatusCode) return;

      var whatTheServiceSaid = await response.Content.ReadAsStringAsync(cancellation);
      throw new TextToSpeechRequestFailedException($"{providerName} refused the request: {(int)response.StatusCode} {response.ReasonPhrase}. {whatTheServiceSaid}".Trim())
            {
               IsARefusalForTheRateOfRequests = response.StatusCode == HttpStatusCode.TooManyRequests
            };
   }
}
