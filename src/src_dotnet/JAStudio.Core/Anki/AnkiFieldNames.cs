using System.Diagnostics.CodeAnalysis;
// ReSharper disable MemberCanBeInternal

// ReSharper disable MemberHidesStaticFromOuterClass

namespace JAStudio.Core.Anki;

/// Field names as they exist in Anki's note type definitions.
/// Used ONLY by Anki integration code (bulk loader, query builder, Anki note wrappers).
/// Domain code must NOT reference these — use NoteFieldsConstants / SentenceNoteFields instead.
public static class AnkiFieldNames
{
   public const string Question = "Q";
   public const string Answer = "A";
   public const string JasNoteId = "jas_note_id";
   public const string NoteId = "nid";

   public static class VocabCard
   {
      public const string Reading = "Reading";
      public const string Listening = "Listening";
   }

   public static class SentenceCard
   {
      public const string Reading = "Reading";
      public const string Listening = "Listening";
   }

   public static class Vocab
   {
      public const string Question = AnkiFieldNames.Question;
      public const string ActiveAnswer = Answer;
      public const string AudioB = "Audio_b";
      public const string AudioG = "Audio_g";
      public const string AudioTTS = "Audio_TTS";
      public const string Image = "Image";
      public const string UserImage = "__image";

      /// <summary>The vocab's forms in Anki: its question first, then its entered forms.</summary>
      public const string Forms = "F";
   }

   public static class Kanji
   {
      public const string Question = AnkiFieldNames.Question;
      public const string ActiveAnswer = Answer;
      public const string SourceAnswer = "source_answer";
   }

   public static class Sentence
   {
      public const string Reading = "Reading";
      public const string Id = "ID";
      public const string SourceQuestion = "source_question";
      public const string SourceComments = "Comments";
      public const string SourceAnswer = "source_answer";
      public const string Audio = "Audio Sentence";
      public const string Screenshot = "Screenshot";
   }
}
