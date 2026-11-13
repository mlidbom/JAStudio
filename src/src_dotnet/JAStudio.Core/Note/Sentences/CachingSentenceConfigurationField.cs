using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Sentences;

public class CachingSentenceConfigurationField
{
   readonly SentenceNote _sentence;
   readonly NoteGuard _guard;

   public CachingSentenceConfigurationField(SentenceNote sentence, SentenceConfigDto? data, NoteGuard guard)
   {
      _sentence = sentence;
      _guard = guard;
      Configuration = SentenceDto.CreateConfiguration(data, () => _guard.Update(() => _sentence.UpdateParsedWords(force: true)));
   }

   public SentenceConfiguration Configuration { get; private set; }

   public WordExclusionSet IncorrectMatches => Configuration.IncorrectMatches;

   public WordExclusionSet HiddenMatches => Configuration.HiddenMatches;

   public List<string> HighlightedWords => Configuration.HighlightedWords;

   /// <summary>Whether the sentence highlights <paramref name="match"/> (<see cref="SentenceConfiguration.Highlights"/>).</summary>
   public bool Highlights(Match match) => Configuration.Highlights(match);

   /// <summary>Highlights <paramref name="match"/>: its form, everywhere in the sentence, since the sentence highlights forms, not positions.</summary>
   public void Highlight(Match match) => AddHighlightedWord(match.ExclusionForm);

   /// <summary>Removes the highlight of <paramref name="match"/>: of its form, everywhere in the sentence.</summary>
   public void RemoveTheHighlightOf(Match match) => RemoveHighlightedWord(match.ExclusionForm);

   /// <summary>Removes the highlight of <paramref name="word"/>, as many times as it was added.</summary>
   public void RemoveHighlightedWord(string word) => _guard.Update(() =>
   {
      HighlightedWords.RemoveAll(it => it == word);
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
