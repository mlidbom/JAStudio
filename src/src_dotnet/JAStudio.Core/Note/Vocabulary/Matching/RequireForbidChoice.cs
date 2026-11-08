namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>What a vocab's <see cref="RequireForbidFlagField"/> says: nothing, that a match requires what the rule is about, or that it forbids it.</summary>
public enum RequireForbidChoice
{
   Unset,
   Required,
   Forbidden
}
