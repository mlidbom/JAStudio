using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching.Checking;

/// <summary>
/// The word in a sentence that a vocab's match-condition is checked against, in its match-condition window (<see cref="MatchConditionWindowViewModel"/>):<br/>
/// the sentence typed, the words in it that are the vocab's forms, each with the vocab's match of it, and the word picked among them. The tree is<br/>
/// checked against the picked word, so that each line shows whether the word meets its condition, and so which conditions decide whether it matches.
/// </summary>
sealed partial class WordToCheckAgainstViewModel : ObservableObject
{
   readonly AnalysisServices _analysisServices;
   readonly Func<VocabNote> _currentVocab;
   readonly Action<CandidateWordVariant?> _checkTheTreeAgainst;

   /// <param name="analysisServices">Analyzes the sentence, as JAStudio parses sentences.</param>
   /// <param name="currentVocab">Finds the vocab whose match-condition is checked, as the collection holds it now.</param>
   /// <param name="checkTheTreeAgainst">Checks the tree against the word picked from now on, or stops checking it when none is.</param>
   public WordToCheckAgainstViewModel(AnalysisServices analysisServices, Func<VocabNote> currentVocab, Action<CandidateWordVariant?> checkTheTreeAgainst)
   {
      _analysisServices = analysisServices;
      _currentVocab = currentVocab;
      _checkTheTreeAgainst = checkTheTreeAgainst;
   }

   /// <summary>The sentence typed, which the <see cref="Words"/> are found in as it's typed.</summary>
   [ObservableProperty] public partial string Sentence { get; set; } = "";

   /// <summary>The words in the <see cref="Sentence"/> that are the vocab's forms, each with the vocab's match of it, valid or not.</summary>
   [ObservableProperty]
   [NotifyPropertyChangedFor(nameof(HasWords), nameof(NoWordIsOneOfTheVocabsForms))]
   public partial IReadOnlyList<WordInTheSentenceViewModel> Words { get; set; } = [];

   /// <summary>The word the tree is checked against: the first of the <see cref="Words"/> until another is picked, and none while there are none.</summary>
   [ObservableProperty] public partial WordInTheSentenceViewModel? Picked { get; set; }

   public bool HasWords => Words.Count > 0;

   /// <summary>Whether a sentence is typed, and no word in it is one of the vocab's forms, as JAStudio splits it into words.</summary>
   public bool NoWordIsOneOfTheVocabsForms => Words.Count == 0 && !string.IsNullOrWhiteSpace(Sentence);

   /// <summary>
   /// Finds the words of <paramref name="vocab"/> in the <see cref="Sentence"/> anew, as the vocab is now, such as after a change to its match-condition<br/>
   /// was saved and its sentences reparsed, keeping the same word picked.
   /// </summary>
   public void ShowTheWordsOf(VocabNote vocab) => ShowTheWordsIn(Sentence, vocab);

   partial void OnSentenceChanged(string value) => ShowTheWordsIn(value, _currentVocab());

   /// <summary>Lists the words of <paramref name="vocab"/> in <paramref name="sentence"/>, keeping the same word picked if it's still there.</summary>
   void ShowTheWordsIn(string sentence, VocabNote vocab)
   {
      var picked = Picked;
      Words = string.IsNullOrWhiteSpace(sentence) ? [] : [.. TextAnalysis.FromText(_analysisServices, sentence).MatchesOf(vocab).Select(it => new WordInTheSentenceViewModel(it))];
      Picked = Words.FirstOrDefault(it => picked is not null && it.IsTheSameWordAs(picked)) ?? Words.FirstOrDefault();
   }

   partial void OnPickedChanged(WordInTheSentenceViewModel? value) => _checkTheTreeAgainst(value?.Word);
}
