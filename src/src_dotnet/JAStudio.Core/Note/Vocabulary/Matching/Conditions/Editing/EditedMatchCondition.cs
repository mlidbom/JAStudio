using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Checking;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;

/// <summary>
/// A vocab's match-condition as the tree editor in its match-condition window edits it: the match-condition is an "all of" group on a word already, so<br/>
/// the tree is the match-condition, and each change is stored in the vocab, which saves it.
/// </summary>
public sealed class EditedMatchCondition : IEditedCondition
{
   readonly Func<VocabNote> _currentVocab;

   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   public EditedMatchCondition(Func<VocabNote> currentVocab) => _currentVocab = currentVocab;

   public IConditionLevel Level => WordConditions.Level;

   public Condition Stored => MatchingConfiguration.MatchCondition;

   public void Store(Condition tree) => MatchingConfiguration.SetMatchCondition(VocabNoteMatchingConfiguration.AsAMatchCondition(tree));

   public IReadOnlyList<CodeLine> LinesOf(Condition tree) => ConditionCode.LinesOfAMatchCondition(VocabNoteMatchingConfiguration.AsAMatchCondition(tree));

   /// <summary>
   /// <paramref name="tree"/>, a tree as the editor built it, checked against <paramref name="word"/>, a word in a sentence: which of its conditions the<br/>
   /// word meets, the root's as matching tests the match-condition.
   /// </summary>
   public static CheckedCondition CheckedAgainst(Condition tree, CandidateWordVariant word) => VocabNoteMatchingConfiguration.AsAMatchCondition(tree).CheckedAgainst(word);

   /// <summary>Every shared-condition on <paramref name="level"/>'s subject: a match-condition can use any.</summary>
   public IReadOnlyList<ISharedCondition> SharedConditionsOfferedOn(IConditionLevel level) => _currentVocab().Services.SharedConditions.On(level);

   VocabNoteMatchingConfiguration MatchingConfiguration => _currentVocab().MatchingConfiguration;
}
