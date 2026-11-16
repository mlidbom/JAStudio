using System;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// Anki refused a call JAStudio made over HTTP, saying why in words for a person: while no Anki profile is open, while Anki's own sync is under<br/>
/// way, or because Anki has another collection open than the one JAStudio works with. Nothing was changed in Anki.
/// </summary>
public sealed class AnkiRefusedTheCallException(string why, string? collectionAnkiHasOpen) : Exception($"Anki refused the call: {why}")
{
   /// <summary>Why Anki refused, in words for a person.</summary>
   public string Why { get; } = why;

   /// <summary>The path of the collection Anki has open, when it refused because that isn't the one JAStudio works with. Otherwise null.</summary>
   public string? CollectionAnkiHasOpen { get; } = collectionAnkiHasOpen;
}
