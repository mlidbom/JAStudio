using System;
using Compze.Contracts;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Storage.Media;
using JAStudio.TextToSpeech;
using Python.Runtime;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// Uses a synthesized speech as a note's TTS audio: the only step of generating TTS audio that changes anything.<br/>
/// It stores the MP3 in the media layer under a new name, copies it into Anki's media folder, and makes it the note's TTS audio in the corpus's own audio<br/>
/// (<see cref="IHasTextToSpeechAudio.Audio"/>), replacing the TTS audio there: as ours, made by text-to-speech, with its voice.
/// </summary>
/// <remarks>
/// The file goes into Anki before the note changes, since the change writes the Anki note, which should find its file there.<br/>
/// Without Anki, as when JAStudio runs on its own or in the specifications, the copy into Anki's media folder is left out.<br/>
/// The file the note referenced before stays where it was, unreferenced, as the media layer does with any replaced audio (<c>media-storage-and-import.md</c>).
/// </remarks>
class UsingASpeech(TemporaryServiceCollection services)
{
   readonly MediaStorageService _mediaStorage = services.Resolve<MediaStorageService>();

   /// <summary>Makes <paramref name="speech"/> the TTS audio of <paramref name="note"/>, and returns the name of the file it's stored in.</summary>
   public string Use(IHasTextToSpeechAudio note, SynthesizedSpeech speech)
   {
      var fileName = speech.UniqueFileName();
      var stored = _mediaStorage.StoreNewAudioFile(speech.Mp3, TextToSpeechAudioFolders.For(note), fileName);
      if(PythonEngine.IsInitialized)
         CopyIntoAnkisMediaFolder(stored);
      note.Audio.ReplaceTheTextToSpeechAudioWith(new AudioReference(fileName)
      {
         IsOurs = true,
         IsTextToSpeech = true,
         TextToSpeechVoice = $"{speech.Voice.Provider.Name} {speech.Voice.Name}",
         VoiceGender = GenderOf(speech.Voice)
      });
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

   static void CopyIntoAnkisMediaFolder(AudioAttachment stored)
   {
      var nameInAnki = AnkiFacade.Media.AddFile(stored.FilePath);
      Contract.State.Assert(nameInAnki == stored.OriginalFileName, () => $"Anki stored {stored.OriginalFileName} as {nameInAnki}, so the note's study-note would reference a file Anki doesn't have.");
   }
}
