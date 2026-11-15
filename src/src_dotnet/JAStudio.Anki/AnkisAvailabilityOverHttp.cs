using JAStudio.Anki.AnkisFunctions.OverHttp;
using JAStudio.Core.Studying;

namespace JAStudio.Anki;

/// <summary>
/// Anki as the spaced repetition system of a JAStudio that is a program of its own (<see cref="SRSAvailability"/>): there is one, whether or not<br/>
/// it runs right now, and JAStudio may read and change its collection whenever a call to it would be answered<br/>
/// (<see cref="AnkiOverHttp.WhyItCannotBeCalledNow"/>). Anki is asked each time, since it tells JAStudio of nothing.
/// </summary>
/// <remarks>
/// It can't be used while Anki isn't running or has no profile open, while Anki's own sync, with AnkiWeb, is under way, and while Anki has<br/>
/// another collection open than the one JAStudio works with.
/// </remarks>
public sealed class AnkisAvailabilityOverHttp(AnkiOverHttp anki) : SRSAvailability
{
   readonly AnkiOverHttp _anki = anki;

   public override bool IsAvailable => true;

   public override string? WhyItCannotBeUsedNow => _anki.WhyItCannotBeCalledNow;

   public override bool HasAnotherCollectionOpenThanJAStudioWorksWith => _anki.TheOtherCollectionAnkiHasOpen != null;

   public override void WorkWithTheCollectionItHasOpen() => _anki.TakeTheCollectionAnkiHasOpen();
}
