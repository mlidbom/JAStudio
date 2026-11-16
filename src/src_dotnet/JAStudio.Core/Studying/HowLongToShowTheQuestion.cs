using System;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Studying;

/// <summary>
/// How long the spaced repetition system's reviewer shows the question of one of a note's reading cards before it shows the answer by itself: its<br/>
/// auto-advance. A vocab's or a sentence's question is given time for each of its characters, by the settings of <see cref="JapaneseConfig"/>, and<br/>
/// a kanji's the time its deck gives. A card failed again and again today is given more each time, while
/// <see cref="JapaneseConfig.BoostFailedCardAllowedTime"/> is on.
/// </summary>
/// <remarks>
/// The Anki add-on asks it of JAStudio for each card it shows the question of, through the card server, and passes what only Anki knows: the time<br/>
/// the card's deck gives, and the card's again answers today.
/// </remarks>
/// <remarks>review-naming:anki-http: HowLongToShowTheQuestion is Claude's (2026-11-15).</remarks>
public sealed class HowLongToShowTheQuestion(JapaneseConfig config)
{
   readonly JapaneseConfig _config = config;

   /// <summary>
   /// The seconds to show the question of <paramref name="note"/>'s reading card, whose deck gives <paramref name="secondsTheDeckGives"/>, and<br/>
   /// whose latest answers today were <paramref name="againAnswersInARowToday"/> again answers in a row.
   /// </summary>
   public double Seconds(JPNote note, double secondsTheDeckGives, int againAnswersInARowToday) =>
      TheTimeTheQuestionIsGiven(note, secondsTheDeckGives) * BoostForFailing(againAnswersInARowToday);

   double TheTimeTheQuestionIsGiven(JPNote note, double secondsTheDeckGives) =>
      note switch
      {
         SentenceNote sentence => new DifficultyCalculator(startingSeconds: _config.AutoadvanceSentenceStartingSeconds.Value,
                                                           hiraganaSeconds: _config.AutoadvanceSentenceHiraganaSeconds.Value,
                                                           katakataSeconds: _config.AutoadvanceSentenceKatakanaSeconds.Value,
                                                           kanjiSeconds: _config.AutoadvanceSentenceKanjiSeconds.Value).AllowedSeconds(sentence.GetQuestion()),
         VocabNote vocab => new DifficultyCalculator(startingSeconds: _config.AutoadvanceVocabStartingSeconds.Value,
                                                     hiraganaSeconds: _config.AutoadvanceVocabHiraganaSeconds.Value,
                                                     katakataSeconds: _config.AutoadvanceVocabKatakanaSeconds.Value,
                                                     kanjiSeconds: _config.AutoadvanceVocabKanjiSeconds.Value).AllowedSeconds(vocab.GetQuestion()),
         KanjiNote => secondsTheDeckGives,
         _ => throw new ArgumentOutOfRangeException(nameof(note), note.GetType(), "Only vocab, kanji and sentences have cards.")
      };

   double BoostForFailing(int againAnswersInARowToday) =>
      _config.BoostFailedCardAllowedTime.Value ? Math.Pow(_config.BoostFailedCardAllowedTimeByFactor.Value, againAnswersInARowToday) : 1;
}
