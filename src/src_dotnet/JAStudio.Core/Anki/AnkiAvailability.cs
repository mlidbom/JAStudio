namespace JAStudio.Core.Anki;

/// <summary>
/// Whether there's an Anki for JAStudio to work with: there is while JAStudio runs inside Anki, and none while it runs without, as the headless<br/>
/// JAStudio and the standalone JAStudio do. What only Anki can do, such as creating a new note's Anki note, importing Anki's notes, or opening<br/>
/// Anki's windows (<see cref="IAnkiWindows"/>), is offered only while there is.
/// </summary>
public sealed class AnkiAvailability
{
   AnkiAvailability(bool isAvailable) => IsAvailable = isAvailable;

   /// <summary>JAStudio runs inside Anki, or, in the specifications, beside a pretended one.</summary>
   public static readonly AnkiAvailability Available = new(true);

   /// <summary>JAStudio runs without Anki.</summary>
   public static readonly AnkiAvailability Unavailable = new(false);

   public bool IsAvailable { get; }
}
