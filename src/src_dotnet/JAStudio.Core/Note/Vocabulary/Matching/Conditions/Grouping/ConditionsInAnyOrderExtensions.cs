using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.Matching.Conditions.Grouping;

/// <summary>
/// Conditions taken in whatever order, as a group's are, and paired by what they mean (<see cref="Condition.MeansTheSameAs"/>): an "all of" or an<br/>
/// "any of" means the same whatever the order of its conditions.
/// </summary>
static class ConditionsInAnyOrderExtensions
{
   extension<TSubject>(IEnumerable<Condition<TSubject>> @this) where TSubject : class
   {
      /// <summary>
      /// Whether these conditions and <paramref name="others"/> are as many, and each of <paramref name="others"/> means the same as a different one of<br/>
      /// these, in whatever order.
      /// </summary>
      internal bool MeanTheSameInAnyOrderAs(IEnumerable<Condition<TSubject>> others)
      {
         List<Condition<TSubject>> these = [.. @this];
         List<Condition<TSubject>> theOthers = [.. others];
         return these.Count == theOthers.Count && these.IndexesOfConditionsMeaningTheSameAsEachOf(theOthers) is not null;
      }
   }

   extension<TSubject>(IReadOnlyList<Condition<TSubject>> @this) where TSubject : class
   {
      /// <summary>
      /// For each of <paramref name="others"/>, in their order, the index of one of these conditions that means the same, a different one for each: none<br/>
      /// when one of <paramref name="others"/> has none.
      /// </summary>
      internal IReadOnlyList<int>? IndexesOfConditionsMeaningTheSameAsEachOf(IReadOnlyList<Condition<TSubject>> others)
      {
         //review:matching: pairing conditions by meaning was ConditionGroup<TSubject>.IndexesIn, a static helper a level below the group's other
         //   members. As extensions on a group's conditions it has a home of its own, and the group reads at its own level. "InAnyOrder" for
         //   what makes them a group's conditions.
         List<int> indexes = [];
         foreach(var other in others)
         {
            var index = Enumerable.Range(0, @this.Count).FirstOrDefault(it => !indexes.Contains(it) && @this[it].MeansTheSameAs(other), -1);
            if(index == -1) return null;
            indexes.Add(index);
         }

         return indexes;
      }
   }
}
