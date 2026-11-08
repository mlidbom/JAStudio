using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteAudio
{
   public WritableAudioValue First { get; }
   public WritableAudioValue Second { get; }
   public WritableAudioValue Tts { get; }

   public VocabNoteAudio(VocabDto? data, NoteGuard guard)
   {
      First = new WritableAudioValue(data?.AudioB ?? string.Empty, guard);
      Second = new WritableAudioValue(data?.AudioG ?? string.Empty, guard);
      Tts = new WritableAudioValue(data?.AudioTTS ?? string.Empty, guard);
   }

   public string PrimaryAudioPath
   {
      get
      {
         var firstPath = First.FirstAudioFilePath();
         if(!string.IsNullOrEmpty(firstPath)) return firstPath;

         var secondPath = Second.FirstAudioFilePath();
         if(!string.IsNullOrEmpty(secondPath)) return secondPath;

         var ttsPath = Tts.FirstAudioFilePath();
         if(!string.IsNullOrEmpty(ttsPath)) return ttsPath;

         return string.Empty;
      }
   }

   public string PrimaryAudio
   {
      get
      {
         var firstValue = First.RawValue();
         if(!string.IsNullOrEmpty(firstValue)) return firstValue;

         var secondValue = Second.RawValue();
         if(!string.IsNullOrEmpty(secondValue)) return secondValue;

         var ttsValue = Tts.RawValue();
         if(!string.IsNullOrEmpty(ttsValue)) return ttsValue;

         return string.Empty;
      }
   }

   public override string ToString() => $"first: {First}, second: {Second}, tts: {Tts}";
}
