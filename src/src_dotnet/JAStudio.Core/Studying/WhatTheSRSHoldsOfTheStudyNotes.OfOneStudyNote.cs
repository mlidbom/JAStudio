using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Studying;

public partial class WhatTheSRSHoldsOfTheStudyNotes
{
   /// <summary>
   /// What the spaced repetition system holds of one study-note that JAStudio reads: whether each of its cards is active, that is unsuspended,<br/>
   /// by the card's type, and its <see cref="Note.Tagging.AnkiTags"/>. Never changed: a change makes another.
   /// </summary>
   sealed class OfOneStudyNote(IReadOnlyDictionary<string, bool> isActiveByCardType, AnkiTags ankiTags)
   {
      /// <summary>What is held of a study-note with no cards and no tags, as of a note that has no study-note.</summary>
      public static readonly OfOneStudyNote None = new(new Dictionary<string, bool>(), AnkiTags.None);

      readonly IReadOnlyDictionary<string, bool> _isActiveByCardType = isActiveByCardType;

      public AnkiTags AnkiTags { get; } = ankiTags;

      public bool IsNothing => _isActiveByCardType.Count == 0 && !AnkiTags.Any();

      public bool HasSuspendedCards => _isActiveByCardType.Values.Any(isActive => !isActive);

      public bool IsStudying(string? cardType) =>
         cardType == null
            ? _isActiveByCardType.Values.Any(isActive => isActive)
            : _isActiveByCardType.TryGetValue(cardType, out var isActive) && isActive;

      public OfOneStudyNote WithTheCard(string cardType, bool isActive) =>
         new(new Dictionary<string, bool>(_isActiveByCardType) { [cardType] = isActive }, AnkiTags);

      public OfOneStudyNote WithEveryCard(bool isActive) =>
         new(_isActiveByCardType.Keys.ToDictionary(cardType => cardType, _ => isActive), AnkiTags);

      public bool IsTheSameAs(OfOneStudyNote other) =>
         AnkiTags.AreTheSameAs(other.AnkiTags) && TheSameCardsAs(other);

      bool TheSameCardsAs(OfOneStudyNote other) =>
         other._isActiveByCardType.Count == _isActiveByCardType.Count
         && other._isActiveByCardType.All(it => _isActiveByCardType.TryGetValue(it.Key, out var isActive) && isActive == it.Value);
   }
}
