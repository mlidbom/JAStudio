using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq;
using Compze.Internals.SystemCE;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Storage.Media;
using JAStudio.Core.Studying;
using MemoryPack;

namespace JAStudio.Core.Note;

/// <summary>
/// A note of the corpus: a vocab, a kanji or a sentence. A Compze taggregate, whose changes are tevents, each an <see cref="INoteTevent"/>, kept by<br/>
/// the state store that <see cref="AppBootstrapper"/> composes.
/// </summary>
/// <remarks>
/// wip: most of a note's parts still change its state directly, and <see cref="Flush"/> publishes <see cref="INoteTevent.PartWithoutTeventsChanged"/><br/>
/// for them (dev_docs/taggregates/shared/the-migration.md).
/// </remarks>
public abstract class JPNote : Taggregate<JPNote, INoteTevent, NoteTevent, INoteTevent<INoteTevent>, NoteTevent<INoteTevent>>, ITeventRouted
{
   /// <summary>The tevent appliers of every note, which each note class's own extend.</summary>
   protected static readonly TeventRouting<JPNote> NoteAppliers = TeventRouting<JPNote>.DeclareAppliers(it => it
     .IgnoreUnhandled<INoteTevent.PartWithoutTeventsChanged>());

   TeventRouting ITeventRouted.TeventAppliers => NoteAppliers;

   public NoteGuard Guard { get; }
   public NoteServices Services { get; }

   /// <summary>
   /// The 128-bit hash of the note's serialized <see cref="ToDto"/> as it was restored, or as it was when <see cref="Flush"/> last published<br/>
   /// its change, to tell whether the note changed since. Null for a note being created, never stored, whose created tevent publishes it whole.<br/>
   /// The hash, not the snapshot: the snapshots took 177 MB right after a start, 2.2 KB a note (2026-11-12). Two snapshots with the same<br/>
   /// 128-bit hash, which would make a change go unsaved, won't happen.
   /// </summary>
   UInt128? _hashOfTheStateLastPublished;

   /// <summary>The note's own tags: the corpus's, stored with the note and edited in JAStudio.</summary>
   public NoteTags Tags { get; private set; }

   /// <summary>The tags the note's study-note has in Anki, below <c>anki::</c>: the student's, read from Anki and never stored or changed in JAStudio.</summary>
   public AnkiTags AnkiTags => Services.WhatTheSRSHoldsOfTheStudyNotes.AnkiTagsOf(Id);

   /// <summary>Every tag the note shows: its own <see cref="Tags"/> and its <see cref="AnkiTags"/>. These are what a person browses and searches.</summary>
   public IEnumerable<Tag> AllTags => Tags.Concat(AnkiTags);

   /// <summary>An empty note, which is no note yet: it gets its id, and so becomes a note, from its class's created tevent or by being restored.</summary>
   protected JPNote(NoteServices services)
   {
      Services = services;
      Guard = new NoteGuard(AssertCanBeChanged, Flush);
      Tags = new NoteTags(this);
   }

   public override NoteId Id => (NoteId)base.Id;

   /// <summary>
   /// Gives this empty note, being restored from <paramref name="data"/>, what every note stores: its id, <paramref name="id"/>, which the note's<br/>
   /// class makes of <paramref name="data"/>'s, and its <see cref="Tags"/>. A note's restoring calls it first. The note starts from the state<br/>
   /// <paramref name="data"/> holds, so that <see cref="Flush"/> publishes a change from it.
   /// </summary>
   protected void RestoreTheNoteFrom(NoteId id, CorpusNoteDto data)
   {
      RestoreIdentity(id);
      Tags = new NoteTags(this, data.Tags);
      _hashOfTheStateLastPublished = HashOf(data);
   }

