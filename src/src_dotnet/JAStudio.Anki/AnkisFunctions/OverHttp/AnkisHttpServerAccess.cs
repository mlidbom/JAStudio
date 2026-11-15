namespace JAStudio.Anki.AnkisFunctions.OverHttp;

/// <summary>
/// What it takes to reach the HTTP server the Anki add-on runs for JAStudio: the port it listens on, on this computer's loopback address, which<br/>
/// the system picks each time Anki starts, and the token every request has to carry, a new one each time too.
/// </summary>
public sealed record AnkisHttpServerAccess(int Port, string Token);
