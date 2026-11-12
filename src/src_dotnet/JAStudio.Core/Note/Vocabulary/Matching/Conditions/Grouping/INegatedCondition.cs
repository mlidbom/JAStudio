namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>A negation, <see cref="NegatedCondition{TSubject}"/>, as the tree editor sees it, whatever its subject: the editor shows it as <c>!</c> in front of what it negates.</summary>
public interface INegatedCondition
{
   /// <summary>The condition a subject must not meet.</summary>
   Condition Condition { get; }
}
