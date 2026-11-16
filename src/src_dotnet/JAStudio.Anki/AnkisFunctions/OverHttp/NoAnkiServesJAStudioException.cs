using System;

namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// JAStudio called Anki over HTTP while no Anki serves it: the Anki add-on serves JAStudio only while an Anki profile is open, so Anki isn't<br/>
/// running, or has no profile open, or it ended without stopping its serving. Nothing was changed in Anki.
/// </summary>
public sealed class NoAnkiServesJAStudioException(string why, Exception? howItShowed = null) : Exception($"No Anki serves JAStudio: {why}", howItShowed)
{
   /// <summary>Why no Anki serves JAStudio, in words for a person.</summary>
   public string Why { get; } = why;
}
