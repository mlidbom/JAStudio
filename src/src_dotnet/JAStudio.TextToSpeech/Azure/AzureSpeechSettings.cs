namespace JAStudio.TextToSpeech.Azure;

/// <summary>
/// What JAStudio needs to call Azure Speech: the key and the region of a Speech resource, from the Azure portal's "Keys and Endpoint" page.<br/>
/// The key only works with its own region. Part of the <see cref="TextToSpeechSettings"/>.
/// </summary>
/// <remarks>A class rather than a record, so that its <see cref="object.ToString"/> doesn't print the key into a log.</remarks>
public sealed class AzureSpeechSettings
{
   /// <summary>Either of the Speech resource's two keys.</summary>
   public required string Key { get; init; }

   /// <summary>The Speech resource's region identifier, such as <c>swedencentral</c>. HD voices are only in some regions: see <c>dev_docs/text-to-speech.md</c>.</summary>
   public required string Region { get; init; }
}
