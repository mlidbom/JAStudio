using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Reservoir.Analysis;

/// <summary>
/// The numbers that a reservoir's analyses (<see cref="ReservoirSentenceAnalysis"/>) give vocab and failure reasons, in place of a vocab's note id<br/>
/// and a reason's text, which keeps each analysis small. They're kept in the reservoir's file, and a number, once given, stays. Numbers can be<br/>
/// given from several threads at once, as sentences are analyzed in parallel.
/// </summary>
public sealed class ReservoirAnalysisNumbers
{
   readonly ConcurrentDictionary<string, NumberedVocab> _vocabByNoteId;
   readonly ConcurrentDictionary<string, int> _reasonNumbers;
   int _lastVocabNumber;
   int _lastReasonNumber;

   public ReservoirAnalysisNumbers(IEnumerable<NumberedVocab> vocab, IEnumerable<KeyValuePair<string, int>> reasonNumbers)
   {
      _vocabByNoteId = new ConcurrentDictionary<string, NumberedVocab>(vocab.Select(it => KeyValuePair.Create(it.NoteId, it)));
      _reasonNumbers = new ConcurrentDictionary<string, int>(reasonNumbers);
      _lastVocabNumber = _vocabByNoteId.Values.Select(it => it.Number).DefaultIfEmpty(0).Max();
      _lastReasonNumber = _reasonNumbers.Values.DefaultIfEmpty(0).Max();
   }

   /// <summary>The number of <paramref name="vocab"/>, given now if it has none yet.</summary>
   public int VocabNumber(VocabNote vocab) =>
      _vocabByNoteId.GetOrAdd(vocab.GetId().ToString(), noteId => new NumberedVocab(Interlocked.Increment(ref _lastVocabNumber), noteId, vocab.GetQuestion())).Number;

   /// <summary>The number of the failure reason <paramref name="reason"/>, given now if it has none yet.</summary>
   public int ReasonNumber(string reason) => _reasonNumbers.GetOrAdd(reason, _ => Interlocked.Increment(ref _lastReasonNumber));

   /// <summary>Every vocab given a number.</summary>
   public IReadOnlyCollection<NumberedVocab> Vocab => [.. _vocabByNoteId.Values];

   /// <summary>Every failure reason given a number, with it.</summary>
   public IReadOnlyCollection<KeyValuePair<string, int>> Reasons => [.. _reasonNumbers];
}
