using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.NoteFields;
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

   public NoteMedia Media => Services.MediaFileIndex.GetNoteMedia(GetId());

   // Dictionary tracks card suspend status: true = active/unsuspended, false = suspended
   readonly Dictionary<string, bool> _cardStatus = new();

   public void CopyStudyingStatusFrom(JPNote other)
   {
      foreach(var kvp in other._cardStatus)
      {
         _cardStatus[kvp.Key] = kvp.Value;
      }
   }

   public NoteTags Tags { get; }

   readonly NoteId _id;

   protected JPNote(NoteServices services, NoteId id, List<string>? tags = null)
   {
      Services = services;
      RecursiveFlushGuard = new NoteFlushGuard(this);
      Guard = new NoteGuard(Flush);
      Tags = new NoteTags(this, tags);
      _id = id;
   }

   public bool IsFlushing => RecursiveFlushGuard.IsFlushing;

   public void SetStudyingStatus(CardStudyingStatus status)
   {
      // Update card status: true = active/unsuspended, false = suspended
      _cardStatus[status.CardType] = !status.IsSuspended;
   }

   // ReSharper disable once UnusedMember.Global Used from python
   public AnkiNoteData GetData() => ToCorpusData().ToNoteData();

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

   public bool IsStudyingRead() => IsStudying(CardTypes.Reading);
   public bool IsStudyingListening() => IsStudying(CardTypes.Listening);

   public void SuspendAllCards()
   {
      Services.CardOperations.SuspendAllCardsForNote(_id);

      // Update local status for all known card types
      var cardTypes = _cardStatus.Keys.ToList();
      foreach(var cardType in cardTypes)
      {
         _cardStatus[cardType] = false; // false = suspended
      }

      Flush();
   }

   public void UnsuspendAllCards()
   {
      Services.CardOperations.UnsuspendAllCardsForNote(_id);

      // Update local status for all known card types
      var cardTypes = _cardStatus.Keys.ToList();
      foreach(var cardType in cardTypes)
      {
         _cardStatus[cardType] = true; // true = active/unsuspended
      }

      Flush();
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

   public virtual void OnTagsUpdated()
   {
      // Called when tags are modified. Subclasses can override to invalidate cached state.
   }

   public NoteId GetId() => _id;

   public virtual void UpdateGeneratedData()
   {
      // Override in subclasses
   }

   public abstract List<MediaReference> MediaReferences { get; }

   public abstract CorpusObjectData ToCorpusData();

   byte[] TakeSnapshot() => MemoryPackSerializer.Serialize(ToCorpusData());

   internal void MarkAsPersisted()
   {
      _persisted = true;
      _lastPersistedSnapshot = TakeSnapshot();
   }

   public void Flush()
   {
      if(!_persisted) return;

      var currentSnapshot = TakeSnapshot();
      if(_lastPersistedSnapshot != null && currentSnapshot.AsSpan().SequenceEqual(_lastPersistedSnapshot))
         return;

      _lastPersistedSnapshot = currentSnapshot;
      RecursiveFlushGuard.Flush();
   }

   public int PriorityTagValue()
   {
      foreach(var tag in Tags)
      {
         if(tag.Name.StartsWith(Note.Tags.PriorityFolder))
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

      foreach(var tag in Tags)
      {
         if(tag.Name.StartsWith(Note.Tags.PriorityFolder))
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
      if(Tags.Contains(Note.Tags.TTSAudio)) tags.Add("tts_audio");

      return tags;
   }

   public string GetSourceTag()
   {
      var sourceTags = Tags.Where(t => t.Name.StartsWith(Note.Tags.Source.Folder)).ToList();
      if(sourceTags.Any())
      {
         var sorted = sourceTags.OrderBy(t => t.Name.Length).ToList();
         return sorted[0].Name.Substring(Note.Tags.Source.Folder.Length);
      }

      return string.Empty;
   }
}
