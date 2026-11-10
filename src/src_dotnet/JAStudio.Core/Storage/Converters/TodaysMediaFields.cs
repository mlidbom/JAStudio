using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Storage.Converters;

/// <summary>
/// The notes' media lists as read from the media fields the corpus stores them in today: Anki's markup in fixed fields, such as a vocab's <c>audioB</c>,<br/>
/// <c>audioG</c> and <c>audioTTS</c>. The converters write the lists back into those fields the same way.
/// </summary>
/// <remarks>
/// Temporary: slice 9's step 3 stores the lists themselves, with the facts a migration proves for each file, and removes this (<c>corpus-data-model.md</c>,<br/>
/// Media, "The plan"). Until then the fields hold no facts, so these are guesses, kept only as long as a note is loaded:
/// <list type="bullet">
/// <item>A vocab's audio is all the corpus's own: <c>audioB</c> and <c>audioG</c> hold recordings, and <c>audioTTS</c> holds TTS audio. Its <c>image</c> is its<br/>
/// source's, mostly Core 2000's, and its <c>userImage</c> the corpus's own. Written back, the first recording goes into <c>audioB</c> and the others into <c>audioG</c>.</item>
/// <item>A sentence's audio is TTS audio, the corpus's own, when the sentence has the <c>_tts_audio</c> tag or the file's name is a TTS file's; any other audio<br/>
/// and its screenshots are its source's.</item>
/// <item>A kanji's images are the corpus's own: kanji have no source media.</item>
/// <item>JAStudio's own TTS audio, <c>jastudio-tts-…</c>, is ours.</item>
/// </list>
/// </remarks>
static class TodaysMediaFields
{
   /// <summary>The audio of the vocab <paramref name="data"/>, all of it the corpus's own.</summary>
   public static IEnumerable<AudioReference> VocabAudioIn(VocabDto? data) =>
      data is null ? [] : [..AudioIn(data.AudioB, isTextToSpeech: false), ..AudioIn(data.AudioG, isTextToSpeech: false), ..AudioIn(data.AudioTTS, isTextToSpeech: true)];

   /// <summary>The images the vocab <paramref name="data"/>'s source gave it: its <c>image</c>.</summary>
   public static IEnumerable<ImageReference> VocabSourceImagesIn(VocabDto? data) => AnkiMediaMarkup.ImagesIn(data?.Image ?? "");

   /// <summary>The corpus's own images of the vocab <paramref name="data"/>: its <c>userImage</c>.</summary>
   public static IEnumerable<ImageReference> VocabImagesIn(VocabDto? data) => AnkiMediaMarkup.ImagesIn(data?.UserImage ?? "");

   /// <summary>The audio the sentence <paramref name="data"/>'s source gave it: its audio unless that's TTS audio.</summary>
   public static IEnumerable<AudioReference> SentenceSourceAudioIn(SentenceDto? data) => AllTheSentenceAudioIn(data).Where(it => it.IsARecording);

   /// <summary>The corpus's own audio of the sentence <paramref name="data"/>: its TTS audio.</summary>
   public static IEnumerable<AudioReference> SentenceAudioIn(SentenceDto? data) => AllTheSentenceAudioIn(data).Where(it => it.IsTextToSpeech);

   /// <summary>The screenshots the sentence <paramref name="data"/>'s source gave it: all of them.</summary>
   public static IEnumerable<ImageReference> SentenceSourceScreenshotsIn(SentenceDto? data) => AnkiMediaMarkup.ImagesIn(data?.Screenshot ?? "");

   /// <summary>The kanji <paramref name="data"/>'s images, the corpus's own.</summary>
   public static IEnumerable<ImageReference> KanjiImagesIn(KanjiDto? data) => AnkiMediaMarkup.ImagesIn(data?.Image ?? "");

   static IEnumerable<AudioReference> AllTheSentenceAudioIn(SentenceDto? data)
   {
      if(data is null) return [];
      var isTaggedAsTextToSpeech = data.Tags.Contains(Tags.TTSAudio.Name, StringComparer.OrdinalIgnoreCase);
      return AnkiMediaMarkup.AudioIn(data.Audio).Select(it => WithTheFactsItsNameProves(it, isTaggedAsTextToSpeech || IsATextToSpeechFileName(it.FileName)));
   }

   static IEnumerable<AudioReference> AudioIn(string field, bool isTextToSpeech) => AnkiMediaMarkup.AudioIn(field).Select(it => WithTheFactsItsNameProves(it, isTextToSpeech));

   static AudioReference WithTheFactsItsNameProves(AudioReference audio, bool isTextToSpeech)
   {
      var isJAStudiosOwnTextToSpeech = audio.FileName.StartsWith("jastudio-tts-", StringComparison.OrdinalIgnoreCase);
      return audio with { IsTextToSpeech = isTextToSpeech || isJAStudiosOwnTextToSpeech, IsOurs = isJAStudiosOwnTextToSpeech };
   }

   /// <summary>Whether <paramref name="fileName"/> is that of TTS audio: HyperTTS's, AwesomeTTS's or JAStudio's own. Proof for a sentence, though not for a vocab.</summary>
   static bool IsATextToSpeechFileName(string fileName) =>
      fileName.StartsWith("hypertts-", StringComparison.OrdinalIgnoreCase)
   || fileName.StartsWith("ATTS ", StringComparison.Ordinal)
   || fileName.StartsWith("jastudio-tts-", StringComparison.OrdinalIgnoreCase);
}
