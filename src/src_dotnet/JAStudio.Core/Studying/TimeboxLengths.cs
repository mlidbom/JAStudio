using System.Collections.Generic;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying;

/// <summary>
/// How long a timebox is when studying the cards of one card type of one note type, by the settings of <see cref="JapaneseConfig"/>: the length of<br/>
/// a stretch of reviewing, after which the spaced repetition system says how many cards were reviewed in it.
/// </summary>
/// <remarks>
/// The Anki add-on asks it of JAStudio, through the card server, as Anki shows a deck's overview, for the note type and card type of the deck's<br/>
/// first card, and sets Anki's timebox to it.
/// </remarks>
/// <remarks>review-naming:anki-http: TimeboxLengths is Claude's (2026-11-15).</remarks>
public sealed class TimeboxLengths(JapaneseConfig config)
{
   readonly JapaneseConfig _config = config;

   /// <summary>
   /// The minutes of a timebox for the cards of the card type named <paramref name="cardType"/>, such as <see cref="CardTypes.Reading"/>, of the note<br/>
   /// type named <paramref name="noteType"/>, such as <see cref="NoteTypes.Vocab"/>. Null for cards JAStudio has no timebox for.
   /// </summary>
   public long? MinutesFor(string noteType, string cardType) =>
      TheSettingsByNoteTypeAndCardType().GetValueOrDefault((noteType, cardType))?.Value;

   Dictionary<(string NoteType, string CardType), ConfigurationValue<long>> TheSettingsByNoteTypeAndCardType() =>
      new()
      {
         [(NoteTypes.Sentence, CardTypes.Reading)] = _config.TimeboxSentenceRead,
         [(NoteTypes.Sentence, CardTypes.Listening)] = _config.TimeboxSentenceListen,
         [(NoteTypes.Vocab, CardTypes.Reading)] = _config.TimeboxVocabRead,
         [(NoteTypes.Vocab, CardTypes.Listening)] = _config.TimeboxVocabListen,
         [(NoteTypes.Kanji, CardTypes.Reading)] = _config.TimeboxKanjiRead
      };
}
