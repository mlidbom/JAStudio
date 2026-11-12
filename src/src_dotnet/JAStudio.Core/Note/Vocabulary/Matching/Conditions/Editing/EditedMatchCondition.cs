using System;
using System.Collections.Generic;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Code;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Sharing;
using JAStudio.Core.Note.Vocabulary.Matching.Conditions.Words;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Editing;

/// <summary>
/// A vocab's match-condition as the tree editor on its Matching tab edits it: the match-condition is an "all of" group on a word already, so the tree is<br/>
/// the match-condition, and each change is stored in the vocab, which saves it.
/// </summary>
public sealed class EditedMatchCondition : IEditedCondition
{
   readonly Func<VocabNote> _currentVocab;

   /// <param name="currentVocab">Finds the vocab being edited, as the collection holds it now.</param>
   public EditedMatchCondition(Func<VocabNote> currentVocab) => _currentVocab = currentVocab;

   public IConditionLevel Level => WordConditions.Level;

   public Condition Stored => MatchingConfiguration.MatchCondition;

   public void Store(Condition tree) => MatchingConfiguration.SetMatchCondition(MatchConditionOf(tree));

   public IReadOnlyList<CodeLine> LinesOf(Condition tree) => ConditionCode.LinesOfAMatchCondition(MatchConditionOf(tree));

   /// <summary>Every shared-condition on <paramref name="level"/>'s subject: a match-condition can use any.</summary>
   public IReadOnlyList<ISharedCondition> SharedConditionsOfferedOn(IConditionLevel level) => _currentVocab().Services.SharedConditions.On(level);

   VocabNoteMatchingConfiguration MatchingConfiguration => _currentVocab().MatchingConfiguration;

   static AllOfGroup<CandidateWordVariant> MatchConditionOf(Condition tree) => ConditionGroup.AsAnAllOf(tree.AsAConditionOn<CandidateWordVariant>());
}
