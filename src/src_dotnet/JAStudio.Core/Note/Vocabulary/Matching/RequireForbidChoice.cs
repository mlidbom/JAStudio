namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// What a vocab says about a require/forbid rule, a <see cref="RequireForbidFlagField"/> or a <see cref="Conditions.Rules.RequireForbidMatchingRule"/>:<br/>
/// nothing, that a match requires what the rule is about, or that it forbids it.
/// </summary>
public enum RequireForbidChoice
{
   Unset,
   Required,
   Forbidden
}
