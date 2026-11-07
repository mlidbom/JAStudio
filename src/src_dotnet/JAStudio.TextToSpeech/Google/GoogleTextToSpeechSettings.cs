namespace JAStudio.TextToSpeech.Google;

/// <summary>
/// What JAStudio needs to call Google Cloud Text-to-Speech: an API key of a Google Cloud project with the Text-to-Speech API enabled.<br/>
/// Part of the <see cref="TextToSpeechSettings"/>.
/// </summary>
/// <remarks>A class rather than a record, so that its <see cref="object.ToString"/> doesn't print the key into a log.</remarks>
public sealed class GoogleTextToSpeechSettings
{
   public required string ApiKey { get; init; }
}
