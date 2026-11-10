using System;
using System.Text.Json.Serialization;
using JAStudio.Core.Note.Media;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>
/// An <see cref="AudioReference"/> as the corpus stores it, in a note's list of audio: the file's name and the facts known about the file.<br/>
/// A fact that isn't known, or is false, is left out of the file, so that a reference with no facts is only its file's name.
/// </summary>
[MemoryPackable]
public partial class AudioReferenceDto
{
   public string FileName { get; init; } = string.Empty;

   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
   public bool IsOurs { get; init; }

   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
   public bool IsTextToSpeech { get; init; }

   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
   public string? TextToSpeechVoice { get; init; }

   /// <summary>The <see cref="Note.Media.VoiceGender"/>'s name in lower case, such as <c>male</c>.</summary>
   [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
   public string? VoiceGender { get; init; }

   public static AudioReferenceDto From(AudioReference audio) =>
      new()
      {
         FileName = audio.FileName,
         IsOurs = audio.IsOurs,
         IsTextToSpeech = audio.IsTextToSpeech,
         TextToSpeechVoice = audio.TextToSpeechVoice,
         VoiceGender = audio.VoiceGender?.ToString().ToLowerInvariant(),
      };

   public AudioReference ToReference() =>
      new(FileName)
      {
         IsOurs = IsOurs,
         IsTextToSpeech = IsTextToSpeech,
         TextToSpeechVoice = TextToSpeechVoice,
         VoiceGender = VoiceGender is null ? null : Enum.Parse<VoiceGender>(VoiceGender, ignoreCase: true),
      };
}
