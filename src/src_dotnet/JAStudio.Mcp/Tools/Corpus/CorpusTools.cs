using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Storage;
using JAStudio.Mcp.Corpus;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Corpus;

/// <summary>The tools about the corpus checkout a JAStudio works on, as git sees it.</summary>
[McpServerToolType]
public class CorpusTools
{
   /// <summary>How many notes of each type <see cref="ListChangedNotes"/> lists unless told otherwise, so that a change to most of the corpus doesn't flood Claude's context.</summary>
   internal const int NotesListedPerTypeByDefault = 20;

   /// <summary>How many words <see cref="DiffTheChangedNotes"/> lists the parsed words changes of unless told otherwise.</summary>
   const int WordsListedByDefault = 30;

   /// <summary>How many sentences of each kind of change <see cref="ListTheSentencesWhereAWordsParsingChanged"/> lists unless told otherwise.</summary>
   const int SentencesListedPerKindOfChangeByDefault = 10;

   /// <summary>The name <see cref="DiffTheChangedNotes"/> gives the working tree, the checkout's files as they are now, as the version compared to.</summary>
   const string TheWorkingTree = "the working tree";

   readonly JPCollection _collection;
   readonly CorpusCheckout _corpusCheckout;
   readonly NoteSerializer _noteSerializer;

   public CorpusTools(JPCollection collection, CorpusCheckout corpusCheckout, NoteSerializer noteSerializer)
   {
      _collection = collection;
      _corpusCheckout = corpusCheckout;
      _noteSerializer = noteSerializer;
   }

   [McpServerTool(Name = "list_changed_notes", ReadOnly = true, Idempotent = true)]
   [Description("""
                Lists the notes whose files in the corpus checkout differ from the checkout's git HEAD, changed, added or deleted, by note type:
                what changes to the notes since the last commit changed, such as a full rebuild. Counts every changed note, and lists the
                first of each type by question. Files of notes the collection doesn't have, such as deleted ones, are listed by note id.
                diff_the_changed_notes shows how each note changed.
                """)]
   public ChangedNotes ListChangedNotes(
      [Description("The most notes of each type to list. Every changed note is counted either way.")] int maxNotesListedPerType = NotesListedPerTypeByDefault) =>
      ChangedNotes.In(_collection, _corpusCheckout.IdsOfTheNotesWhoseFilesDifferFromHead(), maxNotesListedPerType);

   [McpServerTool(Name = "diff_the_changed_notes", ReadOnly = true, Idempotent = true)]
   [Description("""
                Shows how the notes whose files differ between two versions of the corpus differ as notes, rather than as files: each version of
                a note's file is read into a note, as JAStudio reads notes, and the two notes compared property by property, as JAStudio stores
                them. By default it compares the checkout's git HEAD with its files as they are now: what changes to the notes since the last
                commit changed, such as a full rebuild. Reports, by note type, how many notes were changed, added and deleted, how many notes
                each property changed in, and the diffs of the first notes by question: each value changed, before and after, and the items
                added to and removed from each list. A sentence's parsed words are compared as words, each with the vocab it's matched to:
                words added, removed, now displayed and no longer displayed. Those changes are also counted over all the sentences, word by
                word, the words that changed most first: how a change to parsing played out across the corpus.
                """)]
   public NotesDiff DiffTheChangedNotes(
      [Description("The version to compare from: a commit, by its hash or the name of a branch at it. The checkout's HEAD unless given.")] string from = "HEAD",
      [Description("The version to compare to: a commit, by its hash or the name of a branch at it. The checkout's files as they are now unless given.")]
      string? to = null,
      [Description("The most notes of each type to show the diffs of. Every note is counted either way.")] int maxNotesListedPerType = NotesListedPerTypeByDefault,
      [Description("The most words to list the changes of the parsed words of. Every word's changes are counted either way.")] int maxWordsListed = WordsListedByDefault)
   {
      var (commitFrom, commitTo) = TheVersions(from, to);
      return NotesDiff.Of(commitFrom.Name, commitTo?.Name ?? TheWorkingTree, NotesThatDifferBetween(commitFrom, commitTo), _collection, maxNotesListedPerType, maxWordsListed);
   }

   [McpServerTool(Name = "list_the_sentences_where_a_words_parsing_changed", ReadOnly = true, Idempotent = true)]
   [Description("""
                Lists the sentences where one word's parsed words changed between two versions of the corpus, to look into a word that
                diff_the_changed_notes counts: the sentences where the word was added, removed, now displayed and no longer displayed, each
                with the word as it changed and the words the sentence displayed before and after. Counts every change either way.
                """)]
   public ParsedWordChangesBySentence ListTheSentencesWhereAWordsParsingChanged(
      [Description("The word as diff_the_changed_notes lists it: the question of the vocab it's matched to, or for a word matched to no vocab, its form.")] string word,
      [Description("The id of the vocab the word is matched to, to tell apart vocab with the same question. Every word listed as the word unless given.")]
      string? vocabId = null,
      [Description("The version to compare from: a commit, by its hash or the name of a branch at it. The checkout's HEAD unless given.")] string from = "HEAD",
      [Description("The version to compare to: a commit, by its hash or the name of a branch at it. The checkout's files as they are now unless given.")]
      string? to = null,
      [Description("The most sentences of each kind of change to list.")] int maxSentencesPerKindOfChange = SentencesListedPerKindOfChangeByDefault)
   {
      var (commitFrom, commitTo) = TheVersions(from, to);
      return ParsedWordChangesBySentence.Of(word, vocabId, NotesThatDifferBetween(commitFrom, commitTo), _collection, maxSentencesPerKindOfChange);
   }

   /// <summary>The versions named <paramref name="from"/> and <paramref name="to"/>: <paramref name="to"/> is null for the working tree.</summary>
   (CorpusCommit From, CorpusCommit? To) TheVersions(string from, string? to) =>
      (_corpusCheckout.ByItsHash(new CorpusCommit(from)), to == null ? null : _corpusCheckout.ByItsHash(new CorpusCommit(to)));

   /// <summary>Each note whose file differs between <paramref name="from"/> and <paramref name="to"/>, or the working tree if it's null, read in both versions.</summary>
   IReadOnlyList<NoteVersions> NotesThatDifferBetween(CorpusCommit from, CorpusCommit? to)
   {
      var noteFiles = to == null
                         ? _corpusCheckout.NoteFilesThatDifferFromTheWorkingTree(from)
                         : _corpusCheckout.NoteFilesThatDifferBetween(from, to);
      return [.. noteFiles.Select(it => NoteVersions.Read(it, _noteSerializer))];
   }
}
