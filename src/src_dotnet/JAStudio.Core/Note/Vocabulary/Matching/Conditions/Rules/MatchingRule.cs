using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

/// <summary>
/// One of the matching rules the Matching tab of a vocab offers: a condition a vocab's match-condition can hold, which the tab shows as a choice of<br/>
/// required or forbidden (<see cref="RequireForbidMatchingRule"/>), or as a list of texts (<see cref="TextsMatchingRule"/>). They're the rules a vocab<br/>
/// had before match-conditions, each now standing for the condition it became. Each is defined once, in <see cref="MatchingRules"/>.
/// </summary>
/// <remarks>
/// While each of the conditions in a vocab's match-condition is a rule's, the tab edits it rule by rule (<see cref="MatchConditionAsRules"/>). A rule's<br/>
/// condition also weighs in on the vocab's priority over another vocab matching the same word, as the rule did.
/// </remarks>
public abstract class MatchingRule
{
   protected MatchingRule(string name) => Name = name;

   /// <summary>The rule's name, such as <c>masuStem</c>: the name the corpus stored it under before match-conditions, and its editable field's.</summary>
   public string Name { get; }

   /// <summary>Whether <paramref name="condition"/>, one of the conditions of a vocab's match-condition, is this rule's.</summary>
   internal abstract bool Recognizes(Condition<CandidateWordVariant> condition);

   /// <summary>What <paramref name="condition"/>, this rule's, adds to the vocab's priority; nothing when it isn't this rule's.</summary>
   internal abstract int WeightOf(Condition<CandidateWordVariant> condition);

   public override string ToString() => Name;
}
