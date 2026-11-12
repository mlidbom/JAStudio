using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>
/// A shared-condition as <see cref="SharedConditionTools"/> reports it after using it by name wherever its definition was: as listed, and what using it<br/>
/// changed.
/// </summary>
/// <param name="SharedCondition">The shared-condition as listed after it was used, with everything that uses it now.</param>
/// <param name="VocabMadeToUseIt">The questions of the vocab whose match-condition was changed to use it, each with its disambiguation, in ordinal order.</param>
/// <param name="SharedConditionsMadeToUseIt">The full names of the shared-conditions whose definition was changed to use it.</param>
public sealed record SharedConditionPutToUse(ListedSharedCondition SharedCondition, IReadOnlyList<string> VocabMadeToUseIt, IReadOnlyList<string> SharedConditionsMadeToUseIt)
{
   /// <summary>
   /// What using a shared-condition changed, from its listing <paramref name="before"/> it was used to its listing <paramref name="after"/>: the vocab and<br/>
   /// the shared-conditions that use it after, and didn't before.
   /// </summary>
   internal static SharedConditionPutToUse Between(ListedSharedCondition before, ListedSharedCondition after) =>
      //review:matching: Between, for the change from one listing to the other. The tool worked out what used it before and after on its own,
      //   then listed it once more for the report.
      new(after, [.. after.VocabUsingIt.Except(before.VocabUsingIt)], [.. after.SharedConditionsUsingIt.Except(before.SharedConditionsUsingIt)]);
}
