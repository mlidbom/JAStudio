using Compze.Contracts;
using JAStudio.Anki;
using JAStudio.Core;
using JAStudio.Core.Note;
using JAStudio.Core.Storage.Media;
using JAStudio.TextToSpeech;
using Python.Runtime;

namespace JAStudio.UI.ViewModels.TextToSpeech;

/// <summary>
/// Keeps a synthesized speech as a note's TTS audio: the only step of generating TTS audio that changes anything.<br/>
/// It stores the MP3 in the media layer under a new name, copies it into Anki's media folder, and makes the note's TTS audio field reference it.
/// </summary>
/// <remarks>
/// The file goes into Anki before the field changes, since the field change writes the Anki note, which should find its file there.<br/>
/// Without Anki, as when JAStudio runs on its own or in the specifications, the copy into Anki's media folder is left out.<br/>
/// The file the field referenced before stays where it was, unreferenced, as the media layer does with any replaced audio (<c>media-storage-and-import.md</c>).
/// </remarks>
class SpeechKeeping(TemporaryServiceCollection services)
{
   readonly MediaStorageService _mediaStorage = services.Resolve<MediaStorageService>();

   /// <summary>Makes <paramref name="speech"/> the TTS audio of <paramref name="note"/>, and returns the name of the file it's kept in.</summary>
   public string Keep(IHasTextToSpeechAudio note, SynthesizedSpeech speech)
   {
      var fileName = speech.UniqueFileName();
      var kept = _mediaStorage.StoreNewAudioFile(speech.Mp3, TextToSpeechAudioFolders.For(note), fileName);
      if(PythonEngine.IsInitialized)
         CopyIntoAnkisMediaFolder(kept);
      note.TextToSpeechAudio.SetRawValue($"[sound:{fileName}]");
      return fileName;
   }

   static void CopyIntoAnkisMediaFolder(AudioAttachment kept)
   {
      var nameInAnki = AnkiFacade.Media.AddFile(kept.FilePath);
      Contract.State.Assert(nameInAnki == kept.OriginalFileName, () => $"Anki kept {kept.OriginalFileName} as {nameInAnki}, so the note's field would reference a file Anki doesn't have.");
   }
}
