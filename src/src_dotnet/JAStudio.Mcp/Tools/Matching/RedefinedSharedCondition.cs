using System.Collections.Generic;

namespace JAStudio.Mcp.Tools.Matching;

/// <summary>A shared-condition as <see cref="SharedConditionTools"/> reports it after giving it another definition: as listed, and whose sentences were reparsed.</summary>
/// <param name="SharedCondition">The shared-condition as listed with its new definition.</param>
/// <param name="VocabWhoseSentencesWereReparsed">
/// The questions of the vocab whose match-conditions depend on its definition, directly or through other shared-conditions, each with its disambiguation,<br/>
/// in ordinal order: those whose sentences were reparsed.
/// </param>
public sealed record RedefinedSharedCondition(ListedSharedCondition SharedCondition, IReadOnlyList<string> VocabWhoseSentencesWereReparsed);
