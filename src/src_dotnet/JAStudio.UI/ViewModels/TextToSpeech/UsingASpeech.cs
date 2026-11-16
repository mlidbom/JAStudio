using System;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.Studying;
using JAStudio.TextToSpeech;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// Uses a synthesized speech as a note's TTS audio: the only step of generating TTS audio that changes anything.<br/>
/// It stores the MP3 in the media layer under a new name, gives the spaced repetition system a copy (<see cref="ISRSMedia"/>), and makes it<br/>
/// the note's TTS audio in the corpus's own audio (<see cref="IHasTextToSpeechAudio.Audio"/>), replacing the TTS audio there: as ours, made by<br/>
/// text-to-speech, with its voice.
/// </summary>
/// <remarks>
/// The spaced repetition system gets its copy before the note changes, so that a study-note written from the note finds its file there.<br/>
/// The file the note referenced before stays where it was, unreferenced, as the media layer does with any replaced audio (<c>media-storage-and-import.md</c>).
/// </remarks>
class UsingASpeech(TemporaryServiceCollection services)
{
   readonly MediaStorageService _mediaStorage = services.Resolve<MediaStorageService>();
   readonly ISRSMedia _srsMedia = services.Resolve<ISRSMedia>();

   /// <summary>Makes <paramref name="speech"/> the TTS audio of <paramref name="note"/>, and returns the name of the file it's stored in.</summary>
   public string Use(IHasTextToSpeechAudio note, SynthesizedSpeech speech)
   {
      var fileName = speech.UniqueFileName();
      var stored = _mediaStorage.StoreNewAudioFile(speech.Mp3, TextToSpeechAudioFolders.For(note), fileName);
      _srsMedia.Add(stored);
      note.Edit(it => it.Audio.ReplaceTheTextToSpeechAudioWith(new AudioReference(fileName)
      {
         IsOurs = true,
         IsTextToSpeech = true,
         TextToSpeechVoice = speech.Voice.NameWithItsProvider,
         VoiceGender = GenderOf(speech.Voice)
      }));
      return fileName;
   }

   /// <summary>The gender of <paramref name="voice"/>'s speech, as a note's audio knows it: none when its tts-provider gives none.</summary>
   static VoiceGender? GenderOf(TextToSpeechVoice voice) => voice.Gender switch
   {
      TextToSpeechVoiceGender.Female => VoiceGender.Female,
      TextToSpeechVoiceGender.Male => VoiceGender.Male,
      TextToSpeechVoiceGender.Neutral => VoiceGender.Neutral,
      TextToSpeechVoiceGender.Unspecified => null,
      _ => throw new ArgumentOutOfRangeException(nameof(voice), voice.Gender, "A voice gender the note's audio doesn't know.")
   };
}
