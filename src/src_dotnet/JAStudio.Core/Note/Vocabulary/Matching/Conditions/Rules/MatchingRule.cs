using JAStudio.Core.LanguageServices.Parsing.WordExtraction;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Rules;

/// <summary>
/// One of the matching rules a vocab had before match-conditions, standing for the condition it became: required or forbidden<br/>
/// (<see cref="RequireForbidMatchingRule"/>), or given texts (<see cref="TextsMatchingRule"/>). Each is defined once, in <see cref="MatchingRules"/>.
/// </summary>
/// <remarks>
/// A vocab's match-condition is edited as a tree now, and the rules remain for what JAStudio reads off them: a condition of a vocab's match-condition<br/>
/// that is a rule's weighs in on the vocab's priority over another vocab matching the same word, as the rule did, until priority is designed for<br/>
/// conditions (<see cref="VocabNoteMatchingConfiguration.CustomRequirementsWeight"/>). And some say something about the vocab, such as whether it<br/>
/// requires its surface (<see cref="VocabNoteMatchingConfiguration.MatchConditionRequires"/>).
/// </remarks>
public abstract class MatchingRule
{
   protected MatchingRule(string name) => Name = name;

   /// <summary>The rule's name, such as <c>masuStem</c>: the name the corpus stored it under before match-conditions.</summary>
   public string Name { get; }

   /// <summary>Whether <paramref name="condition"/>, one of the conditions of a vocab's match-condition, is this rule's: means the same as its condition (<see cref="Condition.MeansTheSameAs"/>).</summary>
   internal abstract bool Recognizes(Condition<CandidateWordVariant> condition);

   /// <summary>What <paramref name="condition"/>, this rule's, adds to the vocab's priority; nothing when it isn't this rule's.</summary>
   internal abstract int WeightOf(Condition<CandidateWordVariant> condition);

   public override string ToString() => Name;
}
