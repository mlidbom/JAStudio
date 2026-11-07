namespace JAStudio.TextToSpeech.Audio.Loudness;

partial class EqualLoudnessFilter
{
   /// <summary>
   /// The coefficients of an <see cref="EqualLoudnessFilter"/>'s two filters for one sample rate: what each multiplies the latest input and outputs by.<br/>
   /// Feedforward coefficients multiply the inputs, from the current one back; feedback coefficients multiply the earlier outputs,<br/>
   /// and start with the 1 the filter's output is divided by, which is left out of the arithmetic.
   /// </summary>
   sealed class Coefficients
   {
      /// <summary>How many samples back the Yule-Walker filter looks.</summary>
      public const int YuleWalkerOrder = 10;

      /// <summary>How many samples back the Butterworth filter looks.</summary>
      public const int ButterworthOrder = 2;

      public Coefficients(double[] yuleWalkerFeedforward, double[] yuleWalkerFeedback, double[] butterworthFeedforward, double[] butterworthFeedback)
      {
         YuleWalkerFeedforward = yuleWalkerFeedforward;
         YuleWalkerFeedback = yuleWalkerFeedback;
         ButterworthFeedforward = butterworthFeedforward;
         ButterworthFeedback = butterworthFeedback;
      }

      public double[] YuleWalkerFeedforward { get; }
      public double[] YuleWalkerFeedback { get; }
      public double[] ButterworthFeedforward { get; }
      public double[] ButterworthFeedback { get; }
   }
}
