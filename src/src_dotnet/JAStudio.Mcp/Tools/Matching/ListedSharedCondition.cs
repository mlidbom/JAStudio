using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>A shared-condition as <see cref="SharedConditionTools"/> lists it: its full name, its definition as code, and what uses it.</summary>
/// <param name="FullName">Its name with its subject's in front, such as <c>Word.FollowsAPastTenseStem</c>.</param>
/// <param name="Code">Its definition as code, under the line naming it, as the match-condition editor shows it.</param>
/// <param name="VocabUsingIt">
/// The questions of the vocab whose match-condition uses it, in ordinal order, each with its disambiguation, such as <c>んだ:explanatory</c>, so that two<br/>
/// vocab with the same text are told apart.
/// </param>
/// <param name="SharedConditionsUsingIt">The full names of the shared-conditions whose definition uses it.</param>
public sealed record ListedSharedCondition(string FullName, string Code, IReadOnlyList<string> VocabUsingIt, IReadOnlyList<string> SharedConditionsUsingIt);
