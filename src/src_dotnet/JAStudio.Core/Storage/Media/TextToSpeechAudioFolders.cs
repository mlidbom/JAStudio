using System;
using System.IO;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage.Media;

/// <summary>The folders of the media layer that TTS audio JAStudio generates is kept in, beside the TTS audio HyperTTS made.</summary>
/// <remarks>
/// A vocab's goes into <c>free/TTS/vocab</c>, with HyperTTS's. A sentence's goes into <c>free/TTS/sentences/jastudio</c>: HyperTTS's sentence audio<br/>
/// is in a folder per source, named by import rules rather than by anything a note says, and TTS audio is free, so there's no takedown<br/>
/// to split it by source for. See <c>media-storage-and-import.md</c>.
/// </remarks>
public static class TextToSpeechAudioFolders
{
   /// <summary>The folder, relative to the media layer's root, that the TTS audio of <paramref name="note"/> is kept in.</summary>
   public static string For(IHasTextToSpeechAudio note) => note switch
   {
      VocabNote => Path.Combine("free", "TTS", "vocab"),
      SentenceNote => Path.Combine("free", "TTS", "sentences", "jastudio"),
      _ => throw new ArgumentException($"There's no folder for the TTS audio of a {note.GetType().Name}.", nameof(note))
   };
}
