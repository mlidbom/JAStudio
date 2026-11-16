using System.Collections.Generic;
using System.Linq;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Search;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs.Search.When_searching_notes;

public class by_having_audio : NoteSearchSpecification
{
   protected by_having_audio()
   {
      var collection = GetService<JPCollection>();
      collection.Sentences.All().Single(it => it.GetQuestion() == SentenceAboutSchool)
                .Edit(it => it.Audio.ReplaceTheTextToSpeechAudioWith(new AudioReference("jastudio-tts-azure-daichineural-1.mp3") { IsTextToSpeech = true }));
      collection.Vocab.All().Single(it => it.GetQuestion() == VocabToGo).Edit(it => it.Audio.Add(new AudioReference("行く_b.mp3")));
   }

   public class for_notes_that_have_audio : by_having_audio
   {
      readonly List<string> _found;
      public for_notes_that_have_audio() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.HasAudio.Is(true));

      [XF] public void the_sentence_with_tts_audio_and_the_vocab_with_a_recording_are_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutSchool, VocabToGo));
   }

   public class for_notes_that_have_no_audio : by_having_audio
   {
      readonly List<string> _found;
      public for_notes_that_have_no_audio() => _found = QuestionsOfTheNotesFoundBy(NoteProperties.HasAudio.Is(false));

      [XF] public void the_sentence_without_audio_and_the_kanji_are_found() => _found.Must().SequenceEqual(InOrdinalOrder(SentenceAboutHome, KanjiGo));
   }
}
