using Compze.Internals.SystemCE;
using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.Studying;
using MemoryPack;

namespace JAStudio.Core.Note;

public abstract class JPNote
{
   public NoteFlushGuard RecursiveFlushGuard { get; }
   public NoteGuard Guard { get; }
   public NoteServices Services { get; }
   bool _persisted;

   /// <summary>
   /// The 128-bit hash of the note's snapshot as it was last saved or loaded (<see cref="TakeSnapshot"/>), to tell whether the note changed since.<br/>
   /// The hash, not the snapshot: the snapshots took 177 MB right after a start, 2.2 KB a note (2026-11-12). Two snapshots with the same<br/>
   /// 128-bit hash, which would make a change go unsaved, won't happen.
   /// </summary>
   UInt128? _hashOfTheLastPersistedSnapshot;

   /// <summary>The note's own tags: the corpus's, stored with the note and edited in JAStudio.</summary>
   public NoteTags Tags { get; }

   /// <summary>The tags the note's study-note has in Anki, below <c>anki::</c>: the student's, read from Anki and never stored or changed in JAStudio.</summary>
   public AnkiTags AnkiTags => Services.WhatTheSRSHoldsOfTheStudyNotes.AnkiTagsOf(_id);

   /// <summary>Every tag the note shows: its own <see cref="Tags"/> and its <see cref="AnkiTags"/>. These are what a person browses and searches.</summary>
   public IEnumerable<Tag> AllTags => Tags.Concat(AnkiTags);

   readonly NoteId _id;

   protected JPNote(NoteServices services, NoteId id, List<string>? tags = null)
   {
      Services = services;
      RecursiveFlushGuard = new NoteFlushGuard(this);
      Guard = new NoteGuard(Flush);
      Tags = new NoteTags(this, tags);
      _id = id;
   }

   /// <summary>Holds <paramref name="status"/> as the studying status of the card it is of, of the note's study-note (<see cref="WhatTheSRSHoldsOfTheStudyNotes"/>).</summary>
   public void SetStudyingStatus(CardStudyingStatus status) => Services.WhatTheSRSHoldsOfTheStudyNotes.SetStudyingStatus(_id, status);

   /// <summary>
   /// The fields JAStudio writes into the note's Anki study-note, by their names in Anki: the <see cref="StudyNoteFieldsBesidesTheId"/>, and the id of<br/>
   /// this note, which links the two.
   /// </summary>
   /// <remarks>
   /// JAStudio writes no tags into a study-note: the note's tags are the corpus's, and the study-note's tags in Anki are the student's, such as Anki's<br/>
   /// own <c>leech</c> and <c>marked</c>. The two sets are separate (Magnus, 2026-11-10).
   /// </remarks>
   public Dictionary<string, string> StudyNoteFields()
   {
      var fields = StudyNoteFieldsBesidesTheId();
      fields[AnkiFieldNames.JasNoteId] = _id.Value.ToString();
      return fields;
   }

   /// <summary>The fields JAStudio writes into the note's Anki study-note besides <see cref="AnkiFieldNames.JasNoteId"/>: its question and its audio.</summary>
   protected abstract Dictionary<string, string> StudyNoteFieldsBesidesTheId();

   public override bool Equals(object? obj) => obj is JPNote other && other.GetId() == _id;

   public abstract void UpdateInCache();

   /// <summary>
   /// Whether the note is studied: through the card of <paramref name="cardType"/>, such as <see cref="CardTypes.Reading"/>, or through any of its<br/>
   /// cards when it's null. A card that isn't suspended is studied.
   /// </summary>
   public bool IsStudying(string? cardType = null) => Services.WhatTheSRSHoldsOfTheStudyNotes.IsStudying(_id, cardType);

   /// <summary>
   /// Whether the note has a study-note in the SRS, the spaced repetition system, which holds its cards and their review history: in Anki, an Anki<br/>
   /// note in the Anki profile JAStudio works with.
   /// </summary>
   /// <remarks>review-naming:studying: HasAStudyNote was HasAnAnkiNote (Claude, 2026-11-15).</remarks>
   public bool HasAStudyNote() => Services.StudyNoteIds.StudyNoteIdOf(_id).HasValue;

   public bool IsStudyingRead() => IsStudying(CardTypes.Reading);
   public bool IsStudyingListening() => IsStudying(CardTypes.Listening);

   public void SuspendAllCards()
   {
      Services.SRSStudyNotes.SuspendAllCardsForNote(_id);
      SetTheStatusOfEveryKnownCard(isActive: false);
   }

   public void UnsuspendAllCards()
   {
      Services.SRSStudyNotes.UnsuspendAllCardsForNote(_id);
      SetTheStatusOfEveryKnownCard(isActive: true);
   }

   /// <summary>
   /// Marks every card type the note has a status for as active, that is unsuspended, or as suspended, after the spaced repetition system was told to do the same,<br/>
   /// and notifies the change through <see cref="JPCollection.StudyNoteChanged"/>, since the status isn't stored with the note and no flush reports it.
   /// </summary>
   void SetTheStatusOfEveryKnownCard(bool isActive)
   {
      Services.WhatTheSRSHoldsOfTheStudyNotes.SetTheStatusOfEveryKnownCard(_id, isActive);
      Services.Collection.NotifyThatTheStudyNoteChangedFor(this);
   }

