namespace JAStudio.Core.Anki;

/// <summary>
/// The names of the fields JAStudio's note types have in Anki. Since the notes were trimmed into study-notes they hold only what Anki itself needs:<br/>
/// the question, the audio, and the id of the corpus note each Anki note belongs to. JAStudio writes them from the corpus<br/>
/// (<see cref="Note.CorpusData.CorpusObjectData.ToNoteData"/>), and reads only <see cref="JasNoteId"/> back.
/// </summary>
public static class AnkiFieldNames
{
   /// <summary>The question: the sort field in Anki's browser, and what the reading cards' front shows, so that Anki makes a reading card.</summary>
   public const string Question = "Q";

   /// <summary>The id of the corpus note an Anki note belongs to, which links the two.</summary>
   public const string JasNoteId = "jas_note_id";

   /// <summary>A vocab's audio, in three fields as the corpus has it. Anki makes a listening card when one holds audio, and plays it.</summary>
   public static class Vocab
   {
      public const string AudioB = "Audio_b";
      public const string AudioG = "Audio_g";
      public const string AudioTTS = "Audio_TTS";
   }

   /// <summary>A sentence's audio. Anki makes a listening card when it holds audio, and plays it.</summary>
   public static class Sentence
   {
      public const string Audio = "Audio Sentence";
   }
}
