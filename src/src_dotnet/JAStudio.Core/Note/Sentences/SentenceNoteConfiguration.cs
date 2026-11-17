using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences.Tevents;
using JAStudio.Core.Note.Tevents;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// A sentence's configuration of how it's parsed, which a person sets where the parse is wrong: the words it highlights, and its incorrect and hidden<br/>
/// matches. A change reparses the sentence.
/// </summary>
/// <remarks>
/// A component of its sentence: each of the three publishes its whole new value, <see cref="ISentenceTevent.Configuration"/>, which its appliers<br/>
/// apply.
/// </remarks>
public class SentenceNoteConfiguration : TeventiveComponent<SentenceNote, INoteTevent, NoteTevent, SentenceNoteConfiguration, ISentenceTevent.Configuration, SentenceTevent.Configuration>, ITeventRouted
{
   static readonly TeventRouting<SentenceNoteConfiguration> Appliers = TeventRouting<SentenceNoteConfiguration>.DeclareAppliers(it => it
     .For<ISentenceTevent.Configuration.PropertyUpdated.HighlightedWords>(static (configuration, tevent) => configuration.HighlightedWords = tevent.HighlightedWords)
     .For<ISentenceTevent.Configuration.PropertyUpdated.IncorrectMatches>(static (configuration, tevent) => configuration.IncorrectMatches.Apply(tevent.IncorrectMatches))
     .For<ISentenceTevent.Configuration.PropertyUpdated.HiddenMatches>(static (configuration, tevent) => configuration.HiddenMatches.Apply(tevent.HiddenMatches)));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly SentenceNote _sentence;

   /// <param name="sentence">The sentence it's the configuration of, which it reparses after a change.</param>
   /// <param name="data">The configuration as the corpus stores it, or null for a new sentence.</param>
   internal SentenceNoteConfiguration(SentenceNote sentence, SentenceConfigDto? data) : base(sentence)
   {
      _sentence = sentence;
      HighlightedWords = [.. data?.HighlightedWords ?? []];
      IncorrectMatches = new WordExclusionSet(ExclusionsIn(data?.IncorrectMatches),
                                              (set, exclusions) => SetTheExclusions(set, exclusions, static it => new SentenceTevent.Configuration.IncorrectMatchesUpdated(it)));
      HiddenMatches = new WordExclusionSet(ExclusionsIn(data?.HiddenMatches),
                                           (set, exclusions) => SetTheExclusions(set, exclusions, static it => new SentenceTevent.Configuration.HiddenMatchesUpdated(it)));
   }

   /// <summary>The configuration as analyzing the sentence takes it into account.</summary>
   public SentenceConfiguration Configuration => new(HighlightedWords, IncorrectMatches, HiddenMatches);

   /// <summary>The forms the sentence highlights, each everywhere in the sentence, in the order they were added: a form added twice is here twice.</summary>
   public IReadOnlyList<string> HighlightedWords { get; private set; }

   public WordExclusionSet IncorrectMatches { get; }

   public WordExclusionSet HiddenMatches { get; }

   /// <summary>Whether the sentence highlights <paramref name="match"/> (<see cref="SentenceConfiguration.Highlights"/>).</summary>
   public bool Highlights(Match match) => Configuration.Highlights(match);

   /// <summary>Highlights <paramref name="match"/>: its form, everywhere in the sentence, since the sentence highlights forms, not positions.</summary>
   public void Highlight(Match match) => AddHighlightedWord(match.ExclusionForm);

   /// <summary>Removes the highlight of <paramref name="match"/>: of its form, everywhere in the sentence.</summary>
   public void RemoveTheHighlightOf(Match match) => RemoveHighlightedWord(match.ExclusionForm);

   /// <summary>Removes the highlight of <paramref name="word"/>, as many times as it was added.</summary>
   public void RemoveHighlightedWord(string word) => SetTheHighlightedWords([.. HighlightedWords.Where(it => it != word)]);

   public void ResetHighlightedWords() => SetTheHighlightedWords([]);

   /// <summary>Highlights <paramref name="vocab"/>, trimmed, after the words highlighted already: once more if it's among them.</summary>
   public void AddHighlightedWord(string vocab) => SetTheHighlightedWords([.. HighlightedWords, vocab.Trim()]);

   void SetTheHighlightedWords(IReadOnlyList<string> highlightedWords)
   {
      if(_sentence.PublishIfTheListChanged(HighlightedWords, highlightedWords, static it => new SentenceTevent.Configuration.HighlightedWordsUpdated(it)))
         ReparseTheSentence();
   }

   /// <summary>Publishes the tevent <paramref name="teventOf"/> makes of <paramref name="exclusions"/>, the new exclusions of <paramref name="exclusionSet"/>, and reparses the sentence, unless the set holds them already.</summary>
   void SetTheExclusions(WordExclusionSet exclusionSet, IReadOnlySet<WordExclusion> exclusions, Func<IReadOnlySet<WordExclusion>, NoteTevent> teventOf)
   {
      if(_sentence.PublishIfTheSetChanged(exclusionSet.Get(), exclusions, teventOf)) ReparseTheSentence();
   }

   void ReparseTheSentence() => _sentence.UpdateParsedWords(force: true);

   static IEnumerable<WordExclusion> ExclusionsIn(List<WordExclusionDto>? data) => (data ?? []).Select(it => it.ToExclusion());
}
