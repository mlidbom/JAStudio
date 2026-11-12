using System.Collections.Generic;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions;

/// <summary>
/// Compares conditions by what they mean (<see cref="Condition.MeansTheSameAs"/>), not by how they're built, so that conditions can be kept and counted<br/>
/// by what they mean, such as in a dictionary: a group's conditions in any order, and a shared-condition or its definition, count as one.
/// </summary>
public sealed class ConditionMeaningComparer : IEqualityComparer<Condition>
{
   ConditionMeaningComparer() {}

   /// <summary>The one comparer, which has no state.</summary>
   public static ConditionMeaningComparer Instance { get; } = new();

   public bool Equals(Condition? condition, Condition? other) => condition is null ? other is null : other is not null && condition.MeansTheSameAs(other);

   public int GetHashCode(Condition condition) => condition.HashCodeOfItsMeaning();
}
