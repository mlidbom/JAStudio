using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Storage.Media;
using MemoryPack;

namespace JAStudio.Core.Note;

public abstract class JPNote
{
   public NoteFlushGuard RecursiveFlushGuard { get; }
   public NoteGuard Guard { get; }
   public NoteServices Services { get; }
   bool _persisted;
   byte[]? _lastPersistedSnapshot;

   // Dictionary tracks card suspend status: true = active/unsuspended, false = suspended
   readonly Dictionary<string, bool> _cardStatus = new();

   /// <summary>The note's own tags: the corpus's, stored with the note and edited in JAStudio.</summary>
   public NoteTags Tags { get; }

   /// <summary>The tags the note's study-note has in Anki, below <c>anki::</c>: the student's, read from Anki and never stored or changed in JAStudio.</summary>
   public AnkiTags AnkiTags { get; } = new();

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

   public void SetStudyingStatus(CardStudyingStatus status)
   {
      // Update card status: true = active/unsuspended, false = suspended
      _cardStatus[status.CardType] = !status.IsSuspended;
   }

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

   public bool IsStudying(string? cardType = null)
   {
      if(cardType == null)
      {
         // Return true if ANY card is active/unsuspended
         return _cardStatus.Any(kvp => kvp.Value);
      }

      // Return true if specific card type is active/unsuspended
      return _cardStatus.TryGetValue(cardType, out var isActive) && isActive;
   }

   /// <summary>Whether the note has an Anki note in this Anki profile, which holds its cards and their review history.</summary>
   public bool HasAnAnkiNote() => Services.ExternalNoteIdMap.ToExternalId(_id).HasValue;

   public bool IsStudyingRead() => IsStudying(CardTypes.Reading);
   public bool IsStudyingListening() => IsStudying(CardTypes.Listening);

   public void SuspendAllCards()
   {
      Services.CardOperations.SuspendAllCardsForNote(_id);
      SetTheStatusOfEveryKnownCard(isActive: false);
      Flush();
   }

   public void UnsuspendAllCards()
   {
      Services.CardOperations.UnsuspendAllCardsForNote(_id);
      SetTheStatusOfEveryKnownCard(isActive: true);
      Flush();
   }

   /// <summary>
   /// Marks every card type the note has a status for as active, that is unsuspended, or as suspended, after the studying-add-on was told to do the same,<br/>
   /// and notifies the change through <see cref="JPCollection.StudyingStatusChanged"/>, since the status isn't stored with the note and no flush reports it.
   /// </summary>
   void SetTheStatusOfEveryKnownCard(bool isActive)
   {
      foreach(var cardType in _cardStatus.Keys.ToList())
         _cardStatus[cardType] = isActive;
      Services.Collection.NotifyThatTheStudyingStatusChangedFor(this);
   }

   /// <summary>
   /// Forgets the note's Anki note, after it was deleted in Anki: the studying status of every card, since with no cards in this Anki profile the note<br/>
   /// isn't studied in it, and its <see cref="AnkiTags"/>. Notifies the change through <see cref="JPCollection.StudyingStatusChanged"/>, since neither<br/>
   /// is stored with the note and no flush reports it.
   /// </summary>
   public void ForgetItsDeletedAnkiNote()
   {
      _cardStatus.Clear();
      AnkiTags.Forget();
      Services.Collection.NotifyThatTheStudyingStatusChangedFor(this);
   }

   public bool HasSuspendedCards()
   {
      // Return true if ANY card is suspended (value == false)
      return _cardStatus.Any(kvp => !kvp.Value);
   }

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

   public virtual void UpdateGeneratedData()
   {
      // Override in subclasses
   }

   public abstract List<MediaReference> MediaReferences { get; }

   public abstract CorpusNoteDto ToDto();

   byte[] TakeSnapshot() => MemoryPackSerializer.Serialize(ToDto());

   internal void MarkAsPersisted()
   {
      _persisted = true;
      _lastPersistedSnapshot = TakeSnapshot();
   }

   /// <summary>
   /// Whether this note's data, as last saved or loaded, is the same as <paramref name="theNoteAsHeldBefore"/>'s, the same note as the collection held<br/>
   /// it before reloading: not the same when the note's file changed on disk in between (<see cref="Collection.JPCollection.ReloadFromBackendAfter"/>).
   /// </summary>
   internal bool HasTheSameStoredDataAs(JPNote theNoteAsHeldBefore) =>
      _lastPersistedSnapshot.AsSpan().SequenceEqual(theNoteAsHeldBefore._lastPersistedSnapshot);

   public void Flush()
   {
      if(!_persisted) return;

      var currentSnapshot = TakeSnapshot();
      if(_lastPersistedSnapshot != null && currentSnapshot.AsSpan().SequenceEqual(_lastPersistedSnapshot))
         return;

      _lastPersistedSnapshot = currentSnapshot;
      RecursiveFlushGuard.Flush();
   }

   /// <summary>The number in the note's priority tag (<see cref="Tagging.Tags.PriorityFolder"/>), an Anki tag, or 0 when it has none: the lower, the sooner a vocab lists the sentence.</summary>
   public int PriorityTagValue()
   {
      foreach(var tag in AnkiTags)
      {
         if(tag.Name.StartsWith(Tagging.Tags.PriorityFolder))
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
         if(tag.Name.StartsWith(Tagging.Tags.PriorityFolder))
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
      if(Tags.Contains(Tagging.Tags.TTSAudio)) tags.Add("tts_audio");

      return tags;
   }

   static readonly SourceTag UnknownSourceTag = SourceTag.Parse("anki::unknown");

   public SourceTag SourceTag
   {
      get
      {
         var sourceTags = Tags.Where(t => t.Name.StartsWith(Tagging.Tags.Source.Folder)).ToList();
         if(sourceTags.Any())
         {
            var sorted = sourceTags.OrderBy(t => t.Name.Length).ToList();
            return SourceTag.Parse(sorted[0].Name);
         }

         return UnknownSourceTag;
      }
   }
}
