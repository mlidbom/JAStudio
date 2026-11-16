namespace JAStudio.Core.Studying;

public abstract partial class SRSAvailability
{
   /// <summary>The availability of the spaced repetition system when JAStudio runs without one (<see cref="SRSAvailability.Unavailable"/>).</summary>
   sealed class None : SRSAvailability
   {
      public override bool IsAvailable => false;
      public override string WhyItCannotBeUsedNow => "JAStudio runs without a spaced repetition system.";
   }
}
