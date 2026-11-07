using System;
using System.Collections.Generic;
using Compze.Must;
using Compze.xUnitBDD;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.CorpusData;

// ReSharper disable InconsistentNaming

namespace JAStudio.Core.Specifications.AICreatedSpecs;

/// <summary>
/// A note saved in Anki, as Anki's note editor and HyperTTS save notes: only the audio HyperTTS writes is taken into the corpus,<br/>
/// since everything else is edited in JAStudio, and the rest of the audio and the images only come with an import.<br/>
/// Each note is first added in Anki, as an import adds it, and then saved in Anki with every field and the tags changed.
/// </summary>
public class When_a_note_is_saved_in_Anki : SpecificationStartingWithAnEmptyCollection, IAIGeneratedSpec
{
   static AnkiNoteData InAnki(Dictionary<string, string> fields, string tag) => new(null, fields, [tag]);

   public class for_a_vocab : When_a_note_is_saved_in_Anki
   {
      const long AnkiId = 1;
      readonly VocabData _data;

      public for_a_vocab()
      {
         var vocab = GetService<VocabCollection>();
         vocab.ExternalSyncHandler.ExternalNoteAdded(AnkiId, VocabInAnki("食べる", "to eat", "imported", "source::wani"));
         vocab.ExternalSyncHandler.ExternalNoteWillFlush(AnkiId, VocabInAnki("飲む", "to drink", "saved", "edited::in-anki"));
         _data = vocab.WithExternalIdOrNone(AnkiId)?.ToCorpusData() as VocabData ?? throw new InvalidOperationException("The vocab is gone.");
      }

      static AnkiNoteData VocabInAnki(string question, string answer, string fileNamePrefix, string tag) =>
         InAnki(new Dictionary<string, string>
                {
                   [AnkiFieldNames.Vocab.Question] = question,
                   [AnkiFieldNames.Vocab.ActiveAnswer] = answer,
                   [AnkiFieldNames.Vocab.AudioB] = $"[sound:{fileNamePrefix}-b.mp3]",
                   [AnkiFieldNames.Vocab.AudioG] = $"[sound:{fileNamePrefix}-g.mp3]",
                   [AnkiFieldNames.Vocab.AudioTTS] = $"[sound:{fileNamePrefix}-tts.mp3]",
                   [AnkiFieldNames.Vocab.Image] = $"<img src=\"{fileNamePrefix}.png\">",
                   [AnkiFieldNames.Vocab.UserImage] = $"<img src=\"{fileNamePrefix}-user.png\">"
                },
                tag);

      [XF] public void the_tts_audio_is_taken() => _data.AudioTTS.Must().Be("[sound:saved-tts.mp3]");
      [XF] public void audio_g_which_some_tts_presets_write_is_taken() => _data.AudioG.Must().Be("[sound:saved-g.mp3]");
      [XF] public void audio_b_is_not_taken() => _data.AudioB.Must().Be("[sound:imported-b.mp3]");
      [XF] public void the_images_are_not_taken() => (_data.Image, _data.UserImage).Must().Be(("<img src=\"imported.png\">", "<img src=\"imported-user.png\">"));
      [XF] public void the_question_is_not_taken() => _data.Question.Must().Be("食べる");
      [XF] public void the_tags_are_not_taken() => (_data.Tags.Contains("source::wani"), _data.Tags.Contains("edited::in-anki")).Must().Be((true, false));
   }

   public class for_a_sentence : When_a_note_is_saved_in_Anki
   {
      const long AnkiId = 2;
      readonly SentenceData _data;

      public for_a_sentence()
      {
         var sentences = GetService<SentenceCollection>();
         sentences.ExternalSyncHandler.ExternalNoteAdded(AnkiId, SentenceInAnki("家に帰る", "Go home", "imported", "source::anime"));
         sentences.ExternalSyncHandler.ExternalNoteWillFlush(AnkiId, SentenceInAnki("学校に行く", "Go to school", "saved", "edited::in-anki"));
         _data = sentences.WithExternalIdOrNone(AnkiId)?.ToCorpusData() as SentenceData ?? throw new InvalidOperationException("The sentence is gone.");
      }

      static AnkiNoteData SentenceInAnki(string question, string answer, string prefix, string tag) =>
         InAnki(new Dictionary<string, string>
                {
                   [AnkiFieldNames.Sentence.SourceQuestion] = question,
                   [AnkiFieldNames.Sentence.SourceAnswer] = answer,
                   [AnkiFieldNames.Sentence.SourceComments] = $"{prefix} comments",
                   [AnkiFieldNames.Sentence.Reading] = $"{prefix} reading",
                   [AnkiFieldNames.Sentence.Id] = $"{prefix}-id",
                   [AnkiFieldNames.Sentence.Audio] = $"[sound:{prefix}.mp3]",
                   [AnkiFieldNames.Sentence.Screenshot] = $"<img src=\"{prefix}.png\">"
                },
                tag);

      [XF] public void the_audio_which_tts_writes_is_taken() => _data.Audio.Must().Be("[sound:saved.mp3]");
      [XF] public void the_screenshot_is_not_taken() => _data.Screenshot.Must().Be("<img src=\"imported.png\">");
      [XF] public void the_question_and_answer_are_not_taken() => (_data.SourceQuestion, _data.SourceAnswer).Must().Be(("家に帰る", "Go home"));
      [XF] public void the_comments_reading_and_external_id_are_not_taken() =>
         (_data.SourceComments, _data.Reading, _data.ExternalId).Must().Be(("imported comments", "imported reading", "imported-id"));
      [XF] public void the_tags_are_not_taken() => (_data.Tags.Contains("source::anime"), _data.Tags.Contains("edited::in-anki")).Must().Be((true, false));
   }

   public class for_a_kanji : When_a_note_is_saved_in_Anki
   {
      const long AnkiId = 3;
      readonly KanjiNote _beforeTheSave;
      readonly KanjiNote _afterTheSave;

      public for_a_kanji()
      {
         var kanji = GetService<KanjiCollection>();
         kanji.ExternalSyncHandler.ExternalNoteAdded(AnkiId, KanjiInAnki("行", "go", "source::kanken"));
         _beforeTheSave = TheKanji();
         kanji.ExternalSyncHandler.ExternalNoteWillFlush(AnkiId, KanjiInAnki("食", "eat", "edited::in-anki"));
         _afterTheSave = TheKanji();
         return;

         KanjiNote TheKanji() => kanji.WithExternalIdOrNone(AnkiId) ?? throw new InvalidOperationException("The kanji is gone.");
      }

      static AnkiNoteData KanjiInAnki(string kanji, string answer, string tag) =>
         InAnki(new Dictionary<string, string>
                {
                   [AnkiFieldNames.Kanji.Question] = kanji,
                   [AnkiFieldNames.Kanji.SourceAnswer] = answer,
                   [AnkiFieldNames.Kanji.ActiveAnswer] = answer
                },
                tag);

      [XF] public void the_note_is_left_as_it_was_since_tts_writes_no_kanji_audio_that_jastudio_keeps() => ReferenceEquals(_afterTheSave, _beforeTheSave).Must().BeTrue();
      [XF] public void nothing_is_taken() => (_afterTheSave.GetQuestion(), _afterTheSave.GetAnswer()).Must().Be(("行", "go"));
   }
}
