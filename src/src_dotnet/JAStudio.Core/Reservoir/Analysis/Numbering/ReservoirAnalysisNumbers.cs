using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Compze.Threading;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Reservoir.Analysis.Numbering;

/// <summary>
/// The numbers that a reservoir's analyses (<see cref="ReservoirSentenceAnalysis"/>) give vocab and failure reasons, in place of a vocab's note id<br/>
/// and a reason's text, which keeps each analysis small. They're kept in the reservoir's file, and a number, once given, stays. They're looked<br/>
/// up both ways: a vocab's or a reason's number, and what a number stands for.
/// </summary>
/// <remarks>
/// Numbers are given from several threads at once, as sentences are analyzed in parallel. Looking a number up takes no lock; giving a new one<br/>
/// does, which happens once for each vocab and reason.
/// </remarks>
public sealed class ReservoirAnalysisNumbers
{
   readonly IMonitor _givingANewNumber = IMonitor.New();
   readonly ConcurrentDictionary<string, NumberedVocab> _vocabByNoteId = new();
   readonly ConcurrentDictionary<int, NumberedVocab> _vocabByNumber = new();
   readonly ConcurrentDictionary<string, NumberedFailureReason> _reasonByText = new();
   readonly ConcurrentDictionary<int, NumberedFailureReason> _reasonByNumber = new();
   int _lastVocabNumber;
   int _lastReasonNumber;

   /// <param name="vocab">The vocab numbered so far.</param>
   /// <param name="reasons">The failure reasons numbered so far.</param>
   public ReservoirAnalysisNumbers(IEnumerable<NumberedVocab> vocab, IEnumerable<NumberedFailureReason> reasons)
   {
      foreach(var numbered in vocab) Remember(numbered);
      foreach(var numbered in reasons) Remember(numbered);
   }

   /// <summary>The number of <paramref name="vocab"/>, given now if it has none yet.</summary>
   public int VocabNumber(VocabNote vocab) =>
      VocabNumberOrNone(vocab) ?? _givingANewNumber.Locked(() => VocabNumberOrNone(vocab) ?? Remember(new NumberedVocab(_lastVocabNumber + 1, NoteIdOf(vocab), vocab.GetQuestion())).Number);

   /// <summary>The number of the failure reason <paramref name="reason"/>, given now if it has none yet.</summary>
   public int ReasonNumber(string reason) =>
      ReasonNumberOrNone(reason) ?? _givingANewNumber.Locked(() => ReasonNumberOrNone(reason) ?? Remember(new NumberedFailureReason(_lastReasonNumber + 1, reason)).Number);

   /// <summary>The number of <paramref name="vocab"/>, or none when no analysis has referred to it.</summary>
   public int? VocabNumberOrNone(VocabNote vocab) => _vocabByNoteId.TryGetValue(NoteIdOf(vocab), out var numbered) ? numbered.Number : null;

   /// <summary>The number of the failure reason <paramref name="reason"/>, or none when no analysis has given it.</summary>
   public int? ReasonNumberOrNone(string reason) => _reasonByText.TryGetValue(reason, out var numbered) ? numbered.Number : null;

   /// <summary>The vocab with the number <paramref name="number"/>.</summary>
   public NumberedVocab VocabNumbered(int number) => _vocabByNumber[number];

   /// <summary>The text of the failure reason with the number <paramref name="number"/>.</summary>
   public string ReasonNumbered(int number) => _reasonByNumber[number].Text;

   /// <summary>Every vocab given a number.</summary>
   public IReadOnlyCollection<NumberedVocab> Vocab => [.. _vocabByNoteId.Values];

   /// <summary>Every failure reason given a number.</summary>
   public IReadOnlyCollection<NumberedFailureReason> Reasons => [.. _reasonByText.Values];

   /// <summary>Remembers <paramref name="vocab"/>'s number, by the number first, so that a thread finding the vocab's number can look it up.</summary>
   NumberedVocab Remember(NumberedVocab vocab)
   {
      _vocabByNumber[vocab.Number] = vocab;
      _vocabByNoteId[vocab.NoteId] = vocab;
      _lastVocabNumber = Math.Max(_lastVocabNumber, vocab.Number);
      return vocab;
   }

   /// <summary>Remembers <paramref name="reason"/>'s number, by the number first, so that a thread finding the reason's number can look it up.</summary>
   NumberedFailureReason Remember(NumberedFailureReason reason)
   {
      _reasonByNumber[reason.Number] = reason;
      _reasonByText[reason.Text] = reason;
      _lastReasonNumber = Math.Max(_lastReasonNumber, reason.Number);
      return reason;
   }

   static string NoteIdOf(VocabNote vocab) => vocab.GetId().ToString();
}