   /// <summary>Holds <paramref name="status"/> as the studying status of the card it is of, of the note's study-note (<see cref="WhatTheSRSHoldsOfTheStudyNotes"/>).</summary>
   public void SetStudyingStatus(CardStudyingStatus status) => Services.WhatTheSRSHoldsOfTheStudyNotes.SetStudyingStatus(Id, status);

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
      fields[AnkiFieldNames.JasNoteId] = Id.Value.ToString();
      return fields;
   }

   /// <summary>The fields JAStudio writes into the note's Anki study-note besides <see cref="AnkiFieldNames.JasNoteId"/>: its question and its audio.</summary>
   protected abstract Dictionary<string, string> StudyNoteFieldsBesidesTheId();

   public override bool Equals(object? obj) => obj is JPNote other && other.Id == Id;

   /// <summary>
   /// Whether the note is studied: through the card of <paramref name="cardType"/>, such as <see cref="CardTypes.Reading"/>, or through any of its<br/>
   /// cards when it's null. A card that isn't suspended is studied.
   /// </summary>
   public bool IsStudying(string? cardType = null) => Services.WhatTheSRSHoldsOfTheStudyNotes.IsStudying(Id, cardType);

   /// <summary>
   /// Whether the note has a study-note in the SRS, the spaced repetition system, which holds its cards and their review history: in Anki, an Anki<br/>
   /// note in the Anki profile JAStudio works with.
   /// </summary>
   /// <remarks>review-naming:studying: HasAStudyNote was HasAnAnkiNote (Claude, 2026-11-15).</remarks>
   public bool HasAStudyNote() => Services.StudyNoteIds.StudyNoteIdOf(Id).HasValue;

   public bool IsStudyingRead() => IsStudying(CardTypes.Reading);
   public bool IsStudyingListening() => IsStudying(CardTypes.Listening);

   public void SuspendAllCards()
   {
      Services.SRSStudyNotes.SuspendAllCardsForNote(Id);
      SetTheStatusOfEveryKnownCard(isActive: false);
   }

   public void UnsuspendAllCards()
   {
      Services.SRSStudyNotes.UnsuspendAllCardsForNote(Id);
      SetTheStatusOfEveryKnownCard(isActive: true);
   }

   /// <summary>
   /// Marks every card type the note has a status for as active, that is unsuspended, or as suspended, after the spaced repetition system was told to do the same,<br/>
   /// and notifies the change through <see cref="JPCollection.StudyNoteChanged"/>, since the status isn't stored with the note and no flush reports it.
   /// </summary>
   void SetTheStatusOfEveryKnownCard(bool isActive)
   {
      Services.WhatTheSRSHoldsOfTheStudyNotes.SetTheStatusOfEveryKnownCard(Id, isActive);
      Services.Collection.NotifyThatTheStudyNoteChangedFor(this);
   }

   public bool HasSuspendedCards() => Services.WhatTheSRSHoldsOfTheStudyNotes.HasSuspendedCards(Id);

   public bool HasActiveCards() => IsStudying();

   public override int GetHashCode() => Id.GetHashCode();

   public override string ToString() => $"{GetQuestion()}: {GetAnswer()}";

   public JPCollection Collection => Services.Collection;

   public abstract string GetQuestion();

   public abstract string GetAnswer();

   /// <summary>
   /// The texts a search for the note's searchable text looks in (<see cref="Search.NoteProperties.SearchableText"/>): its question and answer,<br/>
   /// and for vocab and kanji also their readings, in kana and in romaji, so that typing <c>taberu</c> finds 食べる.
   /// </summary>
   public virtual IEnumerable<string> SearchableTexts() => [GetQuestion(), GetAnswer()];

   public NoteId GetId() => Id;

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

   static UInt128 HashOf(CorpusNoteDto snapshot) => XxHash128.HashToUInt128(MemoryPackSerializer.Serialize(snapshot));

   /// <summary>
   /// When the note's stored data changed since it was restored, or since this last published a change, publishes<br/>
   /// <see cref="INoteTevent.PartWithoutTeventsChanged"/>: a unit of work writes a note that published a change when it commits. A note being<br/>
   /// created, never stored, has nothing to compare with: its created tevent publishes it.
   /// </summary>
   public void Flush()
   {
      if(_hashOfTheStateLastPublished is not {} hashOfTheStateLastPublished) return;

      var hashOfTheStateNow = HashOf(ToDto());
      if(hashOfTheStateNow == hashOfTheStateLastPublished)
         return;

      _hashOfTheStateLastPublished = hashOfTheStateNow;
      Publish(new NoteTevent.PartWithoutTeventsChanged());
   }

   /// <summary>
   /// Publishes the tevent <paramref name="teventOf"/> makes of <paramref name="updated"/>, the new value of a property of the note or of one of<br/>
   /// its parts, unless the property holds it already, as <paramref name="current"/>: a tevent says that something changed.
   /// </summary>
   /// <returns>Whether it published the tevent.</returns>
   internal bool PublishIfChanged<TValue>(TValue current, TValue updated, Func<TValue, NoteTevent> teventOf)
   {
      if(EqualityComparer<TValue>.Default.Equals(updated, current)) return false;
      Publish(teventOf(updated));
      return true;
   }

   /// <summary>
   /// Publishes the tevent <paramref name="teventOf"/> makes of a copy of <paramref name="updated"/>, the new value of a list property of the note or<br/>
   /// of one of its parts, unless the property holds the same elements in the same order already, as <paramref name="current"/>.
   /// </summary>
   /// <returns>Whether it published the tevent.</returns>
   internal bool PublishIfTheListChanged<TElement>(IReadOnlyList<TElement> current, IReadOnlyList<TElement> updated, Func<IReadOnlyList<TElement>, NoteTevent> teventOf)
   {
      if(updated.SequenceEqual(current)) return false;
      Publish(teventOf([.. updated]));
      return true;
   }

   /// <summary>
   /// Publishes <see cref="INoteTevent.PartWithoutTeventsChanged"/> whether or not the note changed since it was restored: for a change to what the<br/>
   /// note stores that the note's state doesn't show, since an object the note shares with others changed, such as a shared-condition its<br/>
   /// match-condition uses by name getting another name. Every instance of the note, its copies included, then stores it by the new name.
   /// </summary>
   protected void PublishThatWhatItStoresChanged()
   {
      AssertCanBeChanged();
      _hashOfTheStateLastPublished = HashOf(ToDto());
      Publish(new NoteTevent.PartWithoutTeventsChanged());
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
