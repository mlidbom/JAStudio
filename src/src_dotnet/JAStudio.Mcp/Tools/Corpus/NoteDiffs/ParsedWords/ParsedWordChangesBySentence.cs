using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;

/// <summary>
/// How one word's parsed words changed between two versions of the corpus, sentence by sentence: the sentences where it was added, removed, now<br/>
/// displayed and no longer displayed, each with what the sentence displayed before and after. What <see cref="CorpusTools.ListTheSentencesWhereAWordsParsingChanged"/><br/>
/// reports, to look into a word that <see cref="ParsedWordChangeCounts"/> counts.
/// </summary>
/// <param name="Word">The question of the vocab the word is matched to; for a word matched to no vocab the collection has, its form.</param>
/// <param name="VocabId">The id of the vocab the word is matched to, when only those matched to it were asked for.</param>
public record ParsedWordChangesBySentence(
   string Word,
   string? VocabId,
   SentencesOfOneKindOfChange Added,
   SentencesOfOneKindOfChange Removed,
   SentencesOfOneKindOfChange NowDisplayed,
   SentencesOfOneKindOfChange NoLongerDisplayed)
{
   /// <summary>
   /// The sentences of <paramref name="notes"/> where <paramref name="word"/>'s parsed words changed, matched to the vocab <paramref name="vocabId"/> if<br/>
   /// it's given, at most <paramref name="maxSentencesPerKindOfChange"/> of each kind of change, naming vocab by their questions in <paramref name="collection"/>.
   /// </summary>
   public static ParsedWordChangesBySentence Of(string word, string? vocabId, IReadOnlyList<NoteVersions> notes, JPCollection collection, int maxSentencesPerKindOfChange)
   {
      var sentences = SentencesWhoseParsedWordsChanged();
      return new ParsedWordChangesBySentence(word,
                                             vocabId,
                                             SentencesWhere(change => change.Added),
                                             SentencesWhere(change => change.Removed),
                                             SentencesWhere(change => change.NowDisplayed),
                                             SentencesWhere(change => change.NoLongerDisplayed));

      List<(SentenceNote Before, SentenceNote After, ParsedWordsChange Change)> SentencesWhoseParsedWordsChanged()
      {
         List<(SentenceNote Before, SentenceNote After, ParsedWordsChange Change)> changed = [];
         foreach(var note in notes)
         {
            if(note is { Before: SentenceNote before, After: SentenceNote after } && note.Diff(collection).ParsedWords is {} change)
               changed.Add((before, after, change));
         }

         return changed;
      }

      SentencesOfOneKindOfChange SentencesWhere(Func<ParsedWordsChange, IReadOnlyList<ParsedWordReported>> wordsOfTheKind)
      {
         var found = sentences.SelectMany(sentence => wordsOfTheKind(sentence.Change).Where(IsTheWord).Select(it => SentenceWhereTheWordChanged.Of(sentence.Before, sentence.After, it)))
                              .ToList();
         return new SentencesOfOneKindOfChange(found.Count, [.. found.Take(maxSentencesPerKindOfChange)]);
      }

      bool IsTheWord(ParsedWordReported parsed) => (parsed.Vocab ?? parsed.Form) == word && (vocabId == null || parsed.VocabId == vocabId);
   }
}
