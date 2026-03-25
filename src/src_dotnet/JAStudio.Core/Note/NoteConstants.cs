using System;
using System.Collections.Generic;
// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable MemberCanBePrivate.Global

namespace JAStudio.Core.Note;

static class MyNoteFields
{
   public const string Question = "Q";
   public const string Answer = "A";
   public const string JasNoteId = "jas_note_id";
}

static class SentenceNoteFields
{
   public const string Reading = "Reading";
   public const string Id = "ID";
   public const string ActiveQuestion = MyNoteFields.Question;
   public const string SourceQuestion = "source_question";
   public const string SourceComments = "Comments";
   public const string UserComments = "__comments";
   public const string UserQuestion = "__question";
   public const string ActiveAnswer = MyNoteFields.Answer;
   public const string SourceAnswer = "source_answer";
   public const string UserAnswer = "__answer";
   public const string ParsingResult = "__parsing_result";
   public const string Audio = "Audio Sentence";
   public const string Screenshot = "Screenshot";
   public const string Configuration = "__configuration";
}

public static class CardTypes
{
   public const string Reading = "Reading";
   public const string Listening = "Listening";
}

public static class NoteTypes
{
   public const string ImmersionKit = "Immersion Kit Sentence";
   public const string Kanji = "_Kanji";
   public const string Vocab = "_Vocab";
   public const string Sentence = "_japanese_sentence";

   public static readonly HashSet<string> All = [Kanji, Vocab, Sentence];

   public static readonly IReadOnlyList<string> AllList = [Kanji, Vocab, Sentence];

   public static Func<Guid, NoteId> IdFactoryFromName(string noteTypeName) => IDFactoriesByName[noteTypeName];

   //create a dictionary from type to string for our supported type her

   static readonly Dictionary<string, Func<Guid, NoteId>> IDFactoriesByName = new()
                                                                              {
                                                                                 [Kanji] = g => new KanjiId(g),
                                                                                 [Vocab] = g => new VocabId(g),
                                                                                 [Sentence] = g => new SentenceId(g),
                                                                              };
}

static class NoteFieldsConstants
{
   public const string NoteId = "nid";

   internal static class VocabNoteType
   {
      internal static class Card
      {
         public const string Reading = CardTypes.Reading;
         public const string Listening = CardTypes.Listening;
      }
   }

   public static class SentencesNoteType
   {
      public static class Card
      {
         public const string Reading = CardTypes.Reading;
         public const string Listening = CardTypes.Listening;
      }
   }

   internal static class Kanji
   {
      public const string Question = MyNoteFields.Question;
      public const string ActiveAnswer = MyNoteFields.Answer;
      public const string SourceAnswer = "source_answer";
      public const string UserAnswer = "__answer";
      public const string ReadingOn = "Reading_On";
      public const string ReadingKun = "Reading_Kun";
      public const string ReadingNan = "__reading_Nan";
      public const string Radicals = "Radicals";
      public const string SourceMeaningMnemonic = "Meaning_Mnemonic";
      public const string MeaningInfo = "Meaning_Info";
      public const string ReadingMnemonic = "Reading_Mnemonic";
      public const string ReadingInfo = "Reading_Info";
      public const string PrimaryVocab = "__primary_Vocab";
      public const string Audio = "__audio";
      public const string PrimaryReadingsTTSAudio = "_primary_readings_tts_audio";
      public const string References = "__references";
      public const string UserMnemonic = "__mnemonic";
      public const string UserSimilarMeaning = "__similar_meaning";
      public const string RelatedConfusedWith = "__confused_with";
      public const string Image = "_image";
   }

   internal static class Vocab
   {
      public const string MatchingRules = "__matching_rules";
      public const string RelatedVocab = "__related_vocab";
      public const string SentenceCount = "sentence_count";
      public const string Question = MyNoteFields.Question;
      public const string ActiveAnswer = MyNoteFields.Answer;
      public const string SourceAnswer = "source_answer";
      public const string UserAnswer = "__answer";
      public const string UserExplanation = "__explanation";
      public const string UserExplanationLong = "__explanation_long";
      public const string UserCompounds = "__compounds";
      public const string UserMnemonic = "__mnemonic";
      public const string Reading = "Reading";
      public const string PartsOfSpeech = "TOS";
      public const string SourceMnemonic = "source_mnemonic";
      public const string AudioB = "Audio_b";
      public const string AudioG = "Audio_g";
      public const string AudioTTS = "Audio_TTS";
      public const string Kanji = "__kanji";
      public const string Forms = "F";
      public const string SourceReadingMnemonic = "source_reading_mnemonic";
      public const string TechnicalNotes = "__technical_notes";
      public const string References = "__references";
      public const string Image = "Image";
      public const string UserImage = "__image";
   }
}

public static class Builtin
{
   public const string Tag = "tag";
   public const string Note = "note";
   public const string Deck = "deck";
   public const string Card = "card";
}

public static class Mine
{
   public const string AppName = "JA-Studio";

   // ReSharper disable once UnusedMember.Global - Used from Python (dotnet_rendering_content_renderer_anki_shim.py, common.py)
   public static readonly string AppStillLoadingMessage = $"{AppName} still loading, the view will refresh when done...";
   public const string VocabPrefixSuffixMarker = "〜";
}
