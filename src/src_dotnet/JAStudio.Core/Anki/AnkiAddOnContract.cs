namespace JAStudio.Core.Anki;

/// <summary>
/// The contract between JAStudio and the Anki add-on, both ways over HTTP: the functions JAStudio calls in Anki, on the add-on's server, and the<br/>
/// requests the add-on makes of JAStudio, on JAStudio's card server. Each side compares the other's <see cref="Version"/> with its own before it<br/>
/// relies on it, so that a JAStudio and an add-on built from different checkouts refuse each other, saying so, where they'd otherwise misunderstand<br/>
/// each other.
/// </summary>
/// <remarks>review-naming:anki-http: AnkiAddOnContract is Claude's (2026-11-15).</remarks>
public static class AnkiAddOnContract
{
   /// <summary>
   /// The version of the contract this JAStudio speaks. It changes whenever a function, a request, an argument or an answer does, together with<br/>
   /// the add-on's <c>CONTRACT_VERSION</c>.
   /// </summary>
   /// <remarks>Version 4 added the requests from Anki to JAStudio (2026-11-15).</remarks>
   public const int Version = 4;
}
