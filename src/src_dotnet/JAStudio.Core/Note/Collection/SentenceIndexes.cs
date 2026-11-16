using System.Linq;
using Compze.Teventive.StateStore;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.Note.Collection;

/// <summary>
/// What the state store files each sentence under, for <see cref="SentenceCollection"/> to look sentences up by: its question, the words of its<br/>
/// text, the vocab its parsing matched, and the words it highlights or marks as matched incorrectly. Each index has a key type of its own, as<br/>
/// <see cref="VocabIndexes"/> says.
/// </summary>
/// <remarks>review: the key types are nested here, as in <see cref="VocabIndexes"/>.</remarks>
static class SentenceIndexes
{
   /// <summary>Declares the sentences' indexes for the state store being composed.</summary>
   public static void DeclareIn(StateStoreBuilder store) => store
     .Index<SentenceNote, Question>(sentence => [new Question(sentence.GetQuestion())])
     .Index<SentenceNote, Word>(sentence => sentence.GetWords().Distinct().Select(word => new Word(word)))
     .Index<SentenceNote, MatchedVocab>(sentence => sentence.GetParsingResult().MatchedVocabIds.Distinct().Select(id => new MatchedVocab(id)))
     .Index<SentenceNote, HighlightedWord>(sentence => sentence.Configuration.HighlightedWords.Distinct().Select(word => new HighlightedWord(word)))
     .Index<SentenceNote, WordMarkedAsMatchedIncorrectly>(sentence => sentence.Configuration.IncorrectMatches.Words().Distinct().Select(word => new WordMarkedAsMatchedIncorrectly(word)));

   /// <summary>The sentence's question: its text.</summary>
   public readonly record struct Question(string Text);

   /// <summary>One of the words of the sentence's text (<see cref="SentenceNote.GetWords"/>).</summary>
   public readonly record struct Word(string Text);

   /// <summary>A vocab the sentence's parsing matched.</summary>
   public readonly record struct MatchedVocab(NoteId Vocab);

   /// <summary>A word the sentence highlights.</summary>
   public readonly record struct HighlightedWord(string Text);

   /// <summary>A word the sentence marks as matched incorrectly.</summary>
   public readonly record struct WordMarkedAsMatchedIncorrectly(string Text);
}