   public bool HasSuspendedCards() => Services.WhatTheSRSHoldsOfTheStudyNotes.HasSuspendedCards(_id);

   public bool HasActiveCards() => IsStudying();

   public override int GetHashCode() => _id.GetHashCode();

   public override string ToString() => $"{GetQuestion()}: {GetAnswer()}";

   public JPCollection Collection => Services.Collection;

   public abstract string GetQuestion();

   public abstract string GetAnswer();

   /// <summary>
   /// The texts a search for the note's searchable text looks in (<see cref="Search.NoteProperties.SearchableText"/>): its question and answer,<br/>
   /// and for vocab and kanji also their readings, in kana and in romaji, so that typing <c>taberu</c> finds 食べる.
   /// </summary>
   public virtual IEnumerable<string> SearchableTexts() => [GetQuestion(), GetAnswer()];

   public NoteId GetId() => _id;

   /// <summary>
   /// Recomputes the note's generated data: what JAStudio derives from the note and the rest of the corpus, such as a vocab's sentence count or a<br/>
   /// sentence's parsing result.
   /// </summary>
   /// <remarks>
   /// It changes this note only, never another, so that the full rebuild (<see cref="Batches.LocalNoteUpdater.FullRebuild"/>) can run it for many notes<br/>
   /// at once, on many threads: a change to another note would race with that note's own update, and the result would depend on the order. A change<br/>
   /// that must reach other notes is made when what causes it changes, as a perfect synonym's answer is (<see cref="Vocabulary.RelatedVocab.PerfectSynonyms"/>).
   /// </remarks>
   public virtual void UpdateGeneratedData() {}

   /// <summary>Every media file the note references: its source's and the corpus's own, of every kind.</summary>
   public abstract IEnumerable<MediaReference> MediaReferences { get; }

   public abstract CorpusNoteDto ToDto();

   byte[] TakeSnapshot() => MemoryPackSerializer.Serialize(ToDto());

   UInt128 HashOfASnapshotTakenNow() => XxHash128.HashToUInt128(TakeSnapshot());

   internal void MarkAsPersisted()
   {
      _persisted = true;
      _hashOfTheLastPersistedSnapshot = HashOfASnapshotTakenNow();
   }

   /// <summary>
   /// Whether this note's data, as last saved or loaded, is the same as <paramref name="theNoteAsHeldBefore"/>'s, the same note as the collection held<br/>
   /// it before reloading: not the same when the note's file changed on disk in between (<see cref="Collection.JPCollection.ReloadFromBackendAfter"/>).
   /// </summary>
   internal bool HasTheSameStoredDataAs(JPNote theNoteAsHeldBefore) =>
      _hashOfTheLastPersistedSnapshot == theNoteAsHeldBefore._hashOfTheLastPersistedSnapshot;

   public void Flush()
   {
      if(!_persisted) return;

      var hashOfTheSnapshotNow = HashOfASnapshotTakenNow();
      if(hashOfTheSnapshotNow == _hashOfTheLastPersistedSnapshot)
         return;

      _hashOfTheLastPersistedSnapshot = hashOfTheSnapshotNow;
      RecursiveFlushGuard.Flush();
   }

   /// <summary>The number in the note's priority tag (<see cref="Tagging.Tags.PriorityFolder"/>), an Anki tag, or 0 when it has none: the lower, the sooner a vocab lists the sentence.</summary>
   public int PriorityTagValue()
   {
      foreach(var tag in AnkiTags)
      {
         if(tag.Name.StartsWithOrdinal(Tagging.Tags.PriorityFolder))
         {
            var numberStr = StringExtensions.FirstNumber(tag.Name);
            if(int.TryParse(numberStr, out var num))
            {
               return num;
            }
         }
      }

      return 0;
   }

   public HashSet<string> GetMetaTags()
   {
      var tags = new HashSet<string>();

      foreach(var tag in AnkiTags)
      {
         if(tag.Name.StartsWithOrdinal(Tagging.Tags.PriorityFolder))
         {
            if(tag.Name.Contains("high")) tags.Add("high_priority");
            if(tag.Name.Contains("low")) tags.Add("low_priority");
         }
      }

      if(IsStudying(CardTypes.Reading) || IsStudying(CardTypes.Listening))
      {
         tags.Add("is_studying");
      }

      if(IsStudying(CardTypes.Reading)) tags.Add("is_studying_reading");
      if(IsStudying(CardTypes.Listening)) tags.Add("is_studying_listening");
      if(this is IHasTextToSpeechAudio { AllAudio.IsAllTextToSpeech: true }) tags.Add("tts_audio");

      return tags;
   }

   static readonly SourceTag UnknownSourceTag = SourceTag.Parse("anki::unknown");

   public SourceTag SourceTag
   {
      get
      {
         var sourceTags = Tags.Where(t => t.Name.StartsWithOrdinal(Tagging.Tags.Source.Folder)).ToList();
         if(sourceTags.Any())
         {
            var sorted = sourceTags.OrderBy(t => t.Name.Length).ToList();
            return SourceTag.Parse(sorted[0].Name);
         }

         return UnknownSourceTag;
      }
   }
}
