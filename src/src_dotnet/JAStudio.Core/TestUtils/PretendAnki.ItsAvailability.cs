using Compze.Threading;
using JAStudio.Core.Studying;

namespace JAStudio.Core.TestUtils;

public partial class PretendAnki
{
   /// <summary>
   /// The pretend Anki as a spaced repetition system to work with: there is one, which can be used unless its own sync is under way, or it has<br/>
   /// opened another profile, whose collection JAStudio doesn't work with yet.
   /// </summary>
   sealed class ItsAvailability : SRSAvailability
   {
      readonly IMonitor _monitor = IMonitor.New();
      bool _itsOwnSyncIsUnderWay;
      bool _itHasAnotherCollectionOpenThanJAStudioWorksWith;

      public override bool IsAvailable => true;

      public override string? WhyItCannotBeUsedNow => _monitor.Locked(() => _itsOwnSyncIsUnderWay                          ? "Anki is syncing."
                                                                            : _itHasAnotherCollectionOpenThanJAStudioWorksWith ? "Anki has another collection open than the one JAStudio works with."
                                                                                                                                 : null);

      public override bool HasAnotherCollectionOpenThanJAStudioWorksWith => _monitor.Locked(() => _itHasAnotherCollectionOpenThanJAStudioWorksWith);

      public override void WorkWithTheCollectionItHasOpen() => _monitor.Locked(() => _itHasAnotherCollectionOpenThanJAStudioWorksWith = false);

      public void ItsOwnSyncIsUnderWay(bool isUnderWay) => _monitor.Locked(() => _itsOwnSyncIsUnderWay = isUnderWay);

      public void ItOpenedAnotherCollection() => _monitor.Locked(() => _itHasAnotherCollectionOpenThanJAStudioWorksWith = true);
   }
}
