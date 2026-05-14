using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Note.Sentences;

public class CachingSentenceConfigurationField
{
   readonly SentenceNote _sentence;
   readonly NoteGuard _guard;

   public CachingSentenceConfigurationField(SentenceNote sentence, SentenceConfigSubData? data, NoteGuard guard)
   {
      _sentence = sentence;
      _guard = guard;
      Configuration = SentenceData.CreateConfiguration(data, () => _guard.Update(() => _sentence.UpdateParsedWords(force: true)));
   }

   public SentenceConfiguration Configuration { get; private set; }

   public WordExclusionSet IncorrectMatches => Configuration.IncorrectMatches;

   public WordExclusionSet HiddenMatches => Configuration.HiddenMatches;

   public List<string> HighlightedWords => Configuration.HighlightedWords;

   public void RemoveHighlightedWord(string word) => _guard.Update(() =>
   {
      HighlightedWords.Remove(word);
      _sentence.UpdateParsedWords(force: true);
   });

   public void ResetHighlightedWords() => _guard.Update(() =>
   {
      Configuration.HighlightedWords.Clear();
      _sentence.UpdateParsedWords(force: true);
   });

   public void AddHighlightedWord(string vocab) => _guard.Update(() =>
   {
      HighlightedWords.Add(vocab.Trim());
      _sentence.UpdateParsedWords(force: true);
   });

   [Obsolete("For testing only")]
   public void SetValueDirectlyTestsOnly(SentenceConfiguration configuration)
   {
      Configuration = configuration;
   }
}
