using Compze.Threading;
using JAStudio.Core.Studying;

namespace JAStudio.Core.TestUtils;

public partial class PretendAnki
{
   /// <summary>The pretend Anki as a spaced repetition system to work with: there is one, which can be used unless its own sync is under way.</summary>
   sealed class ItsAvailability : SRSAvailability
   {
      readonly IMonitor _monitor = IMonitor.New();
      bool _itsOwnSyncIsUnderWay;

      public override bool IsAvailable => true;
      public override string? WhyItCannotBeUsedNow => _monitor.Locked(() => _itsOwnSyncIsUnderWay ? "Anki is syncing." : null);

      public void ItsOwnSyncIsUnderWay(bool isUnderWay) => _monitor.Locked(() => _itsOwnSyncIsUnderWay = isUnderWay);
   }
}
