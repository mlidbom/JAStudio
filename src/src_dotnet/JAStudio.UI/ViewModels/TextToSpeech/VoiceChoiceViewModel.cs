using JAStudio.TextToSpeech;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>A voice in the dialog for choosing voices: one of the chosen, or one of those still available to choose.</summary>
sealed class VoiceChoiceViewModel(TextToSpeechVoice voice)
{
   public TextToSpeechVoice Voice { get; } = voice;

   /// <summary>The voice as the lists show it, such as "Azure Daichi:DragonHDOmniLatestNeural (Male)".</summary>
   public string Description => Voice.Gender == TextToSpeechVoiceGender.Unspecified ? $"{Voice}" : $"{Voice} ({Voice.Gender})";
}
